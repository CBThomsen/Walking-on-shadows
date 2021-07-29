using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public FlyingControls flyingControls;
    private CharacterController characterController;

    private bool controllingLight;

    private float moveSpeed = 35f;
    private float flyingSpeed = 25f;
    private float horizontalMove = 0f;
    private float verticalMove = 0f;
    private bool jump = false;

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
    }

    void FixedUpdate()
    {
        if (controllingLight)
        {
            flyingControls.Move(horizontalMove * flyingSpeed * Time.fixedDeltaTime, verticalMove * flyingSpeed * Time.fixedDeltaTime);
        }
        else
        {
            characterController.Move(horizontalMove * moveSpeed * Time.fixedDeltaTime, false, jump);
        }

        jump = false;
    }

}