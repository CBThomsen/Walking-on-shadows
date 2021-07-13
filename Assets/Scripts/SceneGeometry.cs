using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct CircleData
{
    public Vector2 center;
    public float radius;
}

public struct BoxData
{
    public Vector2 center;
    public Vector2 extents;
}

public struct LightData
{
    public Vector2 position;
}

public struct EdgeVertex
{
    public Vector2 position;
    public Vector2 roundedPosition;
    public int shapeIndex;
}

public class SceneGeometry : MonoBehaviour
{
    public SpaceConverter spaceConverter;

    public GameObject[] sceneLights;
    public CircleCollider2D[] sceneCircles;
    public BoxCollider2D[] sceneBoxes;

    private CircleData[] circleData;
    private BoxData[] boxData;
    private LightData[] lightData;

    void Awake()
    {
        sceneLights = GameObject.FindGameObjectsWithTag("Light");

        lightData = new LightData[sceneLights.Length];
        circleData = new CircleData[sceneCircles.Length];
        boxData = new BoxData[sceneBoxes.Length];
    }

    public LightData[] GetLightDataArray()
    {
        return lightData;
    }

    public CircleData[] GetCircleDatas()
    {
        return circleData;
    }

    public BoxData[] GetBoxDatas()
    {
        return boxData;
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

        for (var j = 0; j < sceneBoxes.Length; j++)
        {
            //circleData[i].center = cam.WorldToViewportPoint(sceneCircles[i].transform.position);

            boxData[j].center = spaceConverter.WorldToTextureSpace(sceneBoxes[j].transform.position);

            var boxMax = (Vector2)sceneBoxes[j].transform.position + sceneBoxes[j].size * 0.5f;
            boxData[j].extents = spaceConverter.WorldToTextureSpace(boxMax) - boxData[j].center;
        }
    }

}
