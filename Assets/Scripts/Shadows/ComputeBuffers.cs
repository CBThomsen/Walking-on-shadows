using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ComputeBuffers : ITickable
{
    private SceneGeometry sceneGeometry;
    private ComputeBuffer lightBuffer = null;
    private ComputeBuffer boxBuffer = null;

    private Light[] lights;
    private Box[] boxes;

    private LightData[] lightDataArray;
    private BoxData[] boxDataArray;

    [Inject]
    public void Construct(SceneGeometry sceneGeometry)
    {
        this.sceneGeometry = sceneGeometry;
    }

    private void Dispose()
    {
        lightBuffer.Release();
        boxBuffer.Release();
    }

    public ComputeBuffer GetLightBuffer()
    {
        return lightBuffer;
    }

    public ComputeBuffer GetBoxBuffer()
    {
        return boxBuffer;
    }

    private void CreateOrResizeComputeBuffers()
    {
        lights = sceneGeometry.GetLights();

        if (lightDataArray == null || lights.Length != lightDataArray.Length)
        {
            if (lightBuffer != null)
                lightBuffer.Release();

            lightDataArray = new LightData[lights.Length];
            lightBuffer = new ComputeBuffer(lightDataArray.Length, 9 * sizeof(float) + sizeof(int));
        }

        boxes = sceneGeometry.GetBoxes();

        if (boxDataArray == null || boxes.Length != boxDataArray.Length)
        {
            if (boxBuffer != null)
                boxBuffer.Release();

            boxDataArray = new BoxData[boxes.Length];
            boxBuffer = new ComputeBuffer(boxDataArray.Length, boxDataArray.Length * 4 * sizeof(float));
        }
    }

    private void UpdateBuffers()
    {
        for (var i = 0; i < lights.Length; i++)
        {
            LightData data = lights[i].GetLightData();
            lightDataArray[i] = data;
        }

        for (var i = 0; i < boxes.Length; i++)
        {
            BoxData data = boxes[i].GetBoxData();
            boxDataArray[i] = data;
        }
    }

    public void Tick()
    {
        CreateOrResizeComputeBuffers();

        UpdateBuffers();

        lightBuffer.SetData(lightDataArray);
        boxBuffer.SetData(boxDataArray);
    }

}
