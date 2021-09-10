using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MergeSort;
using UnityEditor;


public class TestShadowPhysics
{
    private ShadowColliders shadowColliders;
    private ShapeCollider collider;
    private Light light;
    private Sprite circleSprite;

    [SetUp]
    public void Setup()
    {
        var sceneGeo = new GameObject("SceneGeometry");
        sceneGeo.AddComponent<SceneGeometry>();

        var camera = new GameObject("Camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>();

        var lightObj = new GameObject("Light");
        lightObj.tag = "Light";
        light = lightObj.AddComponent<Light>();
        lightObj.transform.SetParent(sceneGeo.transform);

        var shadowColObj = new GameObject("ShadowColliders");
        shadowColliders = shadowColObj.AddComponent<ShadowColliders>();

        collider = new ShapeCollider(shadowColObj);

        circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
    }

    [TearDown]
    public void Teardown()
    {

    }

    [UnityTest]
    public IEnumerator TestThatNoTunnelingOccurs()
    {
        int totalFrames = 1000;
        int frames = 0;
        float angle = 0f;

        for (var i = 0; i < 5; i++)
        {
            //Spawn circles
            var circle = new GameObject("c");
            var b = circle.AddComponent<Rigidbody2D>();
            b.bodyType = RigidbodyType2D.Dynamic;
            b.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            circle.AddComponent<CircleCollider2D>();
            var spriteRen = circle.AddComponent<SpriteRenderer>();
            spriteRen.sprite = circleSprite;

            circle.transform.position = new Vector2(2f + i * 1f, 0.1f);
        }

        CreateColliders(angle, 0.1f);
        yield return new WaitForSeconds(0.1f);

        while (frames < totalFrames)
        {
            CreateColliders(angle, Time.deltaTime);
            angle += 15f * Time.deltaTime;
            frames++;
            yield return new WaitForEndOfFrame();
        }
    }

    private void CreateColliders(float angle, float debugDrawTime)
    {
        var edgeDirA = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var edgeLenA = 10f;

        var edgeDirB = new Vector2(Mathf.Cos(angle - 0.5f), Mathf.Sin(angle - 0.5f));
        var edgeLenB = 10f;

        var edgeAv1 = (Vector2)light.transform.position + edgeDirA;
        var edgeAv2 = (Vector2)light.transform.position + edgeDirA * edgeLenA;

        var edgeBv1 = (Vector2)light.transform.position + edgeDirB;
        var edgeBv2 = (Vector2)light.transform.position + edgeDirB * edgeLenB;

        Debug.DrawLine(edgeAv1, edgeAv2, Color.magenta, debugDrawTime);
        Debug.DrawLine(edgeBv1, edgeBv2, Color.magenta, debugDrawTime);

        collider.SetColliderPoints(0, edgeAv1, edgeAv2);
        collider.SetColliderPoints(1, edgeBv1, edgeBv2);
        collider.UpdateColliders();
    }



}