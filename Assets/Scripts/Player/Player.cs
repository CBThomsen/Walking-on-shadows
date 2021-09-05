using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGraphic;

    [SerializeField]
    private InputReciever playerInputReciever;

    private InputReciever inputReciever;

    private bool controllingPlayer;

    void Start()
    {
        controllingPlayer = true;
        inputReciever = GetComponent<InputReciever>();
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.X))
        {
            if (controllingPlayer)
            {
                InputReciever newInputReciever = FindNewInputReciever();
                if (newInputReciever != null)
                {
                    inputReciever = newInputReciever;
                    controllingPlayer = false;
                    PlayerControlChanged();
                }
            }
            else
            {
                controllingPlayer = true;
                PlayerControlChanged();
                inputReciever = playerInputReciever;
            }
        }

        if (Input.GetMouseButton(0))
            inputReciever.OnMouseDown();

        inputReciever.OnHorizontalKeyDown(Input.GetAxisRaw("Horizontal"));
        inputReciever.OnVerticalKeyDown(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
            inputReciever.OnJumpDown();
    }

    private InputReciever FindNewInputReciever()
    {
        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 0.25f);
        InputReciever newInputReciever = null;

        for (var i = 0; i < overlappingColliders.Length; i++)
        {
            newInputReciever = overlappingColliders[i].GetComponent<InputReciever>();

            if (newInputReciever != null)
                return newInputReciever;
        }

        return newInputReciever;
    }

    private void PlayerControlChanged()
    {
        if (controllingPlayer)
        {
            playerGraphic.SetActive(true);
            transform.position = inputReciever.transform.position;
        }
        else
        {
            playerGraphic.SetActive(false);
        }
    }
}