using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    [SerializeField]
    private Color color;

    [SerializeField]
    private float range;

    [SerializeField]
    private float intensity;

    private LightData data = new LightData();

    public LightData GetLightData()
    {
        data.angle = transform.rotation.eulerAngles.z * Mathf.PI / 180f;
        data.color = (Vector4)color;
        data.range = range;
        data.intensity = intensity;

        return data;
    }

}