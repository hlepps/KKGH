using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Map : MonoBehaviour
{
    static Map instance;
    public static Map GetInstance() { return instance; }   

    [SerializeField] Noise noise;
    [SerializeField] GameObject chunkPrefab;
    Chunk defaultChunk;
    List<GameObject> chunks = new List<GameObject>();
    [SerializeField] uint3 mapChunkSize = new uint3(1, 1, 1);
    [SerializeField] float chunkScale = 1.0f;
    public uint3 GetMapChunkSize() { return mapChunkSize; }
    public uint3 GetMapSize() 
    {
        return new uint3(
            mapChunkSize.x * (uint)defaultChunk.GetSize(),
            mapChunkSize.y * (uint)defaultChunk.GetSize(),
            mapChunkSize.z * (uint)defaultChunk.GetSize()); 
    }
    public GameObject GetChunkObject(int x, int y, int z)
    {
        return chunks[x + (int)mapChunkSize.y * (y + (int)mapChunkSize.z * z)];
    }
    public int GetChunkSize() { return defaultChunk.GetSize(); }
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            defaultChunk = chunkPrefab.GetComponent<Chunk>();
            GenerateMap();
        }
        else
            Destroy(this.gameObject);
    }

    void GenerateMap()
    {
        StartCoroutine(IterateChunks());
    }

    IEnumerator IterateChunks()
    {
        for (int x = 0; x < mapChunkSize.x; x++)
        {
            for (int y = 0; y < mapChunkSize.y; y++)
            {
                for (int z = 0; z < mapChunkSize.z; z++)
                {
                    GameObject temp = Instantiate(chunkPrefab, this.transform);
                    Chunk chunk = temp.GetComponent<Chunk>();
                    chunk.GetChunkBox().InitChunkBox(chunk, chunk.GetSize());
                    int sub = Mathf.Max(x,y,z);
                    
                    temp.transform.localPosition = new Vector3(((x * chunk.GetSize())-x),
                                                                ((y * chunk.GetSize())-y),
                                                                ((z * chunk.GetSize())-z));
                    temp.name = $"Chunk {x},{y},{z}";
                    chunks.Add(temp);
                    StartCoroutine(GenerateChunk(chunk, x, y, z));
                    while (!chunk.IsReady())
                        yield return null;
                }
            }
        }
    }

    IEnumerator GenerateChunk(Chunk chunk, int x, int y, int z)
    {
        yield return null;
        float[] values;
        float[] texturemap;
        (values, texturemap) = noise.GetSurfaceValues(
            new int3((x * defaultChunk.GetSize()) - x,
            (y * defaultChunk.GetSize()) - y,
            (z * defaultChunk.GetSize()) - z),
            (mapChunkSize.y - 1 == y),
            (int3)GetMapSize());
        //Debug.Log(GetMapSize());
        
        chunk.InitChunk(values, new uint3((uint)x, (uint)y, (uint)z), texturemap);

    }


    /// <summary>
    /// Modifies map in the shape of a circle
    /// </summary>
    /// <param name="center">GLOBAL center of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="change">Value change</param>
    public void ModifyCircle(Vector3 center, float radius, float change)
    {
        Collider[] chunks = Physics.OverlapSphere(center, radius, LayerMask.GetMask("ChunkBox"));
        //Debug.Log(chunks.Length);
        foreach (Collider coll in chunks)
        {
            Chunk chunk = coll.GetComponent<ChunkBox>().GetChunk();
            if(chunk != null)
            {
                Vector3 localCeneter = center;
                localCeneter.x -= (chunk.GetSize() * chunk.GetID().x) - chunk.GetID().x;
                localCeneter.y -= (chunk.GetSize() * chunk.GetID().y) - chunk.GetID().y;
                localCeneter.z -= (chunk.GetSize() * chunk.GetID().z) - chunk.GetID().z;
                //Debug.Log(chunk.GetID() + ", " + localCeneter);
                chunk.ModifyCircle(localCeneter, radius, change);
            }
        }
    }

}
