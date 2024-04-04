using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Noise : MonoBehaviour
{
    [SerializeField] ComputeShader noiseCS;
    ComputeBuffer valuesBuffer;

    [SerializeField] int size = 16;
    [SerializeField] int numberOfThreads = 8;
    [SerializeField] int seed = 2137;
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

    public float[] GetNoiseValues(int offsetX, int offsetY, int offsetZ)
    {
        float[] values = new float[size * size * size];
        
        noiseCS.SetBuffer(0, "_Values", valuesBuffer);
        noiseCS.SetInt("_Size", size);
        noiseCS.SetFloat("_Frequency", 0.02f);
        noiseCS.SetInt("_Octaves", 1);
        noiseCS.SetInt("_Seed", seed);
        noiseCS.SetInts("_Offset", new int[] {offsetX,offsetY,offsetZ});
        int dispatch = size / numberOfThreads;
        noiseCS.Dispatch(0, dispatch, dispatch, dispatch);
        valuesBuffer.GetData(values);
        return values;
        
        /*
        for (int x = 0; x < size; x ++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    values[x + size * (y + size * z)] = Mathf.Clamp(1 / (float)x, 0, 1);
                }
            }
        }
        return values;
        */
    }
}
