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

public struct EdgeVertex
{
    public Vector2Int position;
    public Vector2Int neighbourA;
    public Vector2Int neighbourB;
    public float slope;
    public Vector4 debugstuff;
}

public class SceneGeometry : MonoBehaviour
{
    public SpaceConverter spaceConverter;

    public GameObject[] sceneLights;
    public CircleCollider2D[] sceneCircles;

    private CircleData[] circleData;
    private LightData[] lightData;

    void Awake()
    {
        sceneLights = GameObject.FindGameObjectsWithTag("Light");

        lightData = new LightData[sceneLights.Length];
        circleData = new CircleData[sceneCircles.Length];
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

        for (var j = 0; j < sceneCircles.Length; j++)
        {
            //circleData[i].center = cam.WorldToViewportPoint(sceneCircles[i].transform.position);

            circleData[j].center = spaceConverter.WorldToTextureSpace(sceneCircles[j].transform.position);
            circleData[j].radius = spaceConverter.WorldToTextureSpace(new Vector2(sceneCircles[j].transform.position.x + sceneCircles[j].radius, 0)).x - circleData[j].center.x;
        }
    }

}
