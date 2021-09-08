using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowRenderer : MonoBehaviour
{

    private void Start()
    {
        Vector2 scale = new Vector2(1f, 1f);
        scale.x = ShadowSystem.textureResolution.x / SpaceConverter.WorldToTextureScaleFactor().x;
        scale.y = ShadowSystem.textureResolution.y / SpaceConverter.WorldToTextureScaleFactor().y;
        transform.localScale = scale;
    }

    public RenderTexture CreateRenderTexture(int resolutionX, int resolutionY)
    {
        RenderTexture texture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.Create();

        return texture;
    }

    public void SetTexture(RenderTexture texture)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
    }

}