using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using MergeSort;

public class ShadowColliders : MonoBehaviour
{
    public GameObject prefab;

    public GameObject edgeColliderPrefab;

    public bool debugDrawCorners;
    public bool debugDrawEdgeVertices;

    private List<ShapeCollider> shapeColliders = new List<ShapeCollider>();

    private void Start()
    {
        LightData[] lights = SceneGeometry.instance.GetLightDataArray();
        BoxData[] boxes = SceneGeometry.instance.GetBoxDatas();

        int totalColliders = lights.Length * boxes.Length;
        for (int i = 0; i < totalColliders; i++)
        {
            int lightIndex = Mathf.FloorToInt(i / Mathf.Max(lights.Length, boxes.Length));
            //shapeColliders.Add(new ShapeCollider(gameObject, lights[lightIndex]));
        }
    }

    public void Create(EdgeVertex[] vertices, uint[] sortedKeys, int vertexCount)
    {
        List<List<Vector2>> shapeCorners = FindCorners(vertices, sortedKeys, vertexCount);
        ResetColliders();

        LightData[] lights = SceneGeometry.instance.GetLightDataArray();
        BoxData[] boxes = SceneGeometry.instance.GetBoxDatas();

        for (var shapeIndex = 0; shapeIndex < shapeCorners.Count; shapeIndex++)
        {
            if (shapeCorners[shapeIndex].Count > 2)
                AddPointsToCollider(shapeCorners[shapeIndex].ToArray());
        }
    }

    public List<List<Vector2>> FindCorners(EdgeVertex[] vertices, uint[] sortedKeys, int vertexCount)
    {
        List<List<Vector2>> shapeVertices = new List<List<Vector2>>();
        List<List<Vector2>> corners = new List<List<Vector2>>();

        int shapeIndex = -1;
        for (var i = 0; i < vertexCount; i++)
        {
            EdgeVertex ev = vertices[sortedKeys[i]];
            shapeIndex = ev.shapeIndex;

            while (shapeVertices.Count < shapeIndex + 1)
            {
                shapeVertices.Add(new List<Vector2>());
                corners.Add(new List<Vector2>());
            }

            shapeVertices[shapeIndex].Add(ev.position);
        }

        for (var s = 0; s < shapeVertices.Count; s++)
        {
            List<Vector2> positions = shapeVertices[s];

            if (positions.Count <= 2)
                continue;

            Vector2 deltaPos;
            float angle;
            float deltaAngle;

            Vector2 lastDeltaPos = positions[0] - positions[positions.Count - 1];
            float lastAngle = (Mathf.Atan2(lastDeltaPos.y, lastDeltaPos.x) + Mathf.PI) % (Mathf.PI);

            for (int j = 1; j < positions.Count + 1; j++)
            {
                deltaPos = positions[j == positions.Count ? 0 : j] - positions[j - 1];
                angle = (Mathf.Atan2(deltaPos.y, deltaPos.x) + Mathf.PI) % (Mathf.PI);
                deltaAngle = lastAngle - angle;

                if (Mathf.Abs(deltaAngle) > 0.05f) //1 degree
                {
                    corners[s].Add(SpaceConverter.TextureToWorldSpace(positions[j - 1]));
                }

                lastAngle = angle;
            }
        }

        return corners;
    }

    private float CalculateAngle(EdgeVertex ev, EdgeVertex lastEv)
    {
        Vector2 deltaPos = ev.position - lastEv.position;
        float angle = (Mathf.Atan2(deltaPos.y, deltaPos.x) + Mathf.PI) % (Mathf.PI);
        return angle;
    }

    public void ResetColliders()
    {
        PolygonCollider2D polyCol = prefab.GetComponent<PolygonCollider2D>();
        polyCol.pathCount = 0;
    }

    public void AddPointsToCollider(Vector2[] points)
    {
        PolygonCollider2D polyCol = prefab.GetComponent<PolygonCollider2D>();
        polyCol.pathCount += 1;

        polyCol.SetPath(polyCol.pathCount - 1, points);
    }

    private void DebugDraw(List<Vector2> vertices, bool drawAsPoints = true)
    {
        if (drawAsPoints)
        {
            for (var j = 0; j < vertices.Count; j++)
            {
                Debug.DrawLine(vertices[j], vertices[j] + new Vector2(0f, 0.5f), Color.magenta, Time.deltaTime);
            }
        }
        /*else
        {
            for (var j = 1; j < vertices.Count; j++)
            {
                Debug.DrawLine(SpaceConverter.TextureToWorldSpace(vertices[j - 1].position), SpaceConverter.TextureToWorldSpace(vertices[j].position), Color.magenta, Time.deltaTime);
            }

            Debug.DrawLine(SpaceConverter.TextureToWorldSpace(vertices[corners.Count - 1].position), SpaceConverter.TextureToWorldSpace(vertices[0].position), Color.magenta, Time.deltaTime);
        }*/
    }

}