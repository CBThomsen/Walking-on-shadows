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
        if (Input.GetMouseButtonDown(0))
        {
            InputReciever newInputReciever = FindNewInputReciever();
            if (newInputReciever != null)
            {
                inputReciever = newInputReciever;
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
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(mouseWorldPos, 0.25f);
        InputReciever newInputReciever = null;

        for (var i = 0; i < overlappingColliders.Length; i++)
        {
            newInputReciever = overlappingColliders[i].GetComponent<InputReciever>();

            if (newInputReciever != null)
                return newInputReciever;
        }

        return newInputReciever;
    }
}