using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

public class CharacterController : InputReciever
{
    [SerializeField]
    private float jumpForce = 400f;

    [SerializeField]
    private float moveSpeed = 250f;

    [SerializeField]
    private float maxSlopeAngle = 65f;

    [Range(0, .3f)]
    [SerializeField] private float movementSmoothing = .05f;

    [SerializeField]
    private bool airControl = false;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private Transform groundTransform;

    private const float groundedRadius = 0.6f;
    private bool grounded;
    private float slopeAngle;
    private Rigidbody2D body;
    private bool facingRight = true;
    private Vector3 velocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = grounded;
        grounded = false;
        slopeAngle = 0f;

        float newSlopeAngle = float.MaxValue;

        Vector3 leftLeg = transform.position + Vector3.left * 0.2f;
        RaycastHit2D[] hitsLeftLeg = Physics2D.RaycastAll(leftLeg, -transform.up, groundedRadius, groundLayerMask);
        //Debug.DrawLine(leftLeg, leftLeg + -transform.up * groundedRadius);

        Vector3 rightLeg = transform.position + Vector3.right * 0.2f;
        RaycastHit2D[] hitsRightLeg = Physics2D.RaycastAll(rightLeg, -transform.up, groundedRadius, groundLayerMask);
        //Debug.DrawLine(rightLeg, rightLeg + -transform.up * groundedRadius);

        IEnumerable<RaycastHit2D> allHits = hitsLeftLeg.Concat(hitsRightLeg);

        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.collider.gameObject == gameObject)
                continue;

            float angle = Vector3.Angle(hit.normal, transform.up);
            newSlopeAngle = Mathf.Min(angle, newSlopeAngle);
            grounded = true;
        }

        if (grounded)
            slopeAngle = newSlopeAngle;

        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int count = body.GetContacts(contacts);

        float maxAngleBetweenNormals = 0f;

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < count; j++)
            {
                float dot = Vector2.Dot(contacts[i].normal, contacts[j].normal);
                float angle = Mathf.Acos(dot);

                if (Mathf.Abs(angle) > maxAngleBetweenNormals)
                    maxAngleBetweenNormals = Mathf.Abs(angle);
            }
        }

        if (maxAngleBetweenNormals * 180f / Mathf.PI > 165f)
        {
            gameObject.SetActive(false);
        }
    }

    public override void OnHorizontalKeyDown(float horizontal)
    {
        Move(horizontal * moveSpeed * Time.fixedDeltaTime, false);
    }

    public override void OnJumpDown()
    {
        Move(0f, true);
    }

    public void Move(float move, bool jump)
    {
        if ((grounded || airControl) && slopeAngle < maxSlopeAngle)
        {
            Vector3 targetVelocity = new Vector2(move, body.velocity.y);

            if (targetVelocity.y >= 10f)
                targetVelocity.y *= Mathf.Clamp01((0.8f - (targetVelocity.y - 10f) * 0.01f));

            body.velocity = Vector3.SmoothDamp(body.velocity, targetVelocity, ref velocity, movementSmoothing);

            if (move > 0 && !facingRight)
            {
                Flip();
            }
            else if (move < 0 && facingRight)
            {
                Flip();
            }
        }

        if (grounded && jump && slopeAngle < maxSlopeAngle)
        {
            grounded = false;
            body.AddForce(new Vector2(0f, jumpForce));
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
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