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

    private int textureResolution = 128;

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

        edgeVertexBuffer = new ComputeBuffer(20000, 2 * sizeof(float), ComputeBufferType.Append);
        edgeVertexBuffer.SetCounterValue(0);
        computeShader.SetBuffer(raytracerKI, "edgeVertices", edgeVertexBuffer);
    }

    private EdgeVertex[] GetEdgeVertices()
    {
        int edgeVertexCount = this.GetAppendBufferCount(edgeVertexBuffer);
        EdgeVertex[] edgeVertices = new EdgeVertex[edgeVertexCount];
        edgeVertexBuffer.GetData(edgeVertices);

        Debug.Log("Edge vertex count= " + edgeVertexCount);

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
        var lastSlope = 0f;

        for (var i = 1; i < edgeVertices.Count; i++)
        {
            var lastPos = new Vector2(edgeVertices[i - 1].position.x, edgeVertices[i - 1].position.y);
            var curPos = new Vector2(edgeVertices[i].position.x, edgeVertices[i].position.y);

            var slope = (curPos.y - lastPos.y) / (curPos.x - lastPos.x);
            var deltaSlope = lastSlope - slope;

            if (Mathf.Abs(deltaSlope) > 1f)
            {
                corners.Add(edgeVertices[i - 1]);
            }

            lastSlope = slope;
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

            Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(lastPos), spaceConverter.TextureToWorldSpace(curPos));
        }

        for (var i = 1; i < corners.Count; i++)
        {
            Gizmos.DrawSphere(spaceConverter.TextureToWorldSpace(corners[i].position), 0.1f);
            //Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(corners[i - 1].position), spaceConverter.TextureToWorldSpace(corners[i].position));
        }
    }

    private void ReleaseBuffers()
    {
        circleBuffer.Release();
        lightBuffer.Release();
        edgeVertexBuffer.Release();
    }

    private void RefactorThis()
    {
        /*
        //Edge detection
        edgeVertexBuffer = new ComputeBuffer(2000, 5 * sizeof(float) + 6 * sizeof(int), ComputeBufferType.Append);
        edgeVertexBuffer.SetCounterValue(0);
        edgeDetection.SetBuffer(edgeDetectionKI, "edgeVertices", edgeVertexBuffer);

        edgeDetection.Dispatch(edgeDetectionKI, textureResolution / 32, textureResolution / 32, 1);

        int edgeVertexCount = this.GetAppendBufferCount(edgeVertexBuffer);
        EdgeVertex[] shadowVertices = new EdgeVertex[edgeVertexCount];
        edgeVertexBuffer.GetData(shadowVertices);

        v = shadowVertices.ToList();

        //Optimize edges
        optimizeEdges.SetBuffer(optimizeEdgesKI, "edgeVertices", edgeVertexBuffer);
        optimizeEdges.SetFloats("center", new float[2] { centroid.x, centroid.y });

        optimizeEdges.Dispatch(optimizeEdgesKI, Mathf.CeilToInt((float)edgeVertexCount / 32f), 1, 1);

        edgeVertexBuffer.GetData(shadowVertices);
        v = shadowVertices.ToList();
        v = v.OrderByDescending(vert => vert.debugstuff.x).ToList();

        List<EdgeVertex> corners = new List<EdgeVertex>();
        var lastSlope = 0f;

        for (var i = 1; i < v.Count; i++)
        {
            //Subtracting acc positions is more accurate/easier way to get slope!
            var lastAccPos = new Vector2(v[i - 1].debugstuff.z, v[i - 1].debugstuff.w);
            var curAccPos = new Vector2(v[i].debugstuff.z, v[i].debugstuff.w);
            var slope = (curAccPos.y - lastAccPos.y) / (curAccPos.x - lastAccPos.x);
            var deltaSlope = lastSlope - slope;    //Mathf.Abs(v[i].slope) - Mathf.Abs(v[i - 1].slope);

            if (Mathf.Abs(deltaSlope) > 1f)
            {
                corners.Add(v[i - 1]);
            }

            Debug.DrawLine(spaceConverter.TextureToWorldSpace(lastAccPos), spaceConverter.TextureToWorldSpace(curAccPos), Color.green, Time.deltaTime);

            lastSlope = slope;
        }

        corners.ForEach(c =>
        {
            Debug.DrawLine(spaceConverter.TextureToWorldSpace(new Vector2(c.debugstuff.z, c.debugstuff.w)), spaceConverter.TextureToWorldSpace(new Vector2(c.debugstuff.z, c.debugstuff.w)) + new Vector2(0f, 0.2f), Color.blue, Time.deltaTime);
        });

        circleBuffer.Release();
        lightBuffer.Release();
        edgeVertexBuffer.Release();
        //verticesBuffer.Release();*/
    }

    public void saveCSV()
    {
        using (var w = new StreamWriter("./test.csv"))
        {
            for (var i = 1; i < edgeVertices.Count; i++)
            {
                //var deltaSlope = v[i].slope - v[i - 1].slope;
                //var line = string.Format("{0},{1},{2}", i, v[i].slope, deltaSlope);
                //w.WriteLine(line);
                //w.Flush();
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