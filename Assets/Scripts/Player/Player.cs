using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Zenject;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGraphic;

    [SerializeField]
    private InputReciever playerInputReciever;

    private InputReciever inputReciever;

    private SceneGeometry sceneGeometry;

    [Inject]
    public void Construct(SceneGeometry sceneGeometry)
    {
        this.sceneGeometry = sceneGeometry;
    }

    void Awake()
    {
        Spawn();
    }

    private void Spawn()
    {
        this.transform.position = sceneGeometry.GetSpawnPoint();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            inputReciever = FindOverlappingComponent<InputReciever>();

        if (inputReciever != null)
        {
            if (Input.GetMouseButton(0))
                inputReciever.OnMouseDown();

            if (Input.GetMouseButtonUp(0))
                inputReciever.OnMouseUp();

            if (Input.GetKeyDown(KeyCode.X))
            {
                Light light = FindOverlappingComponent<Light>();

                if (light)
                    light.ToggleOnOff();
            }
        }

        playerInputReciever.OnHorizontalKeyDown(Input.GetAxisRaw("Horizontal"));
        playerInputReciever.OnVerticalKeyDown(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            playerInputReciever.OnJumpDown();
        }
    }

    private T FindOverlappingComponent<T>()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(mouseWorldPos, 0.5f);
        T component;

        for (var i = 0; i < overlappingColliders.Length; i++)
        {
            component = overlappingColliders[i].GetComponent<T>();

            if (component != null)
                return component;
        }

        return default(T);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.gameObject.tag == "Enemy")
        {
            Spawn();
        }
    }
}