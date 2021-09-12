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

public class ShadowCollidersSimple : MonoBehaviour
{
    private LightData[] lights;
    private BoxData[] boxes;

    private List<ShapeCollider> shapeColliders;

    private void Start()
    {
        shapeColliders = new List<ShapeCollider>();

        lights = SceneGeometry.instance.GetLightDataArray();
        boxes = SceneGeometry.instance.GetBoxDatas();

        int shapeIndex = 0;

        for (var i = 0; i < lights.Length; i++)
        {
            for (var j = 0; j < boxes.Length; j++)
            {
                ShapeCollider col = new ShapeCollider(gameObject);
                shapeColliders.Add(col);
                shapeIndex += 1;
            }
        }
    }

    private void Update()
    {
        lights = SceneGeometry.instance.GetLightDataArray();
        boxes = SceneGeometry.instance.GetBoxDatas();

        int shapeIndex = 0;
        for (var i = 0; i < lights.Length; i++)
        {
            for (var j = 0; j < boxes.Length; j++)
            {
                UpdateBoxShadowCollider(shapeColliders[shapeIndex], lights[i], boxes[j]);
                shapeColliders[shapeIndex].SetEnabled(true);
                shapeIndex += 1;
            }
        }
    }

    public void UpdateBoxShadowCollider(ShapeCollider shapeCollider, LightData light, BoxData box)
    {
        List<Vector2> boxCorners = new List<Vector2>();
        boxCorners.Add((box.center + new Vector2(box.extents.x, box.extents.y)));
        boxCorners.Add((box.center + new Vector2(-box.extents.x, box.extents.y)));
        boxCorners.Add((box.center + new Vector2(-box.extents.x, -box.extents.y)));
        boxCorners.Add((box.center + new Vector2(box.extents.x, -box.extents.y)));

        for (var i = 0; i < boxCorners.Count; i++)
        {
            Vector2 distToCorner = boxCorners[i] - light.position;
            float length = Mathf.Max(light.range - distToCorner.magnitude, 0f);

            shapeCollider.SetColliderPoints(i, SpaceConverter.TextureToWorldSpace(boxCorners[i]), SpaceConverter.TextureToWorldSpace(boxCorners[i] + length * distToCorner.normalized));
        }

        shapeCollider.UpdateColliders();
    }

}