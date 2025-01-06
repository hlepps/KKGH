using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleArrow : MonoBehaviour
{
    [SerializeField] GameObject currentCollectible;
    public void SetCollectibleToFollow(GameObject collectible) {  currentCollectible = collectible; }

    void Update()
    {
        if(currentCollectible != null)
        {
            transform.LookAt(currentCollectible.transform);
        }
    }
}
