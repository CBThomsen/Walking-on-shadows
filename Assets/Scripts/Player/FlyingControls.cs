using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingControls : MonoBehaviour
{
    public float movementSmoothing;
    private Rigidbody2D body;

    private Vector3 velocity;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Move(float horizontal, float vertical)
    {
        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(horizontal * 10f, vertical * 10f);
        // And then smoothing it out and applying it to the character
        body.velocity = Vector3.SmoothDamp(body.velocity, targetVelocity, ref velocity, movementSmoothing);
    }
}
