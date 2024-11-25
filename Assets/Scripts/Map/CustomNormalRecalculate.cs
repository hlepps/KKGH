using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomNormalRecalculate : MonoBehaviour
{
    public static Vector3[] Recalculate(Vector3[] verts, Vector3[] normals)
    {
        Dictionary<Vector3, List<Vector3>> dict = new();
        Dictionary<Vector3, Vector3> meds = new();
        List<Vector3> newNormals = new();
        for (int i = 0; i < verts.Length; i++)
        {
            if (dict.ContainsKey(verts[i]))
            {
                dict[verts[i]].Add(normals[i]);
            }
            else
            {
                dict.Add(verts[i], new List<Vector3>() { normals[i] });
            }
        }
        foreach(var pair in dict)
        {
            Vector3 med = Vector3.zero;
            foreach (var v in pair.Value)
            {
                med += v;
            }
            med.Normalize();
            meds.Add(pair.Key, med);
        }
        for (int i = 0; i < normals.Length; i++)
        {
            if (meds.ContainsKey(verts[i]))
            {
                if (Vector3.Distance(meds[verts[i]], normals[i]) < 1)
                {
                    newNormals.Add(meds[verts[i]]);
                }
                else
                {
                    newNormals.Add(normals[i]);
                }
            }
        }
        Debug.Log("AAA");
        return newNormals.ToArray();
    }
}
