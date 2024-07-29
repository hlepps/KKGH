using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    uint3 id;
    public uint3 GetID() {  return id; }
    [SerializeField] ComputeShader chunkCS;
    [SerializeField] ComputeShader updateChunkCS;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer trianglesCountBuffer;
    ComputeBuffer valuesBuffer;

    [SerializeField] int size = 16;
    public int GetSize() { return size; }
    [SerializeField] int numberOfThreads = 8;

    bool ready = false;
    public bool IsReady() { return ready; }

    MeshFilter meshFilter;
    Mesh currentMesh;
    MeshCollider meshCollider;

    [SerializeField] float[] values;

    [SerializeField] ChunkBox chunkBox;
    public ChunkBox GetChunkBox() { return chunkBox; }

    [SerializeField] bool showDebugValues;

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

    public void InitChunk(float[] newvalues, uint3 chunkID)
    {
        id = chunkID;
        values = new float[size * size * size];
        Array.Copy(newvalues, values, newvalues.Count());
        GenerateMesh();
    }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        currentMesh = new Mesh();
    }
    private void InitBuffers()
    {
        trianglesBuffer = new ComputeBuffer(5 * (size * size * size), Triangle.GetSize(), ComputeBufferType.Append);
        trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        valuesBuffer = new ComputeBuffer(size * size * size, sizeof(float));
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
        ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer,0);
        trianglesCountBuffer.GetData(triangleCount);

        // pobieranie trójk¹tów
        
        trianglesBuffer.GetData(triangles);


        // w³aœciwe generowanie mesha
        StartCoroutine(UpdateMeshFromTrianglesAsync());
    }

    IEnumerator UpdateMeshFromTrianglesAsync()
    {
        yield return null;
        UpdateMeshFromTriangles(triangles);
        meshFilter.sharedMesh = currentMesh;
        meshCollider.sharedMesh = currentMesh;
        ready = true;
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
            meshTriangles[offset+1] = offset+1;
            meshTriangles[offset+2] = offset+2;

            uv[offset] = localTriangles[i].auv;
            uv[offset+1] = localTriangles[i].buv;
            uv[offset+2] = localTriangles[i].cuv;

        }

        //Debug.Log($"vertices:{vertices.Length}, meshTriangles:{meshTriangles.Length}, uv:{uv.Length}");
        currentMesh.Clear();
        currentMesh.vertices = vertices;
        currentMesh.triangles = meshTriangles;
        currentMesh.uv = uv;
        currentMesh.RecalculateNormals();
        currentMesh.RecalculateNormals();
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
        updateChunkCS.SetFloats("_CirclePosition", new float[] { center.x,center.y,center.z });
        updateChunkCS.SetFloat("_CircleRadius", radius);
        updateChunkCS.SetFloat("_Change", change);
        updateChunkCS.SetInt("_Size", size);

        int dispatch = size / numberOfThreads;
        updateChunkCS.Dispatch(0, dispatch, dispatch, dispatch);
        valuesBuffer.GetData(values);

        GenerateMesh();
        circlecenterdebug = center;
    }

    Vector3 circlecenterdebug;

    private void Update()
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + circlecenterdebug, 5);
        if(showDebugValues)
        {
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    for(int z = 0; z < size; z++)
                    {
                        float val = values[x + size * (y + size * z)];
                        Gizmos.color = new Color(val,val,val);
                        Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.2f);
                    }
                }
            }
        }
    }
}
