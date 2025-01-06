using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] bool startBlack = true;

    [SerializeField] Image image;

    bool isBlack;
    public static SceneChanger instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (startBlack)
        {
            image.color = Color.black;
            isBlack = true;
        }
    }

    private void Update()
    {
        if (isBlack)
        {
            var color = image.color;
            color.a -= Time.deltaTime;
            if (color.a <= 0)
            {
                isBlack = false;
            }
            image.color = color;

        }
    }

    public void Transition(int sceneIndex)
    {
        StartCoroutine(TransitionAsync(sceneIndex));
    }

    IEnumerator TransitionAsync(int sceneIndex)
    {
        float delay = 1;
        while(delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
        while (image.color.a <= 1)
        {
            var color = image.color;
            color.a += Time.deltaTime;
            image.color = color;
            yield return null;
            Debug.Log(image.color.a);
        }
        if(sceneIndex < 0)
        {
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public void OutAndIn()
    {
        StartCoroutine(OutAndInAsync());
    }

    IEnumerator OutAndInAsync()
    {
        float timer = 0;
        while(timer <= 1)
        {
            timer += Time.deltaTime;
            var color = image.color;
            color.a = timer;
            image.color = color;
            yield return null;
        }
        while(timer >= 0)
        {
            timer -= Time.deltaTime;
            var color = image.color;
            color.a = timer;
            image.color = color;
            yield return null;
        }
    }
}
