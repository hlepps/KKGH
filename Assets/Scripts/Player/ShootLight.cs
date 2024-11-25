using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLight : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int maxLights = 4;
    List<GameObject> objects = new();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Chunk")))
            {
                if (Vector3.Distance(transform.position, hitinfo.point) > 1)
                {
                    GameObject obj = GameObject.Instantiate(prefab);
                    Vector3 offset = (transform.position - hitinfo.point).normalized;
                    obj.transform.position = hitinfo.point + offset;
                    if (objects.Count >= maxLights)
                    {
                        Destroy(objects[0]);
                        objects.RemoveAt(0);
                    }
                    objects.Add(obj);
                }
            }
        }
    }
}
