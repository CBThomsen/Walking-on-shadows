using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MergeSort;
using UnityEditor;


public class TestGPUSort
{
    /*[Test]
    public void TestSortFloats()
    {

        System.Array.Copy(data, values.Data, count);
        values.Upload();

        sort.Init(keys.Buffer);
        sort.Sort(keys.Buffer, values.Buffer);

        keys.Download();
        var failed = false;

        var lastVal = values.Data[keys.Data[0]];
        for (var i = 0; i < count; i++)
        {
            var val = values.Data[keys.Data[i]];
            if (val < lastVal)
            {
                failed = true;
                Debug.LogErrorFormat("Unexpected Key {0} at {1}", val, i);
            }

            lastVal = val;
        }

        Debug.LogFormat("Sort Test Result = {0}", (failed ? "Wrong" : "Correct"));

        sort.Dispose();
        keys.Dispose();
        values.Dispose();
    }

    [Test]
    public void TestSortEdgeVertices()
    {
        var count = 1 << 2;
        Debug.LogFormat("Count={0}", count);

        ComputeShader compute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/BitonicMergeSort.compute");

        BitonicMergeSort sort = new BitonicMergeSort(compute);
        DisposableBuffer<uint> keys = new DisposableBuffer<uint>(count);

        ComputeBuffer valueBuffer = new ComputeBuffer(count, 2 * sizeof(float) + sizeof(int));

        var data = new EdgeVertex[count];
        for (var i = 0; i < count; i++)
        {
            data[i] = new EdgeVertex();
            data[i].position = new Vector2(1f, 1f);
            data[i].shapeIndex = Mathf.RoundToInt(100 * Random.value);

            Debug.Log("Before sort: " + i + " = " + data[i].shapeIndex);
        }

        valueBuffer.SetData(data);

        sort.Init(keys.Buffer);
        sort.SortEdgeVertices(keys.Buffer, valueBuffer);

        keys.Download();
        EdgeVertex[] newData = new EdgeVertex[count];
        valueBuffer.GetData(newData);

        for (var i = 0; i < count; i++)
        {
            Debug.Log("After sort: " + i + " = " + newData[i].shapeIndex);
        }

        var failed = false;
        var lastVal = data[keys.Data[0]].shapeIndex;
        for (var i = 0; i < count; i++)
        {
            var val = data[keys.Data[i]].shapeIndex;
            if (val < lastVal)
            {
                failed = true;
                Debug.LogErrorFormat("Unexpected Key {0} at {1}", val, i);
            }

            lastVal = val;
        }

        Debug.LogFormat("Sort Test Result = {0}", (failed ? "Wrong" : "Correct"));

        sort.Dispose();
        keys.Dispose();
        valueBuffer.Release();
    }*/
}