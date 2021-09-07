using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSystem : MonoBehaviour
{
    public ComputeShader computeShader;
    public ShadowRenderer shadowRenderer;
    public ComputeBuffers computeBuffers;
    public ShadowColliders shadowColliders;
    public Color shadowColor;

    private EdgeVertex[] shadowEdgeVertices = new EdgeVertex[ComputeBuffers.EDGEVERTEX_BUFFER_SIZE];

    public static Vector2Int textureResolution = new Vector2Int(1920, 1080);
    private int shadowComputeShaderKI;

    void Start()
    {
        RenderTexture renderTexture = shadowRenderer.CreateRenderTexture(textureResolution.x, textureResolution.y);
        shadowRenderer.SetTexture(renderTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "destTexture", renderTexture);
        computeShader.SetInts("resolution", new int[] { textureResolution.x, textureResolution.y });

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
        computeShader.SetVector("shadowColor", shadowColor);

        computeShader.Dispatch(shadowComputeShaderKI, textureResolution.x / 32, textureResolution.y / 30, 1);
    }

    void Update()
    {
        SetupAndDispatchComputeShader();

        //Copy shadow edge vertices from shader to shadowEdgeVertices array
        ComputeBuffer edgeVertexBuffer = computeBuffers.GetEdgeVertexBuffer();
        int vertexCount = computeBuffers.GetAppendBufferCount(edgeVertexBuffer);
        edgeVertexBuffer.GetData(shadowEdgeVertices);

        if (vertexCount > ComputeBuffers.EDGEVERTEX_BUFFER_SIZE)
        {
            Debug.LogWarning("Found " + vertexCount + " edge vertices. Max is " + ComputeBuffers.EDGEVERTEX_BUFFER_SIZE + "!!");
            vertexCount = ComputeBuffers.EDGEVERTEX_BUFFER_SIZE;
        }

        shadowColliders.UpdateColliders(shadowEdgeVertices, vertexCount);
    }
}