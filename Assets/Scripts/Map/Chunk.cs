using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Rendering;
using Unity.Burst.Intrinsics;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    uint3 id;
    public uint3 GetID() { return id; }
    [SerializeField] ComputeShader chunkCS;
    [SerializeField] ComputeShader updateChunkCS;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer trianglesCountBuffer;
    ComputeBuffer valuesBuffer;
    ComputeBuffer textureMapBuffer;
    ComputeBuffer colorCountBuffer;
    [SerializeField] int size = 16;
    public int GetSize() { return size; }
    [SerializeField] int numberOfThreads = 8;

    float colliderCalculationDelay = 0.2f;
    float colliderCalculationTimer = 0.0f;

    bool ready = false;
    public bool IsReady() { return ready; }

    MeshFilter meshFilter;
    Mesh currentMesh;
    MeshCollider meshCollider;

    float[] values;
    float[] texturemap;
    public int[] lastColorCount { get; private set; }

    [SerializeField] ChunkBox chunkBox;
    public ChunkBox GetChunkBox() { return chunkBox; }

    [SerializeField] bool showDebugValues;
    [SerializeField] bool showDebugNormals;

    Material material;

    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector2 auv;
        public Vector2 buv;
        public Vector2 cuv;

        public static int GetSize() { return (sizeof(float) * 3) * 3 + (sizeof(float) * 2) * 3; }
    }

    public void InitChunk(float[] newvalues, uint3 chunkID, float[] newTexturemap)
    {
        id = chunkID;
        values = new float[size * size * size];
        Array.Copy(newvalues, values, newvalues.Count());
        texturemap = new float[size * size * size];
        Array.Copy(newTexturemap, texturemap, newTexturemap.Count());
        lastColorCount = new int[32];
        GenerateMesh();
    }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        currentMesh = new Mesh();
        material = GetComponent<MeshRenderer>().material;
    }
    private void InitBuffers()
    {
        trianglesBuffer = new ComputeBuffer(5 * (size * size * size), Triangle.GetSize(), ComputeBufferType.Append);
        trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        valuesBuffer = new ComputeBuffer(size * size * size, sizeof(float));
        textureMapBuffer = new ComputeBuffer(size * size * size, sizeof(float));
        colorCountBuffer = new ComputeBuffer(32, sizeof(int));
    }

    private void ReleaseBuffers()
    {
        if (trianglesBuffer != null)
            trianglesBuffer.Release();
        trianglesBuffer = null;

        if (trianglesCountBuffer != null)
            trianglesCountBuffer.Release();
        trianglesCountBuffer = null;

        if (valuesBuffer != null)
            valuesBuffer.Release();
        valuesBuffer = null;

        if (textureMapBuffer != null)
            textureMapBuffer.Release();
        textureMapBuffer = null;

        if (colorCountBuffer != null)
            colorCountBuffer.Release();
        colorCountBuffer = null;
    }

    private void OnEnable()
    {
        InitBuffers();
    }

    private void OnDisable()
    {
        ready = false;
        ReleaseBuffers();
    }

    //wczesniejsze uszykowanie tablicy
    Triangle[] triangles = new Triangle[16384];
    Texture3D floatData;
    public void GenerateMesh()
    {
        ready = false;
        chunkCS.SetBuffer(0, "_Triangles", trianglesBuffer);
        chunkCS.SetBuffer(0, "_Values", valuesBuffer);
        chunkCS.SetInt("_Size", size);
        chunkCS.SetFloat("_IsoLevel", 0.5f);

        valuesBuffer.SetData(values);
        trianglesBuffer.SetCounterValue(0);

        int dispatch = size / numberOfThreads;
        chunkCS.Dispatch(0, dispatch, dispatch, dispatch);

        // pobieranie iloœci trójk¹tów
        // zwyk³e .count podaje maksymalny capacity nie aktualn¹ liczbê
        int[] triangleCount = { 0 };
        ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
        trianglesCountBuffer.GetData(triangleCount);

        // pobieranie trójk¹tów
        trianglesBuffer.GetData(triangles);


        // w³aœciwe generowanie mesha
        UpdateMeshFromTriangles(triangles);
        meshFilter.sharedMesh = currentMesh;
        ready = true;

        // update kolizji po jeœli mo¿na
        if (colliderCalculationTimer <= 0)
            StartCoroutine(UpdateCollisionDataAsync());

        // update danych do shadera terenu
        floatData = new Texture3D(32, 32, 32, TextureFormat.RFloat, 1);
        floatData.SetPixelData(texturemap, 0);
        floatData.Apply();
        material.SetTexture("_FloatData", floatData);
    }

    // kolizja osobno z opóŸnieniem w celu przyspieszenia dzialania
    IEnumerator UpdateCollisionDataAsync()
    {
        colliderCalculationTimer = colliderCalculationDelay;
        meshCollider.sharedMesh = currentMesh;
        while (colliderCalculationTimer > 0)
        {
            colliderCalculationTimer -= Time.deltaTime;
            yield return null;
        }
    }

    Vector3[] vertices = new Vector3[49152];
    int[] meshTriangles = new int[49152];
    Vector2[] uv = new Vector2[49152];

    void UpdateMeshFromTriangles(Triangle[] localTriangles)
    {
        for (int i = 0; i < localTriangles.Length; i++)
        {
            int offset = i * 3;
            vertices[offset] = localTriangles[i].a;
            vertices[offset + 1] = localTriangles[i].b;
            vertices[offset + 2] = localTriangles[i].c;

            meshTriangles[offset] = offset;
            meshTriangles[offset + 1] = offset + 1;
            meshTriangles[offset + 2] = offset + 2;

            uv[offset] = localTriangles[i].auv;
            uv[offset + 1] = localTriangles[i].buv;
            uv[offset + 2] = localTriangles[i].cuv;

        }

        //Debug.Log($"vertices:{vertices.Length}, meshTriangles:{meshTriangles.Length}, uv:{uv.Length}");
        currentMesh.Clear();
        currentMesh.vertices = vertices;
        currentMesh.triangles = meshTriangles;
        currentMesh.uv = uv;

        currentMesh.RecalculateNormals();
    }

    [SerializeField] bool customRecalculate = true;
    public void UpdateNormals()
    {
        if (customRecalculate)
            currentMesh.normals = CustomNormalRecalculate.Recalculate(currentMesh.vertices, currentMesh.normals);
    }

    /// <summary>
    /// Modifies chunk in the shape of a circle
    /// </summary>
    /// <param name="center">Center of the circle RELATIVE to the chunk</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="change">Value change</param>
    public void ModifyCircle(Vector3 center, float radius, float change)
    {
        updateChunkCS.SetBuffer(0, "_Values", valuesBuffer);
        updateChunkCS.SetBuffer(0, "_TextureMap", textureMapBuffer);
        updateChunkCS.SetBuffer(0, "_ColorCount", colorCountBuffer);
        updateChunkCS.SetFloats("_CirclePosition", new float[] { center.x, center.y, center.z });
        updateChunkCS.SetFloat("_CircleRadius", radius);
        updateChunkCS.SetFloat("_Change", change);
        updateChunkCS.SetInt("_Size", size);

        textureMapBuffer.SetData(texturemap);


        int dispatch = size / numberOfThreads;
        updateChunkCS.Dispatch(0, dispatch, dispatch, dispatch);
        circlecenterdebug = center;
        valuesBuffer.GetData(values);


        colorCountBuffer.GetData(lastColorCount);

        int sum = 0;
        Array.ForEach(lastColorCount, delegate (int i) { sum += i; });

        int l = UnityEngine.Random.Range(0, sum + 1);
        //Debug.Log($"Sum:{sum}, random:{l}");
        int found = 0;

        //string ahh = "";

        for (int i = 0; i < lastColorCount.Length; i++)
        {
            //ahh += $"|{lastColorCount[i]}";

            if (l <= lastColorCount[i])
            {
                found = i;
            }
            else
            {
                l -= lastColorCount[i];
            }
        }
        Gun.Instance.SetColor(found);
        //Debug.Log(ahh);
        colorCountBuffer.SetData(new int[32]);


        GenerateMesh();
    }


    Vector3 circlecenterdebug;

    private void Update()
    {
    }

    float gizmosTimer = 0;
    private void OnDrawGizmosSelected()
    {
        if (gizmosTimer < 0)
        {
            gizmosTimer = -1;
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(transform.position + circlecenterdebug, 5);
            if (showDebugValues)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int z = 0; z < size; z++)
                        {
                            float val = values[x + size * (y + size * z)];
                            Gizmos.color = new Color(val, val, val);
                            Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.2f);
                        }
                    }
                }
            }
            if (showDebugNormals)
            {
                for (int x = 4; x < 5; x++)
                {
                    for (int y = 4; y < 5; y++)
                    {
                        for (int z = 4; z < 5; z++)
                        {
                            if (true)
                            {
                                Vector3 point = currentMesh.vertices[x + size * (y + size * z)];
                                Vector3 target = currentMesh.normals[x + size * (y + size * z)];
                                Gizmos.color = new Color(target.x, target.y, target.z);
                                Gizmos.DrawLine(transform.position + point, transform.position + point + target);
                            }
                        }
                    }
                }

            }
        }
        else
        {
            gizmosTimer -= Time.deltaTime;
        }
    }
}
