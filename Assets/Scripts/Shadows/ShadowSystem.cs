using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MergeSort;
using Zenject;

public class ShadowSystem : MonoBehaviour
{
    public ComputeShader computeShader;
    public ComputeShader vertexSortComputeShader;

    public ShadowRenderer shadowRenderer;
    public ComputeBuffers computeBuffers;
    public Color shadowColor;

    public static Vector2Int textureResolution = new Vector2Int(1920, 1080);

    private int shadowComputeShaderKI;

    [Inject]
    public void Construct(ComputeBuffers computeBuffers)
    {
        this.computeBuffers = computeBuffers;
    }

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

        computeShader.SetBuffer(shadowComputeShaderKI, "lights", lightBuffer);
        computeShader.SetBuffer(shadowComputeShaderKI, "boxes", boxBuffer);
        computeShader.SetVector("shadowColor", shadowColor);

        computeShader.Dispatch(shadowComputeShaderKI, textureResolution.x / 32, textureResolution.y / 30, 1);
    }

    private void Update()
    {
        SetupAndDispatchComputeShader();
    }
}