using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using MergeSort;

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