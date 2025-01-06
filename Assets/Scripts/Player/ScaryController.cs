using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Volume))]
public class ScaryController : MonoBehaviour
{
    Volume volume;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        volume = GetComponent<Volume>();
    }

    private void Start()
    {
        StartCoroutine(CheckGhosts());
    }

    IEnumerator CheckGhosts()
    {
        while (true)
        {
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            GameObject closest;
            if (ghosts.Count() > 0)
            {
                closest = ghosts[0];
                foreach (GameObject ghost in ghosts)
                {
                    if (ghost != null && closest != null)
                    {
                        if (Vector3.Distance(ghost.transform.position, transform.position) < Vector3.Distance(closest.transform.position, transform.position))
                        {
                            closest = ghost;
                        }
                    }
                    yield return null;
                }
                if (closest != null)
                {
                    float dist = Vector3.Distance(closest.transform.position, transform.position);
                    if (dist > 20)
                    {
                        volume.weight = Mathf.Lerp(volume.weight, 0, Time.deltaTime);
                        audioSource.pitch = Mathf.Lerp(audioSource.pitch, 0, Time.deltaTime);
                        audioSource.volume = Mathf.Lerp(audioSource.volume, 0, Time.deltaTime);
                    }
                    else
                    {
                        Mathf.Clamp(dist, 0.0f, 20.0f);
                        dist /= 20f;
                        dist = 1 - dist;

                        volume.weight = dist;
                        audioSource.pitch = -dist * 3;
                        audioSource.volume = dist;
                    }
                }
            }
            else
            {
                volume.weight = Mathf.Lerp(volume.weight, 0, Time.deltaTime);
                audioSource.pitch = Mathf.Lerp(audioSource.pitch, 0, Time.deltaTime);
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, Time.deltaTime);
            }
            yield return null;
        }
    }

    float timer = 1;
    bool savedScore = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Ghost>() != null)
        {
            timer -= Time.deltaTime;
            Debug.Log($"DEAD IN {timer}");
            if (timer < 0)
            {
                if(!savedScore)
                {
                    PlayerPrefs.SetFloat("AllTimeScore", PlayerPrefs.GetFloat("AllTimeScore") + CollectiblePicker.GetInstance().GetScore());
                    if (CollectiblePicker.GetInstance().GetScore() > PlayerPrefs.GetFloat("Highscore"))
                        PlayerPrefs.SetFloat("Highscore", CollectiblePicker.GetInstance().GetScore());

                    savedScore = true;
                }

                SceneChanger.instance.Transition(0);
            }
        }
    }

}
