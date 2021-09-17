using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (body.velocity.y >= 20f)
        {
            Vector2 newVelocity = body.velocity;
            newVelocity.y *= Mathf.Clamp01((0.8f - (body.velocity.y - 20f) * 0.01f));
            body.velocity = newVelocity;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        float maxSpeed = 50f;
        Vector2 maxVelocity = body.velocity.normalized * maxSpeed;
        Vector2 newVelocity = body.velocity;

        newVelocity.y = Mathf.Min(maxSpeed, body.velocity.y);

        if (Mathf.Abs(body.velocity.x) > Mathf.Abs(maxVelocity.x))
        {
            newVelocity.x = maxVelocity.x;
        }

        body.velocity = newVelocity;
    }
}
