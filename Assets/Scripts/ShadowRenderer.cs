using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowRenderer : MonoBehaviour
{
    public void SetTexture(RenderTexture texture)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture("_MainTex", texture);
    }

}