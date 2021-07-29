using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowColliders : MonoBehaviour
{
    public GameObject prefab;

    public void ResetColliders()
    {
        PolygonCollider2D polyCol = prefab.GetComponent<PolygonCollider2D>();
        polyCol.pathCount = 0;
    }

    public void AddPointsToCollider(int index, Vector2[] points)
    {
        PolygonCollider2D polyCol = prefab.GetComponent<PolygonCollider2D>();

        if (index >= polyCol.pathCount)
        {
            polyCol.pathCount = index + 1;
        }

        polyCol.SetPath(index, points);
    }

}