using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static Gun Instance;

    [SerializeField] Transform gunPlace;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] Transform lookAt;
    [SerializeField] float positionLerpSpeed;
    [SerializeField] float rotationLerpSpeed;
    [SerializeField] List<Color> colors = new List<Color>();

    Color currentColor = Color.white;

    ParticleSystem.MainModule particleSystemMainModule;
    ParticleSystem.EmissionModule emissionModule;

    Transform start, end;



    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        particleSystemMainModule = particleSystem.main;
        emissionModule = particleSystem.emission;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, gunPlace.position, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, gunPlace.rotation, Time.deltaTime * rotationLerpSpeed);

        if (end != null)
        {
            particleSystem.transform.LookAt(end);
            particleSystemMainModule.startSpeed = Vector3.Distance(particleSystem.transform.position, end.position) * 2;
            particleSystem.transform.position = start.position;

            particleSystemMainModule.startColor = currentColor;
        }
    }

    public void StartParticleSystem()
    {
        particleSystem.Play();
        start = gunPlace;
        end = lookAt;
    }
    public void StartParticleSystemReversed()
    {
        particleSystem.Play();
        start = lookAt;
        end = gunPlace;
    }

    public void StopParticleSystem()
    {
        particleSystem.Stop();
        //particleSystem.Clear();
    }

    public void SetRate(float rate)
    {
        emissionModule.rateOverTime = rate;
    }

    public void SetColor(int colorID)
    {
        if(colorID < colors.Count && colorID >= 0)
            currentColor = colors[colorID];
    }
}
