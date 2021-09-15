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

    private LightData data = new LightData();

    public LightData GetLightData()
    {
        data.angle = transform.rotation.eulerAngles.z * Mathf.PI / 180f;
        data.position = SpaceConverter.WorldToTextureSpace(transform.position);

        data.range = range * SpaceConverter.WorldToTextureScaleFactor().x;
        data.color = (Vector4)color;
        data.intensity = intensity;
        data.isOn = (gameObject.activeInHierarchy) ? 1 : 0;

        return data;
    }

    public void ToggleOnOff()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void OnDrawGizmos()
    {
        Color gizmoColor = color;
        gizmoColor.a = 0.5f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}