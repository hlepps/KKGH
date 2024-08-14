using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Map))]
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    Map map;
    Vector3 spawnPos;
    private void Start()
    {
        map = GetComponent<Map>();
    }
    public void Spawn()
    {
        spawnPos = new Vector3(
            Random.Range(8, map.GetMapSize().x-8),
            Random.Range(8, map.GetMapSize().y-8),
            Random.Range(8, map.GetMapSize().z-8)
            );

        Debug.Log($"Spawn:{spawnPos}");
        map.ModifyCircle(spawnPos, 5, -0.5f);
        playerObject.transform.position = spawnPos;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Spawn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnPos, 5f);
    }
}
