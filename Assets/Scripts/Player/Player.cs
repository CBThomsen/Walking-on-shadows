using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGraphic;

    [SerializeField]
    private InputReciever playerInputReciever;

    private InputReciever inputReciever;

    private SceneGeometry sceneGeometry;

    private Light carryingLight = null;

    private LightSystem lightSystem;

    private Light lastSpawnedLight;
    private List<Light> spanwedLights;
    private int maxLights = 1;

    [Inject]
    public void Construct(SceneGeometry sceneGeometry, LightSystem lightSystem)
    {
        this.sceneGeometry = sceneGeometry;
        this.lightSystem = lightSystem;
    }

    void Awake()
    {
        spanwedLights = new List<Light>();
        Spawn();
    }

    private void Spawn()
    {
        spanwedLights.ForEach(l => Destroy(l.gameObject));
        this.transform.position = sceneGeometry.GetSpawnPoint();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            inputReciever = FindOverlappingComponentAtMouse<InputReciever>();

        if (inputReciever != null)
        {
            if (Input.GetMouseButton(0))
                inputReciever.OnMouseDown();

            if (Input.GetMouseButtonUp(0))
                inputReciever.OnMouseUp();
        }
        else
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (spanwedLights.Count >= maxLights)
                {
                    Light lightToDespawn = spanwedLights[0];
                    StartCoroutine(lightToDespawn.Despawn());
                    spanwedLights.RemoveAt(0);
                }

                lastSpawnedLight = lightSystem.SpawnLight(SpaceConverter.MouseToWorldPosition(Input.mousePosition));
                lastSpawnedLight.SetCollidersEnabled(false);
            }

            if (Input.GetMouseButton(0))
            {
                lastSpawnedLight.transform.position = SpaceConverter.MouseToWorldPosition(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                lastSpawnedLight.SetCollidersEnabled(true);
                spanwedLights.Add(lastSpawnedLight);
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Light light = FindOverlappingComponentAtPosition<Light>(transform.position);
            if (carryingLight == null)
            {
                if (light)
                {
                    light.PickUp(transform);
                    carryingLight = light;
                }
            }
            else
            {
                carryingLight.Drop();
                carryingLight = null;
            }
        }

        if (carryingLight != null)
        {
            Vector3 lookDir = transform.position - SpaceConverter.MouseToWorldPosition(Input.mousePosition);
            carryingLight.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(lookDir.y, lookDir.x) * 180f / Mathf.PI);
        }

        playerInputReciever.OnHorizontalKeyDown(Input.GetAxisRaw("Horizontal"));
        playerInputReciever.OnVerticalKeyDown(Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            playerInputReciever.OnJumpDown();
        }
    }

    private T FindOverlappingComponentAtMouse<T>()
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

    private T FindOverlappingComponentAtPosition<T>(Vector2 position)
    {
        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(position, 1f);
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