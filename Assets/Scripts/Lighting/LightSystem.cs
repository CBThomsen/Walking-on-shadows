using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LightSystem : MonoBehaviour
{
    public Material spriteLitMaterial;
    public Material spriteLitMaterialNoShadow;
    public GameObject lightPrefab;

    public ComputeBuffers computeBuffers;
    public ShadowRenderer shadowRenderer;
    public Color ambientColor;

    private SceneGeometry sceneGeometry;

    [Inject]
    public void Construct(ComputeBuffers computeBuffers, SceneGeometry sceneGeometry)
    {
        this.computeBuffers = computeBuffers;
        this.sceneGeometry = sceneGeometry;
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

    public Light SpawnLight(Vector3 position)
    {
        GameObject obj = GameObject.Instantiate(lightPrefab, position, Quaternion.identity);
        obj.transform.SetParent(sceneGeometry.transform);
        Light light = obj.GetComponent<Light>();

        StartCoroutine(light.Spawn());

        return light;
    }
}