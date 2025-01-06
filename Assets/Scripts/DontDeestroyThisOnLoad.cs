using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDeestroyThisOnLoad : MonoBehaviour
{
    static List<GameObject> Instances = new();
    void Start()
    {
        if(Instances.Contains(this.gameObject))
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instances.Add(this.gameObject);
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
