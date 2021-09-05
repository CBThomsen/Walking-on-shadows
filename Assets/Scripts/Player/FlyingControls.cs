using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingControls : InputReciever
{
    public float movementSmoothing;
    public float speed = 100f;

    private Rigidbody2D body;

    private Vector3 velocity;
    private Vector3 targetPosition;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public override void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = mouseWorldPos - transform.position;

        if (direction.magnitude > 1f)
            direction.Normalize();

        Move(direction * speed * Time.fixedDeltaTime);
    }

    private void Move(Vector3 movement)
    {
        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(movement.x, movement.y);
        // And then smoothing it out and applying it to the character
        body.velocity = Vector3.SmoothDamp(body.velocity, targetVelocity, ref velocity, movementSmoothing);
    }
}