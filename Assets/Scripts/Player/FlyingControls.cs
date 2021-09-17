using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingControls : InputReciever
{
    public float movementSmoothing;
    public float speed = 100f;

    private Rigidbody2D body;

    private Vector3 velocity;
    private Vector3 lastMousePos = Vector3.zero;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public override void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        if (lastMousePos == Vector3.zero)
            lastMousePos = mousePos;

        Vector3 deltaMousePos = mousePos - lastMousePos;
        deltaMousePos.z = Camera.main.transform.position.z;
        Vector3 origin = Vector3.zero;
        origin.z = Camera.main.transform.position.z;

        Vector3 deltaWorld = Camera.main.ScreenToWorldPoint(deltaMousePos) - Camera.main.ScreenToWorldPoint(origin);
        deltaWorld.z = 0f;

        Move(deltaWorld * 1f / Time.deltaTime);
        lastMousePos = mousePos;
    }

    public override void OnMouseUp()
    {
        body.velocity = Vector3.zero;
        lastMousePos = Vector3.zero;
    }

    private void Move(Vector3 movement)
    {
        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(movement.x, movement.y);
        // And then smoothing it out and applying it to the character
        //body.velocity = targetVelocity;
        body.velocity = Vector3.SmoothDamp(body.velocity, targetVelocity, ref velocity, movementSmoothing);
    }
}