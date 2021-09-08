using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowRenderer : MonoBehaviour
{
    public RenderTexture shadowTexture;
    public RenderTexture lightTexture;

    private void Start()
    {
        Vector2 scale = new Vector2(1f, 1f);
        scale.x = ShadowSystem.textureResolution.x / SpaceConverter.WorldToTextureScaleFactor().x;
        scale.y = ShadowSystem.textureResolution.y / SpaceConverter.WorldToTextureScaleFactor().y;
        transform.localScale = scale;
    }

    public void CreateRenderTextures(int resolutionX, int resolutionY)
    {
        shadowTexture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGB32);
        shadowTexture.enableRandomWrite = true;
        shadowTexture.Create();

        lightTexture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGB32);
        lightTexture.enableRandomWrite = true;
        lightTexture.Create();
    }

    public void SetTexture(RenderTexture texture)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
    }

}