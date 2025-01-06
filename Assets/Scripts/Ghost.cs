using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ghost : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] List<AudioClip> ghostSounds = new List<AudioClip>();
    [SerializeField] float minSoundTime = 5.0f;
    [SerializeField] float maxSoundTime = 15.0f;
    [SerializeField] float minSpeed = 0.7f;
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] float minHealth = 10f;
    [SerializeField] float maxHealth = 30f;

    AudioSource audioSource;
    GameObject playerObject;

    float speed;
    float health;
    float currentMaxHealth;

    public void Damage(float value)
    {
        health -= value;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(ghostSounds[Random.Range(0, ghostSounds.Count)]);
        timer = Random.Range(minSoundTime, maxSoundTime);
        speed = Random.Range(minSpeed, maxSpeed);
        health = Random.Range(minHealth, maxHealth);
        currentMaxHealth = health;

        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    float timer = 0.0f;

    void Update()
    {
        if (timer <= 0)
        {
            audioSource.PlayOneShot(ghostSounds[Random.Range(0, ghostSounds.Count)]);
            timer = Random.Range(minSoundTime, maxSoundTime);
        }
        else
        {
            timer -= Time.deltaTime;
        }

        transform.LookAt(playerObject.transform);
        transform.position += ((playerObject.transform.position - transform.position).normalized * Time.deltaTime * speed);

        Color c = meshRenderer.material.color;
        c.r = health / currentMaxHealth;
        c.g = health / currentMaxHealth;
        c.b = health / currentMaxHealth;
        meshRenderer.material.color = c;

        if (health <= 0)
        {
            GhostController.instance.KillGhost(this);
        }
    }
}
