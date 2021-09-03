using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public FlyingControls flyingControls;
    private CharacterController characterController;

    private bool controllingLight;
    private bool mouseDown;

    private float moveSpeed = 35f;
    private float flyingSpeed = 45f;
    private float horizontalMove = 0f;
    private float verticalMove = 0f;
    private bool jump = false;

    private Vector3 targetPosition;

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");

        jump = Input.GetButtonDown("Jump");

        if (Input.GetKeyUp(KeyCode.X))
        {
            controllingLight = !controllingLight;
        }

        mouseDown = Input.GetMouseButton(0);

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        targetPosition = mouseWorldPos - flyingControls.transform.position;
    }

    void FixedUpdate()
    {
        if (controllingLight)
        {
            Vector3 dir = targetPosition;
            dir.Normalize();
            flyingControls.transform.rotation = Quaternion.Lerp(flyingControls.transform.rotation, Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 180f / Mathf.PI), 0.25f);

            if (mouseDown)
            {
                flyingControls.Move(dir.x * flyingSpeed * Time.fixedDeltaTime, dir.y * flyingSpeed * Time.fixedDeltaTime);
            }
        }

        characterController.Move(horizontalMove * moveSpeed * Time.fixedDeltaTime, false, jump);
        jump = false;
    }
}