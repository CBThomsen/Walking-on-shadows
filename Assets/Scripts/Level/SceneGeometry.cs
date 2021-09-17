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
    public float angleToShapeCenter;
}

public class SceneGeometry : MonoBehaviour
{

    public Light[] GetLights()
    {
        return GetComponentsInChildren<Light>(true);
    }

    public Box[] GetBoxes()
    {
        return GetComponentsInChildren<Box>(true);
    }

    public Vector3 GetSpawnPoint()
    {
        SpawnPoint spawnPoint = GetComponentInChildren<SpawnPoint>();

        if (spawnPoint)
            return spawnPoint.transform.position;
        else
            return new Vector3(5f, 0.5f);
    }

}
