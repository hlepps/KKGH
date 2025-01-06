using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ChunkList
{
    const int offset = 64;
    private GameObject[,,] array = new GameObject[128, 128, 128];
    public GameObject this[int x, int y, int z]
    {
        get { return array[x + offset, y + offset, z + offset]; }
    }

    public void Add(int x, int y, int z, GameObject chunk)
    {
        array[x + offset, y + offset, z + offset] = chunk;
    }

    public int CountNotNull()
    {
        int c = 0;
        foreach (var item in array)
        {
            if (item != null) c++;
        }
        return c;
    }
}

public class Map : MonoBehaviour
{
    static Map instance;
    public static Map GetInstance() { return instance; }

    [SerializeField] Noise noise;
    [SerializeField] CaveCarver caveCarver;
    [SerializeField] GameObject chunkPrefab;
    Chunk defaultChunk;
    ChunkList chunks = new();
    public ChunkList GetChunks() { return chunks; }
    [SerializeField] uint3 mapChunkSize = new uint3(1, 1, 1);
    [SerializeField] float chunkScale = 1.0f;
    [SerializeField] GameObject playerObject;

    bool playerSpawned = false;
    public void SetPlayerSpawned()
    {
        playerSpawned = true;
        StartCoroutine(ChunkUpdater());
        CollectibleSpawner.Instance.SpawnCollectible();
        GhostController.instance.StartGhostSpawning();
    }

    int seed = 0;

    public uint3 GetMapChunkSize() { return mapChunkSize; }
    public int GetMapChunkLength() { return (int)(mapChunkSize.x * mapChunkSize.y * mapChunkSize.z); }
    public uint3 GetMapSize()
    {
        return new uint3(
            mapChunkSize.x * (uint)defaultChunk.GetSize(),
            mapChunkSize.y * (uint)defaultChunk.GetSize(),
            mapChunkSize.z * (uint)defaultChunk.GetSize());
    }
    public GameObject GetChunkObject(int x, int y, int z)
    {
        return chunks[x, y, z];
    }
    public int GetChunkSize() { return defaultChunk.GetSize(); }
    private void Start()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        if (instance == null)
        {
            instance = this;
            defaultChunk = chunkPrefab.GetComponent<Chunk>();
            caveCarver = GetComponent<CaveCarver>();
            caveCarver.bounds = new Vector3(mapChunkSize.x, mapChunkSize.y, mapChunkSize.z) * defaultChunk.GetSize();
            GenerateMap();
        }
        else
            Destroy(this.gameObject);

    }

    float timer = 0;

    IEnumerator ChunkUpdater()
    {
        while (playerSpawned)
        {
            if (timer >= 1)
            {
                timer = 0;
                Vector3 playerPos = playerObject.transform.position;
                int3 playerChunkPos = new();
                playerChunkPos.x = (int)playerPos.x / defaultChunk.GetSize();
                playerChunkPos.y = (int)playerPos.y / defaultChunk.GetSize();
                playerChunkPos.z = (int)playerPos.z / defaultChunk.GetSize();
                //Debug.Log(playerChunkPos);

                for (int x = playerChunkPos.x - 2; x <= playerChunkPos.x + 2; x++)
                {
                    for (int y = playerChunkPos.y - 2; y <= playerChunkPos.y + 2; y++)
                    {
                        for (int z = playerChunkPos.z - 2; z <= playerChunkPos.z + 2; z++)
                        {
                            if (!GetChunkObject(x, y, z))
                            {
                                StartCoroutine(InitNewChunk(x, y, z));
                                yield return null;
                            }
                        }
                    }
                }
            }
            yield return null;
            timer += Time.deltaTime;
        }
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
                    StartCoroutine(InitNewChunk(x, y, z));
                    yield return null;
                }
            }
        }
        caveCarver.GenerateCaves();
    }

    IEnumerator InitNewChunk(int x, int y, int z)
    {
        GameObject temp = Instantiate(chunkPrefab, this.transform);
        Chunk chunk = temp.GetComponent<Chunk>();
        chunk.GetChunkBox().InitChunkBox(chunk, chunk.GetSize());
        int sub = Mathf.Max(x, y, z);

        temp.transform.localPosition = new Vector3(((x * chunk.GetSize()) - x),
                                                    ((y * chunk.GetSize()) - y),
                                                    ((z * chunk.GetSize()) - z));
        temp.name = $"Chunk {x},{y},{z}";
        chunks.Add(x, y, z, temp);
        StartCoroutine(GenerateChunk(chunk, x, y, z));
        while (!chunk.IsReady())
            yield return null;
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
            (mapChunkSize.y <= y),
            (int3)GetMapSize(),
            seed);
        //Debug.Log(GetMapSize());

        chunk.InitChunk(values, new int3(x, y, z), texturemap);
        chunk.UpdateNormals();

    }
    public float GetPointValue(Vector3 point)
    {
        Collider[] chunks = Physics.OverlapSphere(point, 0.01f, LayerMask.GetMask("ChunkBox"));
        //Debug.Log(chunks.Length);
        Collider coll = chunks[0];

        //Debug.Log(coll, coll);
        Chunk chunk = coll.GetComponent<ChunkBox>().GetChunk();

        if (chunk != null)
        {
            int3 localCeneter = new int3((int)point.x, (int)point.y, (int)point.z);
            localCeneter.x -= (chunk.GetSize() * chunk.GetID().x) - chunk.GetID().x;
            localCeneter.y -= (chunk.GetSize() * chunk.GetID().y) - chunk.GetID().y;
            localCeneter.z -= (chunk.GetSize() * chunk.GetID().z) - chunk.GetID().z;
            //Debug.Log(chunk.GetID() + ", " + localCeneter);
            return chunk.GetPointValue(localCeneter);
        }
        else return 0;

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
            //Debug.Log(coll, coll);
            Chunk chunk = coll.GetComponent<ChunkBox>().GetChunk();
            if (chunk != null)
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
    /// <summary>
    /// Updates normals in chunks inside the sphere
    /// </summary>
    /// <param name="center">GLOBAL center of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    public void UpdateNormals(Vector3 center, float radius)
    {
        Collider[] chunks = Physics.OverlapSphere(center, radius, LayerMask.GetMask("ChunkBox"));
        //Debug.Log(chunks.Length);
        foreach (Collider coll in chunks)
        {
            Chunk chunk = coll.GetComponent<ChunkBox>().GetChunk();
            if (chunk != null)
            {
                chunk.UpdateNormals();
            }
        }
    }

}
