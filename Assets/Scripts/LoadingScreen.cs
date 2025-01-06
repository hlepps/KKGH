using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] CaveCarver carver;
    [SerializeField] PlayerSpawner spawner;
    [SerializeField] SceneChanger changer;
    float progress = 0f;
    
    float delay = 0.25f;

    List<bool> readyChunks = new List<bool>();

    bool afterDelay = false;

    bool loading = true;

    void Update()
    {
        if(loading)
        {
            if (delay > 0f)
            {
                delay -= Time.deltaTime;
            }
            else
            {
                progress = ((float)Map.GetInstance().GetChunks().CountNotNull() / (float)Map.GetInstance().GetMapChunkLength()) / 2.0f;
                if (carver.GetMaxSegments() > 0)
                {
                    progress += ((float)carver.GetDoneSegments() / (float)carver.GetMaxSegments()) / 2.0f;
                }
                else
                {
                    progress *= 2;
                }

                delay = 0.25f;
            }
            slider.value = progress;

            if (progress >= 1)
            {
                spawner.Spawn();
                spawner.Spawn();
                changer.OutAndIn();
                StartCoroutine(TurnOff());
                loading = false;
            }
        }
    }

    IEnumerator TurnOff()
    {
        float timer = 1;
        while(timer >= 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
        spawner.Spawn();
    }
}
