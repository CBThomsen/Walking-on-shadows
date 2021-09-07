using UnityEngine;

public class SpaceConverter : MonoBehaviour
{
    private static Camera cam { get { return Camera.main; } }
    private static Vector2 scaleFactor;

    private static void CalculateScaleFactor()
    {
        Vector2 origin = WorldToTextureSpace(Vector2.zero);
        Vector2 scaledVector = WorldToTextureSpace(Vector2.one) - origin;
        scaleFactor = scaledVector / Vector2.one;
    }

    public static Vector2 WorldToTextureScaleFactor()
    {
        if (scaleFactor == Vector2.zero)
            CalculateScaleFactor();

        return scaleFactor;
    }

    public static Vector2 WorldToTextureSpace(Vector2 worldPos)
    {
        return cam.WorldToViewportPoint(worldPos) * (Vector2)ShadowSystem.textureResolution;
    }

    public static Vector2 TextureToWorldSpace(Vector2 texturePos)
    {
        Vector3 viewPortPos = new Vector3((float)texturePos.x / (float)ShadowSystem.textureResolution.x, (float)texturePos.y / (float)ShadowSystem.textureResolution.y, cam.transform.position.z);
        Vector2 result = cam.ViewportToWorldPoint(viewPortPos);
        return result;
    }


}