using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Noise : MonoBehaviour
{
    [SerializeField] ComputeShader noiseCS;
    [SerializeField] ComputeShader surfaceCS;
    ComputeBuffer valuesBuffer;

    [SerializeField] int size = 16;
    [SerializeField] int numberOfThreads = 8;
    public int GetSize() { return size; }

    private void OnEnable()
    {
        valuesBuffer = new ComputeBuffer(size * size * size, sizeof(float));
    }

    private void OnDisable()
    {
        valuesBuffer.Release();
        valuesBuffer = null;
    }

    public (float[] noise, float[] layers) GetNoiseValues(int offsetX, int offsetY, int offsetZ, int seed = 1170)
    {
        float[] noiseValues = new float[size * size * size];
        float[] layersValues = new float[size * size * size];
        
        noiseCS.SetBuffer(0, "_Values", valuesBuffer);
        noiseCS.SetInt("_Size", size);
        noiseCS.SetFloat("_Frequency", 0.02f);
        noiseCS.SetInt("_Octaves", 1);
        noiseCS.SetInt("_Seed", seed);
        noiseCS.SetInts("_Offset", new int[] {offsetX,offsetY,offsetZ});
        int dispatch = size / numberOfThreads;
        noiseCS.Dispatch(0, dispatch, dispatch, dispatch);
        valuesBuffer.GetData(noiseValues);
        
        noiseCS.SetFloat("_Frequency", 0.02f);
        noiseCS.SetInt("_Seed", seed*seed);
        noiseCS.Dispatch(0, dispatch, dispatch, dispatch);
        valuesBuffer.GetData(layersValues);

        return (noiseValues, layersValues);
    }
    public (float[] noise, float[] layers) GetSurfaceValues(int offsetX, int offsetY, int offsetZ, int seed = 1170)
    {
        float[] values = new float[size * size * size];

        surfaceCS.SetBuffer(0, "_Values", valuesBuffer);
        surfaceCS.SetInt("_Size", size);
        surfaceCS.SetFloat("_Frequency", 0.02f);
        surfaceCS.SetInt("_Octaves", 1);
        surfaceCS.SetInt("_Seed", seed);
        surfaceCS.SetInts("_Offset", new int[] {offsetX,offsetY,offsetZ });
        int dispatch = size / numberOfThreads;
        surfaceCS.Dispatch(0, dispatch, dispatch, dispatch);
        valuesBuffer.GetData(values);
        return (values, Enumerable.Repeat(2f, size*size*size).ToArray());
    }
}
