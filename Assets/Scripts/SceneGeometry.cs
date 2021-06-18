using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct CircleData
{
    public Vector2 center;
    public float radius;
}

public struct LightData
{
    public Vector2 position;
}

public class SceneGeometry : MonoBehaviour
{
    public SpaceConverter spaceConverter;

    public GameObject[] sceneLights;
    public CircleCollider2D[] sceneCircles;

    private CircleData[] circleData;
    private LightData[] lightData;

    private float textureResolution = 1024f;

    void Awake()
    {
        sceneLights = GameObject.FindGameObjectsWithTag("Light");

        lightData = new LightData[sceneLights.Length];
        circleData = new CircleData[sceneCircles.Length];

        for (var i = 0; i < sceneLights.Length; i++)
        {
            lightData[i].position = sceneLights[i].transform.position;
            Debug.Log("Light positions:");
            Debug.Log("World:" + lightData[i].position);
            //Debug.Log("Normalized: " + cam.WorldToViewportPoint(sceneLights[i].transform.position));
            //Debug.Log("Screen: " + cam.WorldToScreenPoint(sceneLights[i].transform.position));
        }
    }

    public LightData[] GetLightDataArray()
    {
        return lightData;
    }

    public CircleData[] GetCircleDatas()
    {
        return circleData;
    }

    private void Update()
    {
        for (var i = 0; i < sceneLights.Length; i++)
        {
            //lightData[i].worldPos = sceneLights[i].transform.position;
            lightData[i].position = spaceConverter.WorldToTextureSpace(sceneLights[i].transform.position);
            //lightData[i].position = cam.WorldToViewportPoint(sceneLights[i].transform.position);
        }

        for (var i = 0; i < sceneCircles.Length; i++)
        {
            //circleData[i].center = cam.WorldToViewportPoint(sceneCircles[i].transform.position);

            circleData[i].center = spaceConverter.WorldToTextureSpace(sceneLights[i].transform.position);
            circleData[i].radius = 0.1f * 1024f;
        }
    }

}
