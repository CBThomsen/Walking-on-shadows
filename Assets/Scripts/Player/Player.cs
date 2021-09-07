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

    void Start()
    {
        inputReciever = GetComponent<InputReciever>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            inputReciever.OnMouseDown();

        if (Input.GetKeyDown(KeyCode.X))
        {
            Light light = FindOverlappingComponent<Light>();

            if (light)
                light.ToggleOnOff();
        }

        inputReciever.OnHorizontalKeyDown(Input.GetAxisRaw("Horizontal"));
        inputReciever.OnVerticalKeyDown(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            inputReciever.OnJumpDown();
        }
    }

    private T FindOverlappingComponent<T>()
    {
        /*Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);*/

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        T component;

        for (var i = 0; i < overlappingColliders.Length; i++)
        {
            component = overlappingColliders[i].GetComponent<T>();

            if (component != null)
                return component;
        }

        return default(T);
    }
}