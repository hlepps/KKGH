using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] ComputeShader chunkCS;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer trianglesCountBuffer;
    ComputeBuffer valuesBuffer;

    [SerializeField] int size = 16;
    public int GetSize() { return size; }
    [SerializeField] int numberOfThreads = 8;

    MeshFilter meshFilter;

    Mesh currentMesh;

    struct Triangle
    {
        public float3 a;
        public float3 b;
        public float3 c;
    }


    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        trianglesBuffer = new ComputeBuffer(5 * (size * size * size), sizeof(float) * 3 * 3, ComputeBufferType.Append);
        trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        valuesBuffer = new ComputeBuffer(size * size * size, sizeof(float));
    }

    private void OnDisable()
    {
        trianglesBuffer.Release();
        trianglesBuffer = null;
        trianglesCountBuffer.Release();
        trianglesCountBuffer = null;
        valuesBuffer.Release();
        valuesBuffer = null;
    }

    public void GenerateMesh(float[] values)
    {

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
        Triangle[] triangles = new Triangle[triangleCount[0]];
        trianglesBuffer.GetData(triangles);

        // w³aœciwe generowanie mesha
        currentMesh = GetMeshFromTriangles(triangles);
        currentMesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = currentMesh;
    }

    Mesh GetMeshFromTriangles(Triangle[] triangles)
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            int offset = i * 3;
            vertices[offset] = triangles[i].a;
            vertices[offset + 1] = triangles[i].b;
            vertices[offset + 2] = triangles[i].c;

            meshTriangles[offset] = offset;
            meshTriangles[offset+1] = offset+1;
            meshTriangles[offset+2] = offset+2;

            uv[i] = new Vector2(0, 0);

        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        return mesh;
    }
}
