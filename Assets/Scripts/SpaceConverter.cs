using UnityEngine;

public class SpaceConverter : MonoBehaviour
{
    public Camera cam;

    private Vector2 scaleFactor;

    void Awake()
    {
        CalculateScaleFactor();
    }

    private void CalculateScaleFactor()
    {
        Vector2 origin = WorldToTextureSpace(Vector2.zero);
        Vector2 scaledVector = WorldToTextureSpace(Vector2.one) - origin;
        scaleFactor = scaledVector / Vector2.one;
    }

    public Vector2 WorldToTextureScaleFactor()
    {
        return scaleFactor;
    }

    public Vector2 WorldToTextureSpace(Vector2 worldPos)
    {
        return cam.WorldToViewportPoint(worldPos) * Raytracer.textureResolution;
    }

    public Vector2 TextureToWorldSpace(Vector2 texturePos)
    {
        Vector3 viewPortPos = new Vector3(1f - (float)texturePos.x / (float)Raytracer.textureResolution, 1f - (float)texturePos.y / (float)Raytracer.textureResolution, cam.transform.position.z);
        Vector2 result = cam.ViewportToWorldPoint(viewPortPos);
        return result;
    }


}