using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] Noise noise;
    [SerializeField] GameObject chunkPrefab;
    Chunk defaultChunk;
    List<GameObject> chunks = new List<GameObject>();
    [SerializeField] uint3 mapChunkSize = new uint3(1, 1, 1);
    [SerializeField] float chunkScale = 1.0f;
    private void Start()
    {
        defaultChunk = chunkPrefab.GetComponent<Chunk>();
        GenerateMap();
    }

    void GenerateMap()
    {
        for(int x = 0; x < mapChunkSize.x; x++)
        {
            for (int y = 0; y < mapChunkSize.y; y++)
            {
                for (int z = 0; z < mapChunkSize.z; z++)
                {
                    GameObject temp = Instantiate(chunkPrefab, this.transform);
                    Chunk chunk = temp.GetComponent<Chunk>();
                    temp.transform.localPosition = new Vector3(x * chunk.GetSize() * chunkScale,
                                                                y * chunk.GetSize() * chunkScale, 
                                                                z * chunk.GetSize() * chunkScale);
                    chunks.Add(temp);
                    StartCoroutine(GenerateChunk(chunk, x, y, z));
                    //GenerateChunk(chunk, x, y, z);
                }
            }
        }
    }

    IEnumerator GenerateChunk(Chunk chunk, int x, int y, int z)
    {
        yield return null;
        float[] values = noise.GetNoiseValues(x * defaultChunk.GetSize(), y * defaultChunk.GetSize(), z * defaultChunk.GetSize());
        chunk.GenerateMesh(values);

    }
}
