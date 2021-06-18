using UnityEngine;

public class SpaceConverter : MonoBehaviour
{
    public Camera cam;
    public int textureResolution = 1024;

    public Vector2 WorldToTextureSpace(Vector2 worldPos)
    {
        return cam.WorldToViewportPoint(worldPos) * textureResolution;
    }


}