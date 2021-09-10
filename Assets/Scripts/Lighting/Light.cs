using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    [SerializeField]
    public Color color;

    [SerializeField]
    public float range;

    [SerializeField]
    public float intensity;

    public bool isOn = true;

    private LightData data = new LightData();

    public LightData GetLightData()
    {
        data.angle = transform.rotation.eulerAngles.z * Mathf.PI / 180f;
        data.color = (Vector4)color;
        data.range = range;
        data.intensity = intensity;
        data.isOn = (isOn && gameObject.activeInHierarchy) ? 1 : 0;

        return data;
    }

    public void ToggleOnOff()
    {
        isOn = !isOn;
    }

}