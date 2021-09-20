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

    [SerializeField]
    public float angle = 360f;

    private LightData data = new LightData();

    private Transform followTarget;
    private Rigidbody2D body;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public LightData GetLightData()
    {
        data.rotation = transform.rotation.eulerAngles.z * Mathf.PI / 180f;//((transform.rotation.eulerAngles.z) * Mathf.PI / 180f + 2 * Mathf.PI) % (2 * Mathf.PI);
        data.angle = angle;
        data.position = SpaceConverter.WorldToTextureSpace(transform.position);
        data.range = range * SpaceConverter.WorldToTextureScaleFactor().x;
        data.color = (Vector4)color;
        data.intensity = intensity;
        data.isOn = (gameObject.activeInHierarchy) ? 1 : 0;

        return data;
    }

    public void PickUp(Transform followTarget)
    {
        this.followTarget = followTarget;
        if (body)
            body.simulated = false;
    }

    public void Drop()
    {
        followTarget = null;
        body.simulated = true;
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

    private void FixedUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

}