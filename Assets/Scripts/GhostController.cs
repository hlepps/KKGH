using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostController : MonoBehaviour
{
    [SerializeField] GameObject ghostPrefab;
    [SerializeField] GameObject playerObject;
    [SerializeField] float minRadius = 10;
    [SerializeField] float maxRadius = 50;

    List<Ghost> ghosts = new();

    [SerializeField] bool spawnGhostManually;

    public static GhostController instance;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(spawnGhostManually)
        {
            spawnGhostManually = false;
            SpawnGhost();
        }
    }

    public void SpawnGhost()
    {
        StartCoroutine(SpawnGhostAsync());
    }

    IEnumerator SpawnGhostAsync()
    {
        Vector3 point = Vector3.zero;
        float value = 0;
        while (value < 0.5f)
        {
            point = playerObject.transform.position + (Random.insideUnitSphere.normalized * Random.Range(minRadius, maxRadius));
            value = Map.GetInstance().GetPointValue(point);
            yield return null;
        }
        GameObject currentGhostObject = Instantiate(ghostPrefab);
        currentGhostObject.transform.SetParent(this.transform);
        currentGhostObject.transform.position = point;
        ghosts.Add(currentGhostObject.GetComponent<Ghost>());
    }

    public void StartGhostSpawning()
    {
        if(!ghostSpawning)
        {
            StartCoroutine(GhostSpawner());
            ghostSpawning = true;
        }
    }

    bool ghostSpawning = false;
    float timer = 120;
    float ghostAmount = 1;
    float nextGhostTimeSkip = 1;
    float nextGhostTime = 120;
    IEnumerator GhostSpawner()
    {
        while(true)
        {
            if(timer <= 0)
            {
                for(int i = 0; i < ghostAmount; i++)
                {
                    SpawnGhost();
                    Debug.Log("[!] Spawning Ghost");
                    yield return null;
                }
                timer = nextGhostTime * (1f / nextGhostTimeSkip);
            }
            else
            {
                timer -= Time.deltaTime;
            }
            yield return null;
        }
    }

    public void KillGhost(Ghost ghost)
    {
        ghosts.Remove(ghost);
        Destroy(ghost.gameObject);
        ghostAmount += Random.Range(0.1f, 0.33f);
        ghostAmount += Random.Range(0.01f, 0.1f);
    }


}
