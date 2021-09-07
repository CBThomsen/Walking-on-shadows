using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class ShadowColliders : MonoBehaviour
{
    public GameObject prefab;
    public bool debugDrawCorners;
    public bool debugDrawEdgeVertices;

    private int edgeVertexCount;
    private EdgeVertex[] edgeVertices;

    private List<Vector2> centroids;
    private List<List<EdgeVertex>> shapeEdgeVertices;
    private List<EdgeVertex> corners = new List<EdgeVertex>();

    private void Start()
    {
        centroids = new List<Vector2>();
        shapeEdgeVertices = new List<List<EdgeVertex>>();
        corners = new List<EdgeVertex>();
    }

    public void UpdateColliders(EdgeVertex[] shadowEdgeVertices, int vertexCount)
    {
        edgeVertices = shadowEdgeVertices;
        edgeVertexCount = vertexCount;

        shapeEdgeVertices = SortEdgeVerticesByAngle();
        ResetColliders();

        for (var i = 0; i < shapeEdgeVertices.Count; i++)
        {
            List<EdgeVertex> ev = shapeEdgeVertices[i];
            List<EdgeVertex> corners = FindCorners(ev);

            if (corners.Count == 0)
                continue;

            //Convert to world space array
            Vector2[] worldSpaceCorners = corners.Select(v => SpaceConverter.TextureToWorldSpace(v.position)).ToArray();
            AddPointsToCollider(worldSpaceCorners);

            if (debugDrawEdgeVertices)
                DebugDraw(ev);

            if (debugDrawCorners)
                DebugDraw(corners);
        }
    }

    private List<List<EdgeVertex>> SortEdgeVerticesByAngle()
    {
        centroids.Clear();
        shapeEdgeVertices.Clear();

        //Locate centroids for each shape
        for (var i = 0; i < edgeVertexCount; i++)
        {
            int shapeIndex = edgeVertices[i].shapeIndex;

            while (shapeIndex > centroids.Count - 1)
            {
                centroids.Add(Vector2.zero);
                shapeEdgeVertices.Add(new List<EdgeVertex>());
            }

            Vector2 c = centroids[shapeIndex];
            c += edgeVertices[i].position;

            centroids[shapeIndex] = c;
            shapeEdgeVertices[shapeIndex].Add(edgeVertices[i]);
        }

        for (var j = 0; j < shapeEdgeVertices.Count; j++)
        {
            Vector2 centroid = centroids[j] / (float)shapeEdgeVertices[j].Count;

            //Order by angle to centroid
            shapeEdgeVertices[j] = shapeEdgeVertices[j].OrderBy(v =>
            {
                Vector2 distToCenter = v.position - centroid;
                float angleToCenter = Mathf.Atan2(distToCenter.y, distToCenter.x) + 2 * Mathf.PI % (2 * Mathf.PI);
                return angleToCenter;
            }).ToList();
        }

        return shapeEdgeVertices;
    }

    private List<EdgeVertex> FindCorners(List<EdgeVertex> ev)
    {
        corners.Clear();

        float lastAngle = 0f;

        for (var i = 1; i < ev.Count + 1; i++)
        {
            int curIndex = i == ev.Count ? 0 : i;

            Vector2 lastPos = new Vector2(ev[i - 1].position.x, ev[i - 1].position.y);
            Vector2 curPos = new Vector2(ev[curIndex].position.x, ev[curIndex].position.y);

            Vector2 deltaPos = curPos - lastPos;
            float angle = (Mathf.Atan2(deltaPos.y, deltaPos.x) + Mathf.PI) % (Mathf.PI);

            var deltaAngle = lastAngle - angle;

            if (Mathf.Abs(deltaAngle) * 180 / Mathf.PI > 1f)
            {
                corners.Add(ev[i - 1]);
            }

            lastAngle = angle;
        }

        return corners;
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

    private void DebugDraw(List<EdgeVertex> vertices)
    {
        for (var j = 1; j < vertices.Count; j++)
        {
            Debug.DrawLine(SpaceConverter.TextureToWorldSpace(vertices[j - 1].position), SpaceConverter.TextureToWorldSpace(vertices[j].position), Color.magenta, Time.deltaTime);
        }

        Debug.DrawLine(SpaceConverter.TextureToWorldSpace(vertices[corners.Count - 1].position), SpaceConverter.TextureToWorldSpace(vertices[0].position), Color.magenta, Time.deltaTime);

    }

}