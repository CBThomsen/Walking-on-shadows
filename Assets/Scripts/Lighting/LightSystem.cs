using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LightSystem : MonoBehaviour
{
    public Material spriteLitMaterial;
    public Material spriteLitMaterialNoShadow;

    public ComputeBuffers computeBuffers;
    public ShadowRenderer shadowRenderer;
    public Color ambientColor;

    [Inject]
    public void Construct(ComputeBuffers computeBuffers)
    {
        this.computeBuffers = computeBuffers;
    }

    void Start()
    {
        spriteLitMaterial.SetInt("resolutionX", ShadowSystem.textureResolution.x);
        spriteLitMaterial.SetInt("resolutionY", ShadowSystem.textureResolution.y);
        spriteLitMaterial.SetTexture("LightTex", shadowRenderer.lightTexture);

        spriteLitMaterialNoShadow.SetInt("resolutionX", ShadowSystem.textureResolution.x);
        spriteLitMaterialNoShadow.SetInt("resolutionY", ShadowSystem.textureResolution.y);
        spriteLitMaterialNoShadow.SetTexture("LightTex", shadowRenderer.lightTexture);
    }

    void Update()
    {
        spriteLitMaterial.SetVector("ambient", ambientColor);
        spriteLitMaterialNoShadow.SetVector("ambient", ambientColor);
        spriteLitMaterialNoShadow.SetBuffer("lights", computeBuffers.GetLightBuffer());
    }
}