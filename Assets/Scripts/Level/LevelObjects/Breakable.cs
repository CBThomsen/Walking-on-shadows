using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    private float breakForce = 100f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        MoveableObject m = other.gameObject.GetComponent<MoveableObject>();
        if (m != null)
        {
            float velocityAlongNormal = Mathf.Abs(Vector2.Dot(other.attachedRigidbody.velocity, transform.up));

            if (velocityAlongNormal > 10f)
                Destroy(gameObject);
        }
    }
}
