using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSystem : MonoBehaviour
{
    public ComputeShader computeShader;
    public ShadowRenderer shadowRenderer;
    public ComputeBuffers computeBuffers;
    public ShadowColliders shadowColliders;

    private EdgeVertex[] shadowEdgeVertices = new EdgeVertex[ComputeBuffers.EDGEVERTEX_BUFFER_SIZE];

    public static int textureResolution = 1024;
    private int shadowComputeShaderKI;

    void Start()
    {
        RenderTexture renderTexture = shadowRenderer.CreateRenderTexture(textureResolution, textureResolution);
        shadowRenderer.SetTexture(renderTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "destTexture", renderTexture);

        shadowComputeShaderKI = computeShader.FindKernel("ShadowComputeShader");
    }

    private void SetupAndDispatchComputeShader()
    {
        ComputeBuffer lightBuffer = computeBuffers.GetLightBuffer();
        ComputeBuffer boxBuffer = computeBuffers.GetBoxBuffer();
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();

        computeShader.SetBuffer(shadowComputeShaderKI, "lights", lightBuffer);
        computeShader.SetBuffer(shadowComputeShaderKI, "boxes", boxBuffer);
        computeShader.SetBuffer(shadowComputeShaderKI, "edgeVertices", edgeVertexBuffer);

        computeShader.Dispatch(shadowComputeShaderKI, textureResolution / 32, textureResolution / 32, 1);
    }

    void Update()
    {
        SetupAndDispatchComputeShader();

        //Copy shadow edge vertices from shader to shadowEdgeVertices array
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();
        int vertexCount = computeBuffers.GetAppendBufferCount(edgeVertexBuffer);
        edgeVertexBuffer.GetData(shadowEdgeVertices);

        shadowColliders.UpdateColliders(shadowEdgeVertices, vertexCount);
    }
}