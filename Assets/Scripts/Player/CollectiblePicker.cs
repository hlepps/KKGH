using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectiblePicker : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moneySound;
    static CollectiblePicker instance;
    public static CollectiblePicker GetInstance() { return instance; }
    private void Awake()
    {
        instance = this;
    }
    float score;
    public float GetScore() { return score; }

    [SerializeField] TextMeshProUGUI scoreText;
    private void Update()
    {
        scoreText.text = score.ToString("0.## M€");
    }
    private void OnTriggerEnter(Collider other)
    {
        var collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            score += collectible.GetValue();
            CollectibleSpawner.Instance.SpawnCollectible();
            Destroy(collectible.gameObject);
            audioSource.PlayOneShot(moneySound);
        }
    }
}
