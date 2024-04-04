using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    [SerializeField] KeyCode key;
    [SerializeField] GameObject objectToThrow;
    [SerializeField] float force = 10;

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
        }
    }
}
