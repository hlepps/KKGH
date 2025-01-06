using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    public static CollectibleSpawner Instance;
    [SerializeField] GameObject collectiblePrefab;
    [SerializeField] CollectibleArrow arrow;
    [SerializeField] GameObject playerObject;

    [SerializeField] float minDistance = 50f;
    [SerializeField] float maxdistance = 300f;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnCollectible()
    {
        Vector3 position = playerObject.transform.position + Random.insideUnitSphere.normalized * Random.RandomRange(minDistance,maxdistance);
        if(position.y > Map.GetInstance().GetMapSize().y)
        {
            position.y = Random.Range(-Map.GetInstance().GetMapSize().y, Map.GetInstance().GetMapSize().y);
        }
        GameObject temp = Instantiate(collectiblePrefab);
        temp.transform.SetParent(this.transform);
        temp.transform.position = position;
        arrow.SetCollectibleToFollow(temp);
    }
}
