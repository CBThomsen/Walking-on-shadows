using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform targetA;
    public Transform targetB;

    void Start()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, targetA.transform.position, 1f);
        newPos.z = transform.position.z;

        transform.position = newPos;
    }

    void FixedUpdate()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, targetA.transform.position, 0.1f);
        newPos.z = transform.position.z;

        transform.position = newPos;
    }
}
