using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public Material spriteLitMaterial;
    public ComputeBuffers computeBuffers;
    public Color ambientColor;

    void Start()
    {
        spriteLitMaterial.SetInt("resolutionX", ShadowSystem.textureResolution.x);
        spriteLitMaterial.SetInt("resolutionY", ShadowSystem.textureResolution.y);
    }

    void Update()
    {
        spriteLitMaterial.SetVector("ambient", ambientColor);
        spriteLitMaterial.SetBuffer("lights", computeBuffers.GetLightBuffer());
    }
}