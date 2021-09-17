using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField]
    private GameObject pointGameObject;

    public Transform target;

    private int numPoints;
    private float spacing = 0.15f;
    private List<Transform> pointsTransforms = new List<Transform>();
    private List<Vector2> points = new List<Vector2>();

    void Start()
    {
        numPoints = Mathf.RoundToInt((target.position - transform.position).magnitude / spacing);

        for (var i = 0; i < numPoints; i++)
        {
            var p = Instantiate(pointGameObject);
            p.transform.SetParent(transform);
            p.transform.position += Vector3.down * i * spacing;

            pointsTransforms.Add(p.transform);
            points.Add(p.transform.position);
        }
    }

    private void Solve()
    {
        Vector2 origin = transform.position;
        Vector2 targetPosition = target.position;

        for (int itt = 0; itt < 6; itt++)
        {
            bool startingFromTarget = itt % 2 == 0;
            points.Reverse();

            points[0] = startingFromTarget ? targetPosition : origin;

            for (int i = 1; i < points.Count; i++)
            {
                Vector2 dir = (points[i] - points[i - 1]).normalized;
                points[i] = points[i - 1] + dir * spacing;
            }

            float distanceToTarget = (points[points.Count - 1] - targetPosition).magnitude;
            if (!startingFromTarget && distanceToTarget <= 0.01f)
                return;
        }
    }

    void Update()
    {
        Solve();

        for (var i = 0; i < points.Count; i++)
        {
            pointsTransforms[i].position = points[i];
        }
    }
}
