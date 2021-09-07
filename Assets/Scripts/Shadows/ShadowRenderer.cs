using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowRenderer : MonoBehaviour
{

    public RenderTexture CreateRenderTexture(int resolutionX, int resolutionY)
    {
        RenderTexture texture = new RenderTexture(resolutionX, resolutionY, 24, RenderTextureFormat.ARGB32);
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