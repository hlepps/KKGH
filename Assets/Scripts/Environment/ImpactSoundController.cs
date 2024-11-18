using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpactSoundController : MonoBehaviour
{
    [SerializeField] List<AudioClip> audioClips = new();
    [SerializeField] float pitchRandomRange = 0.1f;
    [SerializeField] float minimalMovement = 0.5f;
    AudioSource AudioSource;
    Vector3 lastPos;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.playOnAwake = false;
        AudioSource.loop = false;
        AudioSource.spatialBlend = 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(Vector3.Distance(lastPos, transform.position) > minimalMovement)
        {
            AudioSource.pitch = Random.Range(1f - pitchRandomRange, 1f + pitchRandomRange);
            AudioSource.PlayOneShot(audioClips[Random.RandomRange(0, audioClips.Count)]);
        }
        lastPos = transform.position;
    }
}