using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.GlobalIllumination;

public class Gun : MonoBehaviour
{
    public static Gun Instance;

    [SerializeField] Transform gunPlace;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] Transform lookAt;
    [SerializeField] float positionLerpSpeed;
    [SerializeField] float rotationLerpSpeed;
    [SerializeField] List<Color> colors = new List<Color>();
    [SerializeField] TunelRay ray;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Light GunLight;
    [SerializeField] Slider vacuumSlider;

    Color currentColor = Color.white;

    ParticleSystem.MainModule particleSystemMainModule;
    ParticleSystem.EmissionModule emissionModule;

    Transform start, end;

    public Action<bool> StartShootingAction = (a) => { };
    public Action StopShootingAction = () => { };

    bool isShooting = false;

    [SerializeField] float vacuumConsumptionRate = 1;
    [SerializeField] float maxVacuumConsumption = 30;
    [SerializeField] float vacuumConsumptionDecay = 3;
    [SerializeField] float vacuumConsumptionDecayWhenLocked = 6;
    float currentVacuumConsumption = 0;
    bool stopVacuum = false;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        particleSystemMainModule = particleSystem.main;
        emissionModule = particleSystem.emission;
        vacuumSlider.maxValue = maxVacuumConsumption;
    }

    float shakeCooldown = 0;
    void Update()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, gunPlace.position, Time.deltaTime * positionLerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, gunPlace.rotation, Time.deltaTime * rotationLerpSpeed);

        if (end != null)
        {
            particleSystem.transform.LookAt(end);
            particleSystemMainModule.startSpeed = Vector3.Distance(particleSystem.transform.position, end.position) * 2;
            particleSystem.transform.position = start.position;

            particleSystemMainModule.startColor = currentColor;
        }

        if (ray.GetVacuumLock())
        {
            if(currentVacuumConsumption <= 0)
            {
                currentVacuumConsumption = 0;
                ray.SetVacuumLock(false);
            }
        }

        if(isShooting)
        {
            float strength = shakeCooldown * (ray.GetCurrentRadius() / 5f) * 0.1f;
            shakeCooldown += Time.deltaTime/5f;
            if (shakeCooldown > 1f) shakeCooldown = 1f;
            targetPosition = targetPosition + UnityEngine.Random.insideUnitSphere * strength;
            currentVacuumConsumption += Time.deltaTime * (shakeCooldown / 5f) * ray.GetCurrentRadius() * vacuumConsumptionRate;
            //Debug.Log(currentVacuumConsumption);
            if(currentVacuumConsumption > maxVacuumConsumption)
            {
                ray.SetVacuumLock(true);
            }
        }
        else
        {
            if (shakeCooldown > 0)
            {
                float strength = shakeCooldown * (ray.GetCurrentRadius() / 5f) * 0.1f;
                shakeCooldown -= Time.deltaTime/5f;
                targetPosition = targetPosition + UnityEngine.Random.insideUnitSphere * strength;
            }

            if (currentVacuumConsumption > 0)
            {
                if(ray.GetVacuumLock())
                    currentVacuumConsumption -= Time.deltaTime * vacuumConsumptionDecayWhenLocked;
                else
                    currentVacuumConsumption -= Time.deltaTime * vacuumConsumptionDecay;
            }
            else currentVacuumConsumption = 0;
        }

        transform.position = targetPosition;

        Color c = meshRenderer.material.color;
        c.g = 1 - (currentVacuumConsumption / maxVacuumConsumption);
        c.b = c.g;
        meshRenderer.material.color = c;

        var lc = GunLight.color;
        lc.r = (currentVacuumConsumption / maxVacuumConsumption);
        lc.g = 1 - lc.r;
        GunLight.color = lc;

        vacuumSlider.value = currentVacuumConsumption;
    }

    public void StartParticleSystem()
    {
        StartShootingAction.Invoke(true);
        isShooting = true;

        particleSystem.Play();
        start = gunPlace;
        end = lookAt;
    }
    public void StartParticleSystemReversed()
    {
        StartShootingAction.Invoke(false);
        isShooting = true;

        particleSystem.Play();
        start = lookAt;
        end = gunPlace;
    }

    public void StopParticleSystem()
    {
        StopShootingAction.Invoke();
        isShooting = false;

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
