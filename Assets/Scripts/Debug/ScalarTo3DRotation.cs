using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalarTo3DRotation : MonoBehaviour
{
    [SerializeField] float scalar;

    void Update()
    {
        scalar += Time.deltaTime;
        if (scalar > 1f)
            scalar -= 1f;

        transform.rotation = new Quaternion(scalar,scalar, scalar, scalar);

    }
}
