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

    private List<GameObject> sceneLights;
    private List<CircleCollider2D> sceneCircles;
    private List<BoxCollider2D> sceneBoxes;

    private List<CircleData> circleData;
    private List<BoxData> boxData;
    private List<LightData> lightData;

    void Start()
    {
        Transform[] childObjects = gameObject.GetComponentsInChildren<Transform>(true);

        sceneLights = childObjects.Where(t => t.tag == "Light").Select(t => t.gameObject).ToList();
        sceneBoxes = childObjects.Where(t => t.tag == "EnvBox").Select(g => g.GetComponent<BoxCollider2D>()).ToList();
        sceneCircles = childObjects.Where(t => t.tag == "EnvCircle").Select(g => g.GetComponent<CircleCollider2D>()).ToList();

        lightData = new List<LightData>();
        circleData = new List<CircleData>();
        boxData = new List<BoxData>();
    }

    public LightData[] GetLightDataArray()
    {
        return lightData.ToArray();
    }

    public CircleData[] GetCircleDatas()
    {
        return circleData.ToArray();
    }

    public BoxData[] GetBoxDatas()
    {
        return boxData.ToArray();
    }

    private void Update()
    {
        lightData.Clear();
        circleData.Clear();
        boxData.Clear();

        for (var i = 0; i < sceneLights.Count; i++)
        {
            if (!sceneLights[i].activeInHierarchy)
                continue;

            var l = new LightData();
            l.position = spaceConverter.WorldToTextureSpace(sceneLights[i].transform.position);
            l.angle = sceneLights[i].transform.rotation.eulerAngles.z * Mathf.PI / 180f;

            lightData.Add(l);
        }

        for (var j = 0; j < sceneCircles.Count; j++)
        {
            if (!sceneCircles[j].gameObject.activeInHierarchy)
                continue;

            var c = new CircleData();
            c.center = spaceConverter.WorldToTextureSpace(sceneCircles[j].transform.position);
            c.radius = spaceConverter.WorldToTextureSpace(new Vector2(sceneCircles[j].transform.position.x + sceneCircles[j].radius, 0)).x - c.center.x;

            circleData.Add(c);
        }

        for (var j = 0; j < sceneBoxes.Count; j++)
        {
            if (!sceneBoxes[j].gameObject.activeInHierarchy)
                continue;

            var b = new BoxData();
            b.center = spaceConverter.WorldToTextureSpace(sceneBoxes[j].transform.position);

            var boxMax = (Vector2)sceneBoxes[j].transform.position + sceneBoxes[j].size * 0.5f;
            b.extents = spaceConverter.WorldToTextureSpace(boxMax) - b.center;

            boxData.Add(b);
        }
    }

}
