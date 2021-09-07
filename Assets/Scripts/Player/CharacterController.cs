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
    private float maxSlopeAngle = 45f;

    [Range(0, .3f)]
    [SerializeField] private float movementSmoothing = .05f;

    [SerializeField]
    private bool airControl = false;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private Transform groundTransform;

    private const float groundedRadius = 0.3f;
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

        /*Collider2D[] colliders = Physics2D.OverlapCircleAll(groundTransform.position, groundedRadius, groundLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
            }
        }*/

        float newSlopeAngle = float.MaxValue;

        Vector3 leftLeg = transform.position + Vector3.left * 0.2f;
        RaycastHit2D[] hitsLeftLeg = Physics2D.RaycastAll(leftLeg, -transform.up, groundedRadius, groundLayerMask);
        Debug.DrawLine(leftLeg, leftLeg + -transform.up * groundedRadius);

        Vector3 rightLeg = transform.position + Vector3.right * 0.2f;
        RaycastHit2D[] hitsRightLeg = Physics2D.RaycastAll(rightLeg, -transform.up, groundedRadius, groundLayerMask);
        Debug.DrawLine(rightLeg, rightLeg + -transform.up * groundedRadius);

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
}