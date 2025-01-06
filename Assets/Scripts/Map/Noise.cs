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
    ComputeBuffer textureMapBuffer;

    [SerializeField] int size = 32;
    [SerializeField] int numberOfThreads = 8;
    public int GetSize() { return size; }

    private void OnEnable()
    {
        valuesBuffer = new ComputeBuffer(size * size * size, sizeof(float));
        textureMapBuffer = new ComputeBuffer(size * size * size, sizeof(float));
    }

    private void OnDisable()
    {
        valuesBuffer.Release();
        valuesBuffer = null;
        textureMapBuffer.Release();
        textureMapBuffer = null;
    }

    public (float[] noise, float[] layers) GetSurfaceValues(int3 offset, bool isSurface, bool isEmpty, int3 mapsize, int seed = 1170)
    {
        float[] noiseValues = new float[size * size * size];
        float[] layersValues = new float[size * size * size];

        surfaceCS.SetBuffer(0, "_Values", valuesBuffer);
        surfaceCS.SetBuffer(0, "_TextureMap", textureMapBuffer);
        surfaceCS.SetInt("_Size", size);
        surfaceCS.SetFloat("_Frequency", 0.02f);
        surfaceCS.SetInt("_Octaves", 1);
        surfaceCS.SetInt("_Seed", seed);
        surfaceCS.SetInts("_Offset", new int[] {offset.x, offset.y, offset.z });
        surfaceCS.SetInts("_MapSize", new int[] {mapsize.x, mapsize.y, mapsize.z });
        surfaceCS.SetBool("_SurfaceLevel", isSurface);
        surfaceCS.SetBool("_EmptyChunk", isEmpty);

        int dispatch = size / numberOfThreads;
        surfaceCS.Dispatch(0, dispatch, dispatch, dispatch);

        valuesBuffer.GetData(noiseValues);
        textureMapBuffer.GetData(layersValues); 

        return (noiseValues, layersValues);
    }
}
