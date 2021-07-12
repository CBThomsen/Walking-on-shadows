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
    private ComputeBuffer optimizedEdgeVertexBuffer;

    private List<EdgeVertex> v;

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

    void Update()
    {
        //Raytracing
        LightData[] lightDataArray = this.sceneGeometry.GetLightDataArray();
        lightBuffer = new ComputeBuffer(lightDataArray.Length, lightDataArray.Length * 2 * sizeof(float));
        lightBuffer.SetData(lightDataArray);
        computeShader.SetBuffer(raytracerKI, "lights", lightBuffer);

        CircleData[] circleDataArray = this.sceneGeometry.GetCircleDatas();
        circleBuffer = new ComputeBuffer(circleDataArray.Length, circleDataArray.Length * 3 * sizeof(float));
        circleBuffer.SetData(circleDataArray);
        computeShader.SetBuffer(raytracerKI, "circles", circleBuffer);

        ComputeBuffer verticesBuffer = new ComputeBuffer(2000, 2 * sizeof(float), ComputeBufferType.Append);
        verticesBuffer.SetCounterValue(0);
        computeShader.SetBuffer(raytracerKI, "vertices", verticesBuffer);

        computeShader.Dispatch(raytracerKI, textureResolution / 32, textureResolution / 32, 1);

        //Edge detection
        edgeVertexBuffer = new ComputeBuffer(2000, 5 * sizeof(float) + 6 * sizeof(int), ComputeBufferType.Append);
        edgeVertexBuffer.SetCounterValue(0);
        edgeDetection.SetBuffer(edgeDetectionKI, "edgeVertices", edgeVertexBuffer);

        edgeDetection.Dispatch(edgeDetectionKI, textureResolution / 32, textureResolution / 32, 1);


        int edgeVertexCount = this.GetAppendBufferCount(edgeVertexBuffer);
        EdgeVertex[] shadowVertices = new EdgeVertex[edgeVertexCount];
        edgeVertexBuffer.GetData(shadowVertices);

        v = shadowVertices.ToList();

        var centroid = new Vector2(0, 0);

        for (var i = 0; i < v.Count; i++)
        {
            centroid.x += (float)v[i].position.x / (float)v.Count;
            centroid.y += (float)v[i].position.y / (float)v.Count;
        }

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

            Debug.DrawLine(spaceConverter.TextureToWorldSpace(new Vector2(v[i - 1].position.x, v[i - 1].position.y)), spaceConverter.TextureToWorldSpace(new Vector2(v[i].position.x, v[i].position.y)), Color.red, Time.deltaTime);
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
        verticesBuffer.Release();
    }

    public void saveCSV()
    {
        using (var w = new StreamWriter("./test.csv"))
        {
            for (var i = 1; i < v.Count; i++)
            {
                var deltaSlope = v[i].slope - v[i - 1].slope;
                var line = string.Format("{0},{1},{2}", i, v[i].slope, deltaSlope);
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