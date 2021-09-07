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
    public float angle;
    public float range;
    public float intensity;
    public Vector4 color;
    public Vector2 position;
    public int isOn;
}

public struct EdgeVertex
{
    public Vector2 position;
    public int shapeIndex;
}

public class SceneGeometry : MonoBehaviour
{
    public static SceneGeometry instance;

    private List<GameObject> sceneLights;
    private List<CircleCollider2D> sceneCircles;
    private List<BoxCollider2D> sceneBoxes;

    private CircleData[] circleData;
    private BoxData[] boxData;
    private LightData[] lightData;

    void Awake()
    {
        if (!SceneGeometry.instance)
            SceneGeometry.instance = this;
        else
            Debug.LogWarning("More instances of SceneGeomtry found!");

        Transform[] childObjects = gameObject.GetComponentsInChildren<Transform>(true);

        sceneLights = childObjects.Where(t => t.tag == "Light").Select(t => t.gameObject).ToList();
        sceneBoxes = childObjects.Where(t => t.tag == "EnvBox").Select(g => g.GetComponent<BoxCollider2D>()).ToList();
        sceneCircles = childObjects.Where(t => t.tag == "EnvCircle").Select(g => g.GetComponent<CircleCollider2D>()).ToList();

        lightData = new LightData[sceneLights.Count];
        circleData = new CircleData[sceneCircles.Count];
        boxData = new BoxData[sceneBoxes.Count];
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
        for (var i = 0; i < sceneLights.Count; i++)
        {
            /*if (!sceneLights[i].activeInHierarchy)
                continue;*/

            var l = sceneLights[i].GetComponent<Light>();

            if (l)
            {
                LightData data = l.GetLightData();
                data.position = SpaceConverter.WorldToTextureSpace(sceneLights[i].transform.position);
                data.range = data.range * SpaceConverter.WorldToTextureScaleFactor().x;
                lightData[i] = data;
            }
        }

        for (var j = 0; j < sceneCircles.Count; j++)
        {
            /*if (!sceneCircles[j].gameObject.activeInHierarchy)
                continue;*/

            var c = new CircleData();
            c.center = SpaceConverter.WorldToTextureSpace(sceneCircles[j].transform.position);
            c.radius = sceneCircles[j].radius * SpaceConverter.WorldToTextureScaleFactor().x;

            circleData[j] = c;
        }

        for (var j = 0; j < sceneBoxes.Count; j++)
        {
            /*if (!sceneBoxes[j].gameObject.activeInHierarchy)
                continue;*/

            var b = new BoxData();
            b.center = SpaceConverter.WorldToTextureSpace(sceneBoxes[j].transform.position);
            b.extents = sceneBoxes[j].size * 0.5f * SpaceConverter.WorldToTextureScaleFactor();

            boxData[j] = b;
        }
    }

}
