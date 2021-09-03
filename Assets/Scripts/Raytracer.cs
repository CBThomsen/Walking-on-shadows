using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class Raytracer : MonoBehaviour
{
    public ComputeShader computeShader;

    public SceneGeometry sceneGeometry;
    public SpaceConverter spaceConverter;
    public ShadowColliders shadowColliders;
    public ShadowRenderer shadowRenderer;

    private ComputeBuffer lightBuffer;
    private ComputeBuffer circleBuffer;
    private ComputeBuffer boxBuffer;
    private ComputeBuffer edgeVertexBuffer;
    private List<EdgeVertex> edgeVertices;
    private List<List<EdgeVertex>> shapeEdgeVertices;

    private int raytracerKI;
    public RenderTexture destTexture;
    public static int textureResolution = 1024;

    void Start()
    {
        raytracerKI = computeShader.FindKernel("Raytracer");

        SetupRenderTexture();
        shadowRenderer.SetTexture(destTexture);
    }

    private void onDestroy()
    {
    }

    private void SetupRenderTexture()
    {
        destTexture = new RenderTexture(textureResolution, textureResolution, 32, RenderTextureFormat.ARGB32);
        destTexture.enableRandomWrite = true;
        destTexture.Create();

        computeShader.SetTexture(raytracerKI, "destTexture", destTexture);
    }

    private void SetRaytracingBuffers()
    {
        LightData[] lightDataArray = this.sceneGeometry.GetLightDataArray();
        lightBuffer = new ComputeBuffer(lightDataArray.Length, lightDataArray.Length * 3 * sizeof(float));
        lightBuffer.SetData(lightDataArray);
        computeShader.SetBuffer(raytracerKI, "lights", lightBuffer);

        CircleData[] circleDataArray = this.sceneGeometry.GetCircleDatas();
        if (circleDataArray.Length > 0)
        {
            circleBuffer = new ComputeBuffer(circleDataArray.Length, circleDataArray.Length * 3 * sizeof(float));
            circleBuffer.SetData(circleDataArray);
            computeShader.SetBuffer(raytracerKI, "circles", circleBuffer);
        }

        BoxData[] boxDataArray = this.sceneGeometry.GetBoxDatas();
        boxBuffer = new ComputeBuffer(boxDataArray.Length, boxDataArray.Length * 4 * sizeof(float));
        boxBuffer.SetData(boxDataArray);
        computeShader.SetBuffer(raytracerKI, "boxes", boxBuffer);

        edgeVertexBuffer = new ComputeBuffer(20000, 4 * sizeof(float) + sizeof(int), ComputeBufferType.Append);
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

    private List<List<EdgeVertex>> SortEdgeVerticesByAngle()
    {
        List<Vector2> centroids = new List<Vector2>();
        shapeEdgeVertices = new List<List<EdgeVertex>>();

        //  lculate centroids for each shape
        for (var i = 0; i < edgeVertices.Count; i++)
        {
            int shapeIndex = edgeVertices[i].shapeIndex;

            while (shapeIndex > centroids.Count - 1)
            {
                centroids.Add(new Vector2(0f, 0f));
                shapeEdgeVertices.Add(new List<EdgeVertex>());
            }

            Vector2 c = centroids[shapeIndex];
            c += edgeVertices[i].position;

            centroids[shapeIndex] = c;
            shapeEdgeVertices[shapeIndex].Add(edgeVertices[i]);
        }

        for (var j = 0; j < shapeEdgeVertices.Count; j++)
        {
            Vector2 centroid = centroids[j] / (float)shapeEdgeVertices[j].Count;

            //Order by angle to centroid
            shapeEdgeVertices[j] = shapeEdgeVertices[j].OrderBy(v =>
            {
                Vector2 distToCenter = v.position - centroid;
                float angleToCenter = Mathf.Atan2(distToCenter.y, distToCenter.x) + 2 * Mathf.PI % (2 * Mathf.PI);
                return angleToCenter;
            }).ToList();
        }

        return shapeEdgeVertices;
    }

    private List<EdgeVertex> FindCorners(List<EdgeVertex> ev)
    {
        List<EdgeVertex> corners = new List<EdgeVertex>();
        float lastAngle = 0f;

        for (var i = 1; i < ev.Count + 1; i++)
        {
            Vector2 lastPos = new Vector2(ev[i - 1].position.x, ev[i - 1].position.y);
            Vector2 curPos = new Vector2(ev[i % (ev.Count - 1)].position.x, ev[i % (ev.Count - 1)].position.y);

            Vector2 deltaPos = curPos - lastPos;
            float angle = (Mathf.Atan2(deltaPos.y, deltaPos.x) + Mathf.PI) % (Mathf.PI);

            var deltaAngle = lastAngle - angle;

            if (Mathf.Abs(deltaAngle) * 180 / Mathf.PI > 1f)
            {
                corners.Add(ev[i - 1]);
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
        shapeEdgeVertices = SortEdgeVerticesByAngle();
        shadowColliders.ResetColliders();

        for (var i = 0; i < shapeEdgeVertices.Count; i++)
        {
            List<EdgeVertex> ev = shapeEdgeVertices[i];
            List<EdgeVertex> corners = FindCorners(ev);

            if (corners.Count == 0)
                continue;


            //Convert to world space array
            Vector2[] worldSpaceCorners = corners.Select(v => spaceConverter.TextureToWorldSpace(v.position)).ToArray();
            shadowColliders.AddPointsToCollider(worldSpaceCorners);

            /*for (var j = 1; j < corners.Count; j++)
            {
                Debug.DrawLine(spaceConverter.TextureToWorldSpace(corners[j - 1].position), spaceConverter.TextureToWorldSpace(corners[j].position), Color.magenta, Time.deltaTime);
            }

            Debug.DrawLine(spaceConverter.TextureToWorldSpace(corners[corners.Count - 1].position), spaceConverter.TextureToWorldSpace(corners[0].position), Color.magenta, Time.deltaTime);
            */
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
        /*
        for (var i = 1; i < corners.Count; i++)
        {
            //Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(corners[i - 1].position), spaceConverter.TextureToWorldSpace(corners[i].position));
        }

        Gizmos.DrawLine(spaceConverter.TextureToWorldSpace(corners[corners.Count - 1].position), spaceConverter.TextureToWorldSpace(corners[0].position));
        */
    }

    private void ReleaseBuffers()
    {
        if (circleBuffer != null)
            circleBuffer.Release();

        lightBuffer.Release();
        edgeVertexBuffer.Release();
        boxBuffer.Release();
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