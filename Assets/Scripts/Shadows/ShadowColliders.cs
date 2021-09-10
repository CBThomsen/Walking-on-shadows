using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using MergeSort;

public class Vector2Pair
{
    public Vector2 v1;
    public Vector2 v2;
    public Vector2 delta;

    public Vector2Pair(Vector2 v1, Vector2 v2)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.delta = v2 - v1;
    }
}

public class ShapeCollider
{
    public GameObject parentObject;
    public BoxCollider2D[] colliders;
    public Rigidbody2D[] colliderBodies;
    private LightData lightData;

    private List<Vector2Pair> colliderVecs = new List<Vector2Pair>();

    public ShapeCollider(GameObject parent)
    {
        colliders = new BoxCollider2D[4];
        colliderBodies = new Rigidbody2D[4];

        for (var i = 0; i < 4; i++)
        {
            GameObject col = new GameObject("col" + i);
            Rigidbody2D body = col.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            colliderBodies[i] = body;

            colliders[i] = col.AddComponent<BoxCollider2D>();
            colliders[i].size = new Vector2(10f, 0.1f);
            col.transform.SetParent(parent.transform);
        }

        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        bool wasDisabled = false;
        for (var i = 0; i < colliders.Length; i++)
        {
            wasDisabled = wasDisabled ? wasDisabled : !colliders[i].gameObject.activeInHierarchy;
            colliders[i].gameObject.SetActive(enabled);

        }

        if (wasDisabled)
            UpdateColliders(true);
    }

    public void SetColliderPoints(int index, Vector2 p1, Vector2 p2)
    {
        if (colliderVecs.Count <= index)
            colliderVecs.Add(new Vector2Pair(p1, p2));
        else
            colliderVecs[index] = new Vector2Pair(p1, p2);
    }

    public void UpdateColliders(bool instant = false)
    {
        //Scale and position properly
        for (var i = 0; i < colliderVecs.Count; i++)
        {
            Vector2Pair vecs = colliderVecs[i];
            Vector2 newPosition = vecs.v1 + vecs.delta * 0.5f;
            Quaternion newRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vecs.delta.y, vecs.delta.x) * 180f / Mathf.PI);

            if (instant)
            {
                colliderBodies[i].position = newPosition;
                colliderBodies[i].rotation = newRotation.eulerAngles.z;
            }
            else
            {
                colliderBodies[i].MovePosition(newPosition);
                colliderBodies[i].MoveRotation(newRotation);
            }

            colliders[i].size = new Vector2(vecs.delta.magnitude, colliders[i].size.y);
        }
    }
}

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
            /*   if (shapeCorners[shapeIndex].Count < 2)
               {
                   shapeColliders[shapeIndex].SetEnabled(false);
                   continue;
               }
               else
               {
                   shapeColliders[shapeIndex].SetEnabled(true);
               }

               if (debugDrawCorners)
                   DebugDraw(shapeCorners[shapeIndex]);

               int lightIndex = Mathf.FloorToInt(shapeIndex / Mathf.Max(lights.Length, boxes.Length));
               int boxIndex = shapeIndex % boxes.Length;

               //Debug.Log(shapeIndex + " , light=" + lightIndex + " , boxes=" + boxIndex + " corners:" + shapeCorners[shapeIndex].Count);

               LightData light = lights[lightIndex];
               BoxData box = boxes[boxIndex];

               List<Vector2> boxCorners = new List<Vector2>();
               boxCorners.Add((box.center + new Vector2(box.extents.x, box.extents.y)));
               boxCorners.Add((box.center + new Vector2(-box.extents.x, box.extents.y)));
               boxCorners.Add((box.center + new Vector2(-box.extents.x, -box.extents.y)));
               boxCorners.Add((box.center + new Vector2(box.extents.x, -box.extents.y)));

               List<Vector2> corners = shapeCorners[shapeIndex];

               for (int i = 0; i < boxCorners.Count; i++)
               {
                   int bestFitIndex = -1;
                   float lowestAngle = 9999f;

                   for (var j = 1; j < corners.Count + 1; j++)
                   {
                       int curIndex = j == corners.Count ? 0 : j;
                       Vector2 line = corners[j - 1] - corners[curIndex];

                       Vector2 lightToBoxExtentLine = SpaceConverter.TextureToWorldSpace(light.position) - SpaceConverter.TextureToWorldSpace(boxCorners[i]);
                       float dot = Vector2.Dot(line, lightToBoxExtentLine);
                       float angleBetweenVectors = Mathf.Acos(dot / (line.magnitude * lightToBoxExtentLine.magnitude)) * 180 / Mathf.PI;
                       if (angleBetweenVectors > 90f)
                           angleBetweenVectors -= 180f;

                       if (Mathf.Abs(angleBetweenVectors) < lowestAngle)
                       {
                           bestFitIndex = j;
                           lowestAngle = Mathf.Abs(angleBetweenVectors);
                       }
                   }

                   //shapeColliders[shapeIndex].AddColliderPoints(corners[bestFitIndex - 1], corners[bestFitIndex == corners.Count ? 0 : bestFitIndex], lowestAngle);
               }

               shapeColliders[shapeIndex].UpdateColliders();

               /*for (var i = 1; i < corners.Count + 1; i++)
               {
                   int curIndex = i == corners.Count ? 0 : i;
                   Vector2 line = corners[i - 1] - corners[curIndex];

                   for (var j = 0; j < boxCorners.Count; j++)
                   {
                       Vector2 lightToBoxExtentLine = SpaceConverter.TextureToWorldSpace(light.position) - SpaceConverter.TextureToWorldSpace(boxCorners[j]);
                       float dot = Vector2.Dot(line, lightToBoxExtentLine);
                       float angleBetweenVectors = Mathf.Acos(dot / (line.magnitude * lightToBoxExtentLine.magnitude)) * 180 / Mathf.PI;
                       float tol = 5f;

                       if ((Mathf.Abs(angleBetweenVectors) < tol || Mathf.Abs(angleBetweenVectors - 180f) < tol) && line.magnitude > 1f)
                       {
                           Debug.DrawLine(SpaceConverter.TextureToWorldSpace(light.position), SpaceConverter.TextureToWorldSpace(boxCorners[j]), Color.green, Time.deltaTime);
                           shapeColliders[shapeIndex].AddColliderPoints(corners[i - 1], corners[curIndex], angleBetweenVectors);
                           break;
                       }
                   }
               }*/

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