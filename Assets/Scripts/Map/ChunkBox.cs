using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBox : MonoBehaviour
{
    Chunk chunk;
    public Chunk GetChunk() { return chunk; }

    public void InitChunkBox(Chunk parent, int size)
    {
        chunk = parent;
        transform.localScale = Vector3.one * size;
        transform.localPosition = (Vector3.one * size) / 2f;
    }
}
