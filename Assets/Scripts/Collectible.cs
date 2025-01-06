using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] float minValue = 1;
    [SerializeField] float maxValue = 5;
    float value;
    public float GetValue() { return value; }

    void Start()
    {
        value = Random.Range(minValue, maxValue);
        transform.localScale = Vector3.one * value/2f;
    }
}
