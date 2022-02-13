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

    private float currentIntensity;

    private Transform followTarget;
    private Rigidbody2D body;

    private bool collidersEnabled = true;
    public bool renderingEnabled = true;

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
        data.intensity = currentIntensity;
        data.isOn = renderingEnabled ? 1 : 0;

        return data;
    }

    public bool GetCollidersEnabled()
    {
        return collidersEnabled;
    }

    public void SetCollidersEnabled(bool enabled)
    {
        collidersEnabled = enabled;
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

    public IEnumerator Spawn()
    {
        transform.localScale = Vector3.zero;

        while (currentIntensity < intensity - 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, intensity, 0.05f);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.05f);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator Despawn()
    {
        while (currentIntensity > 0.01f)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, 0f, 0.1f);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.05f);
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
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

    private void Update()
    {
    }
}