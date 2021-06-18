using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CPUIntersections : MonoBehaviour
{
    public static int TestRaySphere(Vector2 p, Vector2 dir, Vector2 center, float radius)
    {
        Vector2 m = p - center;
        float b = Vector2.Dot(m, dir);
        float c = Vector2.Dot(m, m) - radius * radius;

        if (c > 0.0 && b > 0.0) return 0;

        float disc = b * b - c;

        if (disc < 0.0) return 0;

        return 1;
    }

}