using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    [SerializeField] KeyCode key;
    [SerializeField] GameObject objectToThrow;
    [SerializeField] float force = 10;
    [SerializeField] int maxInstances = 8;

    List<GameObject> thrown = new List<GameObject>();

    private void Update()
    {
        if(Input.GetKeyDown(key))
        {
            GameObject obj = Instantiate(objectToThrow);
            obj.transform.position = transform.position;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if(rb != null )
            {
                rb.AddForce((transform.forward + transform.up/2f).normalized * force);
                rb.AddTorque(obj.transform.right * force/100f);
            }
            if (thrown.Count >= maxInstances)
            {
                Destroy(thrown[0]);
                thrown.RemoveAt(0);
                thrown.Add(obj);
            }
            else
            {
                thrown.Add(obj);
            }
        }
    }
}
