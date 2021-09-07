using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeBuffers : MonoBehaviour
{
    public const int EDGEVERTEX_BUFFER_SIZE = 10000;

    private ComputeBuffer lightBuffer;
    private ComputeBuffer boxBuffer;
    private ComputeBuffer edgeVertexBuffer;

    private void Start()
    {
        CreateComputeBuffers();
    }

    private void OnDestroy()
    {
        lightBuffer.Release();
        boxBuffer.Release();
        edgeVertexBuffer.Release();
    }

    private void CreateComputeBuffers()
    {
        LightData[] lightDataArray = SceneGeometry.instance.GetLightDataArray();
        lightBuffer = new ComputeBuffer(lightDataArray.Length, lightDataArray.Length * 9 * sizeof(float));

        BoxData[] boxDataArray = SceneGeometry.instance.GetBoxDatas();
        boxBuffer = new ComputeBuffer(boxDataArray.Length, boxDataArray.Length * 4 * sizeof(float));

        edgeVertexBuffer = new ComputeBuffer(EDGEVERTEX_BUFFER_SIZE, 4 * sizeof(float) + sizeof(int), ComputeBufferType.Append);
        edgeVertexBuffer.SetCounterValue(0);
    }

    public ComputeBuffer GetLightBuffer()
    {
        return lightBuffer;
    }

    public ComputeBuffer GetBoxBuffer()
    {
        return boxBuffer;
    }

    public ComputeBuffer GetEdgeVertexBuffer()
    {
        return edgeVertexBuffer;
    }

    private void Update()
    {
        lightBuffer.SetData(SceneGeometry.instance.GetLightDataArray());
        boxBuffer.SetData(SceneGeometry.instance.GetBoxDatas());
        edgeVertexBuffer.SetCounterValue(0);
    }

    public int GetAppendBufferCount(ComputeBuffer appendBuffer)
    {
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);

        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);
        countBuffer.Dispose();
        return counter[0];
    }

}
