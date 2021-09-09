using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MergeSort;

public class ShadowSystem : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader vertexSortComputeShader;

    public ShadowRenderer shadowRenderer;
    public ComputeBuffers computeBuffers;
    public ShadowColliders shadowColliders;
    public Color shadowColor;

    private EdgeVertex[] shadowEdgeVertices = new EdgeVertex[ComputeBuffers.EDGEVERTEX_BUFFER_SIZE];

    public static Vector2Int textureResolution = new Vector2Int(1920, 1080) / 1;

    private int shadowComputeShaderKI;
    private int moveTexPixelsKI;

    void Start()
    {
        shadowRenderer.CreateRenderTextures(textureResolution.x, textureResolution.y);

        shadowRenderer.SetTexture(shadowRenderer.lightTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "shadowTexture", shadowRenderer.shadowTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "lightTexture", shadowRenderer.lightTexture);
        computeShader.SetInts("resolution", new int[] { textureResolution.x, textureResolution.y });

        shadowComputeShaderKI = computeShader.FindKernel("ShadowComputeShader");

        moveTexPixelsKI = computeShader.FindKernel("MoveTexturePixels");
        computeShader.SetTexture(moveTexPixelsKI, "shadowTexture", shadowRenderer.shadowTexture);
        computeShader.SetTexture(moveTexPixelsKI, "lightTexture", shadowRenderer.lightTexture);

        //StartCoroutine(DelayedUpdate());
    }

    private void SetupAndDispatchComputeShader()
    {
        ComputeBuffer lightBuffer = computeBuffers.GetLightBuffer();
        ComputeBuffer boxBuffer = computeBuffers.GetBoxBuffer();
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();

        computeShader.SetBuffer(shadowComputeShaderKI, "lights", lightBuffer);
        computeShader.SetBuffer(shadowComputeShaderKI, "boxes", boxBuffer);
        computeShader.SetBuffer(shadowComputeShaderKI, "edgeVertices", edgeVertexBuffer);
        computeShader.SetVector("shadowColor", shadowColor);

        computeShader.Dispatch(shadowComputeShaderKI, textureResolution.x / 32, textureResolution.y / 30, 1);
    }

    private IEnumerator DelayedUpdate()
    {
        yield return new WaitForEndOfFrame();

        SetupAndDispatchComputeShader();

        //Copy shadow edge vertices from shader to shadowEdgeVertices array
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();
        int vertexCount = computeBuffers.GetAppendBufferCount(edgeVertexBuffer);
        int powTwoCount = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(vertexCount) / Mathf.Log(2)));

        Debug.Log("Count: " + vertexCount + ", powTwoCount =" + powTwoCount);

        BitonicMergeSort sort = new BitonicMergeSort(vertexSortComputeShader);
        DisposableBuffer<uint> keys = new DisposableBuffer<uint>(powTwoCount);

        sort.Init(keys.Buffer);
        sort.SortEdgeVertices(keys.Buffer, edgeVertexBuffer);
        keys.Download();

        edgeVertexBuffer.GetData(shadowEdgeVertices);

        var failed = false;
        var lastVal = shadowEdgeVertices[keys.Data[0]].angleToShapeCenter;
        for (var i = 0; i < powTwoCount; i++)
        {
            var val = shadowEdgeVertices[keys.Data[i]].angleToShapeCenter;
            if (val > lastVal)
            {
                failed = true;
                Debug.LogErrorFormat("Unexpected Key {0} at {1}", val, i);
            }

            lastVal = val;
        }

        Debug.LogFormat("Sort Test Result = {0}", (failed ? "Wrong" : "Correct"));

        //shadowColliders.UpdateColliders(shadowEdgeVertices, keys, vertexCount);
    }

    private void Update()
    {
        for (var i = 0; i < shadowEdgeVertices.Length; i++)
        {
            shadowEdgeVertices[i].shapeIndex = -1;
            shadowEdgeVertices[i].angleToShapeCenter = -99;
            shadowEdgeVertices[i].position = Vector2.zero;
        }

        computeBuffers.GetEdgeVertexBuffer().SetData(shadowEdgeVertices);

        SetupAndDispatchComputeShader();

        //Copy shadow edge vertices from shader to shadowEdgeVertices array
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();
        int vertexCount = computeBuffers.GetAppendBufferCount(edgeVertexBuffer);
        int powTwoCount = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(vertexCount) / Mathf.Log(2)));

        if (vertexCount == 0)
            return;

        BitonicMergeSort sort = new BitonicMergeSort(vertexSortComputeShader);
        DisposableBuffer<uint> keys = new DisposableBuffer<uint>(powTwoCount);

        sort.Init(keys.Buffer);
        sort.SortEdgeVertices(keys.Buffer, edgeVertexBuffer);
        keys.Download();

        edgeVertexBuffer.GetData(shadowEdgeVertices);

        var failed = false;
        var lastVal = shadowEdgeVertices[keys.Data[0]].angleToShapeCenter;
        for (var i = 0; i < vertexCount; i++)
        {
            var val = shadowEdgeVertices[keys.Data[i]].angleToShapeCenter;
            if (val > lastVal)
            {
                failed = true;
                Debug.LogErrorFormat("Unexpected Key {0} at {1}", val, i);
            }

            lastVal = val;
        }

        if (failed)
            Debug.LogWarningFormat("Sort Test Result = {0}", (failed ? "Wrong" : "Correct"));

        shadowColliders.Create(shadowEdgeVertices, keys.Data, vertexCount);
        keys.Dispose();
    }

    public void Move(int direction)
    {
        computeShader.Dispatch(moveTexPixelsKI, textureResolution.x / 32, textureResolution.y / 30, 1);
    }
}