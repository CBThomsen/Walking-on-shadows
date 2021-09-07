using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public Material spriteLitMaterial;
    public ComputeBuffers computeBuffers;

    void Start()
    {
        spriteLitMaterial.SetFloat("resolution", ShadowSystem.textureResolution);
    }

    void Update()
    {
        spriteLitMaterial.SetBuffer("lights", computeBuffers.GetLightBuffer());
    }
}