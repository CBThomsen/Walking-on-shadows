using UnityEngine;

public class SpaceConverter : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        //cam.aspect = 1f / 1f;
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