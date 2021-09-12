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
    public Color shadowColor;

    private EdgeVertex[] shadowEdgeVertices = new EdgeVertex[ComputeBuffers.EDGEVERTEX_BUFFER_SIZE];

    public static Vector2Int textureResolution = new Vector2Int(1920, 1080);

    private int shadowComputeShaderKI;

    void Start()
    {
        shadowRenderer.CreateRenderTextures(textureResolution.x, textureResolution.y);

        shadowRenderer.SetTexture(shadowRenderer.lightTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "shadowTexture", shadowRenderer.shadowTexture);
        computeShader.SetTexture(shadowComputeShaderKI, "lightTexture", shadowRenderer.lightTexture);
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

    private void Update()
    {
        SetupAndDispatchComputeShader();
    }
}