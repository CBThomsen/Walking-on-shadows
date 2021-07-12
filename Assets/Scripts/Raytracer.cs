using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class Raytracer : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader edgeDetection;
    public ComputeShader optimizeEdges;

    public SceneGeometry sceneGeometry;
    public SpaceConverter spaceConverter;

    private ComputeBuffer lightBuffer;
    private ComputeBuffer circleBuffer;
    private ComputeBuffer edgeVertexBuffer;

    private List<EdgeVertex> edgeVertices;
    private List<EdgeVertex> corners;

    private int raytracerKI;
    private int edgeDetectionKI;
    private int optimizeEdgesKI;
    public RenderTexture destTexture;
    private RenderTexture edgePositionTexture;

    private float cpuTimer = 0f;
    private float cpuFPS = 1f;

    private int textureResolution = 1024;

    void Start()
    {
        cpuTimer = 1f / cpuFPS;
        raytracerKI = computeShader.FindKernel("Raytracer");
        edgeDetectionKI = edgeDetection.FindKernel("EdgeDetection");
        optimizeEdgesKI = optimizeEdges.FindKernel("OptimizeEdges");

        SetupRenderTexture();
    }

    private void onDestroy()
    {
    }

    private void SetupRenderTexture()
    {
        destTexture = new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.ARGB32);
        destTexture.enableRandomWrite = true;
        destTexture.Create();

        //Contains accurate edge positions X, Y in R, G channels rather than rounded pixels
        edgePositionTexture = new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.RGFloat);
        edgePositionTexture.enableRandomWrite = true;
        edgePositionTexture.Create();

        computeShader.SetTexture(raytracerKI, "destTexture", destTexture);
        computeShader.SetTexture(raytracerKI, "edgePositionTexture", edgePositionTexture);

        edgeDetection.SetTexture(edgeDetectionKI, "inputTexture", destTexture);

        optimizeEdges.SetTexture(optimizeEdgesKI, "inputTexture", destTexture);
        optimizeEdges.SetTexture(raytracerKI, "edgePositionTexture", edgePositionTexture);

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(destTexture, dest);
    }

    private void SetRaytracingBuffers()
    {
        LightData[] lightDataArray = this.sceneGeometry.GetLightDataArray();
        lightBuffer = new ComputeBuffer(lightDataArray.Length, lightDataArray.Length * 2 * sizeof(float));
        lightBuffer.SetData(lightDataArray);
        computeShader.SetBuffer(raytracerKI, "lights", lightBuffer);

        CircleData[] circleDataArray = this.sceneGeometry.GetCircleDatas();
        circleBuffer = new ComputeBuffer(circleDataArray.Length, circleDataArray.Length * 3 * sizeof(float));
        circleBuffer.SetData(circleDataArray);
        computeShader.SetBuffer(raytracerKI, "circles", circleBuffer);

        edgeVertexBuffer = new ComputeBuffer(2000, 4 * sizeof(float), ComputeBufferType.Append);
        edgeVertexBuffer.SetCounterValue(0);
        computeShader.SetBuffer(raytracerKI, "edgeVertices", edgeVertexBuffer);
    }

    private EdgeVertex[] GetEdgeVertices()
    {
        int edgeVertexCount = this.GetAppendBufferCount(edgeVertexBuffer);
        EdgeVertex[] edgeVertices = new EdgeVertex[edgeVertexCount];
        edgeVertexBuffer.GetData(edgeVertices);

        return edgeVertices;
    }

    private List<EdgeVertex> SortEdgeVerticesByAngle()
    {
        //Calculate centroid
        Vector2 centroid = new Vector2(0, 0);
        float verticesCount = (float)edgeVertices.Count;

        for (var i = 0; i < edgeVertices.Count; i++)
        {
            centroid.x += edgeVertices[i].position.x / verticesCount;
            centroid.y += edgeVertices[i].position.y / verticesCount;
        }

        //Order by angle to centroid
        edgeVertices = edgeVertices.OrderBy(v =>
        {
            Vector2 distToCenter = v.position - centroid;
            float angleToCenter = Mathf.Atan2(distToCenter.y, distToCenter.x) + 2 * Mathf.PI % (2 * Mathf.PI);
            return angleToCenter;
        }).ToList();

        return edgeVertices;
    }

    private List<EdgeVertex> FindCorners()
    {
        List<EdgeVertex> corners = new List<EdgeVertex>();
        float lastAngle = 0f;

        for (var i = 1; i < edgeVertices.Count + 1; i++)
        {
            Vector2 lastPos = new Vector2(edgeVertices[i - 1].position.x, edgeVertices[i - 1].position.y);
            Vector2 curPos = new Vector2(edgeVertices[i % (edgeVertices.Count - 1)].position.x, edgeVertices[i % (edgeVertices.Count - 1)].position.y);

            Vector2 deltaPos = curPos - lastPos;
            float angle = Mathf.Atan2(deltaPos.y, deltaPos.x);

            var deltaAngle = lastAngle - angle;

            //Debug.Log("i = " + i + " angle = " + angle + " pos = " + curPos.x + ", " + curPos.y + " roundedPos = " + edgeVertices[i].roundedPosition + " change in angle = " + deltaAngle);

            if (Mathf.Abs(deltaAngle) * 180 / Mathf.PI > 1f)
            {
                corners.Add(edgeVertices[i - 1]);
            }

            lastAngle = angle;
        }

        return corners;
    }

    void Update()
    {
        SetRaytracingBuffers();
        computeShader.Dispatch(raytracerKI, textureResolution / 32, textureResolution / 32, 1);

        edgeVertices = GetEdgeVertices().ToList();
        edgeVertices = SortEdgeVerticesByAngle();

        corners = FindCorners();

        for (var i = 1; i < corners.Count; i++)
        {
            Debug.DrawLine(spaceConverter.TextureToWorldSpace(corners[i - 1].position), spaceConverter.TextureToWorldSpace(corners[i].position), Color.magenta, Time.deltaTime);
        }

        ReleaseBuffers();
    }

    private void OnDrawGizmos()
    {
        if (edgeVertices == null)
            return;

        for (var i = 1; i < edgeVertices.Count; i++)
        {
            var lastPos = new Vector2(edgeVertices[i - 1].position.x, edgeVertices[i - 1].position.y);
            var curPos = new Vector2(edgeVertices[i].position.x, edgeVertices[i].position.y);

            //Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(lastPos), spaceConverter.TextureToWorldSpace(curPos));
        }

        Debug.Log(corners.Count);

        for (var i = 1; i < corners.Count; i++)
        {
            Gizmos.DrawSphere(spaceConverter.TextureToWorldSpace(corners[i].position), 0.1f);
            //Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(corners[i - 1].position), spaceConverter.TextureToWorldSpace(corners[i].position));
        }

        Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(corners[corners.Count - 1].position), spaceConverter.TextureToWorldSpace(corners[0].position));

    }

    private void ReleaseBuffers()
    {
        circleBuffer.Release();
        lightBuffer.Release();
        edgeVertexBuffer.Release();
    }

    public void saveCSV()
    {
        using (var w = new StreamWriter("./test.csv"))
        {
            for (var i = 1; i < edgeVertices.Count; i++)
            {
                Vector2 lastPos = new Vector2(edgeVertices[i - 1].position.x, edgeVertices[i - 1].position.y);
                Vector2 curPos = new Vector2(edgeVertices[i].position.x, edgeVertices[i].position.y);

                Vector2 deltaPos = curPos - lastPos;
                float angle = Mathf.Atan2(deltaPos.y, deltaPos.x);

                var line = string.Format("{0},{1},{2},{3}", i, angle, deltaPos.x, deltaPos.y);
                w.WriteLine(line);
                w.Flush();
            }
        }

        Debug.Log("FILE WRITTEN!");
    }

    private int GetAppendBufferCount(ComputeBuffer appendBuffer)
    {
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);

        // always size 1 -- Debug.Log("Copy buffer : " + countBuffer.count);
        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);
        countBuffer.Dispose();
        return counter[0];
    }
}