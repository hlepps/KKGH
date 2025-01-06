using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveCarver : MonoBehaviour
{
    public Vector3 bounds = new Vector3(64, 64, 64);

    [SerializeField] int iterations = 1;
    [SerializeField] float segmentSpace = 1;
    [SerializeField] float segmentScale = 5f;
    [SerializeField] float segmentScaleVariation = 0.5f;
    [SerializeField] int minLength = 10;
    [SerializeField] int maxLength = 10;
    [SerializeField] float noiseFrequency = 1.0f;

    [SerializeField] GameObject prefab;

    FastNoiseLite noise = new FastNoiseLite();

    int maxSements = 0, doneSegments = 0;
    public int GetMaxSegments() { return maxSements; }
    public int GetDoneSegments() { return doneSegments; }


    void Start()
    {
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(noiseFrequency);

        //GenerateCaves();
    }

    public void GenerateCaves()
    {
        for(int i = 0; i < iterations; i++)
        {
            noise.SetSeed(Random.Range(int.MinValue, int.MaxValue));

            Vector3 position = new Vector3 (Random.Range(0,bounds.x), Random.Range(0,bounds.y), Random.Range(0,bounds.z));
            Vector3 direction = new Vector3 (Random.Range(0,1), Random.Range(0,1), Random.Range(0,1));
            int segments = Random.Range(minLength, maxLength);
            maxSements += segments;
            StartCoroutine(GenCave(segments, position, direction));
        }
    }

    IEnumerator GenCave(int segments, Vector3 position, Vector3 direction)
    {
        Vector3 newPosition = position + (direction * segmentSpace);

        if(newPosition.x < 0) newPosition.x = - newPosition.x;
        if(newPosition.y < 0) newPosition.y = - newPosition.y;
        if(newPosition.z < 0) newPosition.z = - newPosition.z;

        if(newPosition.x > bounds.x) newPosition.x = bounds.x - newPosition.x;
        if(newPosition.y > bounds.y) newPosition.y = bounds.y - newPosition.y;
        if(newPosition.z > bounds.z) newPosition.z = bounds.z - newPosition.z;

        PlaceCaveSegment(newPosition, segmentScale + (noise.GetNoise(100,segments) * segmentScaleVariation)); 
        segments--;
        doneSegments++;
        if (segments > 0)
        {
            Vector3 newDirection = new Vector3(noise.GetNoise(0, segments), noise.GetNoise(10, segments), noise.GetNoise(20, segments));
            newDirection /= 2;
            newDirection += direction;
            yield return null;
            StartCoroutine(GenCave(segments, newPosition, newDirection.normalized));
        }
        
        
    }

    void PlaceCaveSegment(Vector3 position, float scale)
    {
        Map.GetInstance().ModifyCircle(position, scale, -0.75f);
        /*
        var temp = Instantiate(prefab);
        temp.transform.position = position;
        temp.transform.localScale = Vector3.one * scale;
        temp.transform.SetParent(this.transform);
        */
    }
}
