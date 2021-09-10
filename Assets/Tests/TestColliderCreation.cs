using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MergeSort;
using UnityEditor;
using System.Linq;


public class TestColliderCreation
{

    private SpaceConverter spaceConverter;
    private Camera camera;

    [SetUp]
    public void Setup()
    {
        spaceConverter = new GameObject("spaceConverter").AddComponent<SpaceConverter>();
        camera = new GameObject("Camera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 10;
        camera.gameObject.tag = "MainCamera";
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(spaceConverter.gameObject);
        Object.Destroy(camera.gameObject);
    }

    [Test]
    public void TestCornersOneShape()
    {
        ShadowColliders s = new GameObject("shadowColliders").AddComponent<ShadowColliders>();

        EdgeVertex[] edgeVertices = CreateTestCornerVertices();
        uint[] sortedKeys = new uint[5] { 0, 1, 2, 3, 4 };

        List<List<Vector2>> corners = s.FindCorners(edgeVertices, sortedKeys, 5);

        /*Debug.Log(corners.Count);
        corners.ForEach(c =>
        {
            c.ForEach(p =>
            {
                Debug.Log("Corner =" + p);
            });
        });*/

        Assert.That(corners.Count == 1);
        Assert.That(corners[0].Count == 5);
    }

    [Test]
    public void TestCornersMultipleShapes()
    {
        ShadowColliders s = new GameObject("shadowColliders").AddComponent<ShadowColliders>();

        EdgeVertex[] edgeVertices = CreateTestCornerVertices(2);
        uint[] sortedKeys = new uint[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        List<List<Vector2>> corners = s.FindCorners(edgeVertices, sortedKeys, 10);

        /*Debug.Log(corners.Count);
        corners.ForEach(c =>
        {
            c.ForEach(p =>
            {
                Debug.Log("Corner =" + p);
            });
        });*/

        Assert.That(corners.Count == 2);
        Assert.That(corners[0].Count == 5);
        Assert.That(corners[1].Count == 5);
    }


    [Test]
    public void TestNonCornersRemoved()
    {
        ShadowColliders s = new GameObject("shadowColliders").AddComponent<ShadowColliders>();

        EdgeVertex[] edgeVertices = CreateTestShapeWithCollinearPoints(2);
        uint[] sortedKeys = new uint[24];
        sortedKeys = sortedKeys.Select((s, i) => (uint)i).ToArray();

        List<List<Vector2>> corners = s.FindCorners(edgeVertices, sortedKeys, edgeVertices.Length);

        Debug.Log(corners.Count);
        corners.ForEach(c =>
        {
            c.ForEach(p =>
            {
                Debug.Log("Corner =" + SpaceConverter.WorldToTextureSpace(p));
            });
        });

        Assert.That(corners.Count == 2);
        Assert.That(corners[0].Count == 4);
        Assert.That(corners[1].Count == 4);
    }

    [UnityTest]
    public IEnumerator TestCollidersAligningWithLightCone()
    {
        SceneGeometry sceneGeo = new GameObject("scceneGeometry").AddComponent<SceneGeometry>();
        ShadowColliders s = new GameObject("shadowColliders").AddComponent<ShadowColliders>();

        var lightObj = new GameObject("Light");
        lightObj.tag = "Light";
        lightObj.AddComponent<Light>().range = 10f;
        lightObj.transform.SetParent(sceneGeo.transform);

        var box = new GameObject("Box");
        box.tag = "EnvBox";
        box.AddComponent<BoxCollider2D>();
        box.transform.SetParent(sceneGeo.transform);

        //To run awakes/starts. Needed?
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(5);

        List<Vector2> corners = new List<Vector2>();

        corners.Add(new Vector2(0f, 0f));
        corners.Add(new Vector2(0f, 1f));
        corners.Add(new Vector2(5f, 3f));

        corners.Add(new Vector2(0f, -1f));
        corners.Add(new Vector2(5f, -3f));


    }

    private EdgeVertex[] CreateTestCornerVertices(int shapes = 1)
    {
        Vector2[] positions = new Vector2[5] {
            new Vector2(0f, 1f),
            new Vector2(-0.9f, 0.3f),
            new Vector2(-0.6f, -0.8f),
            new Vector2(0.6f, -0.8f),
            new Vector2(0.9f, 0.3f),
        };

        EdgeVertex[] edgeVertices = new EdgeVertex[5 * shapes];

        for (var i = 0; i < shapes * 5; i++)
        {
            int shapeIndex = Mathf.FloorToInt(i / 5);
            EdgeVertex ev = new EdgeVertex();
            ev.position = positions[i % 5] + new Vector2(5f * shapeIndex, 0f);
            ev.shapeIndex = shapeIndex;
            edgeVertices[i] = ev;
        }

        return edgeVertices;
    }


    private EdgeVertex[] CreateTestShapeWithCollinearPoints(int shapes = 1)
    {
        Vector2[] positions = new Vector2[12] {
            new Vector2(1f, 1f),

            new Vector2(0.5f, 1f),
            new Vector2(-0.5f, 1f),

            new Vector2(-1f, 1f),

            new Vector2(-1f, 0.8f),
            new Vector2(-1f, -0.3f),

            new Vector2(-1f, -1f),

            new Vector2(-0.23f, -1f),
            new Vector2(-0.1f, -1f),

            new Vector2(1f, -1f),

            new Vector2(1f, -0.7f),
            new Vector2(1f, 0.5f),
        };

        int pointsPerShape = positions.Length;

        EdgeVertex[] edgeVertices = new EdgeVertex[pointsPerShape * shapes];

        for (var i = 0; i < shapes * pointsPerShape; i++)
        {
            int shapeIndex = Mathf.FloorToInt(i / pointsPerShape);
            EdgeVertex ev = new EdgeVertex();
            ev.position = (positions[i % pointsPerShape] + new Vector2(5f * shapeIndex, 0f));
            ev.shapeIndex = shapeIndex;
            edgeVertices[i] = ev;
        }

        return edgeVertices;
    }

}