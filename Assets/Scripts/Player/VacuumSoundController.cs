using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumSoundController : MonoBehaviour
{
    [SerializeField] Gun gun;
    [SerializeField] TunelRay ray;
    [SerializeField] AudioSource vacuumClick, vacuumMain;
    [SerializeField] AudioClip vacuumOn, vacuumOff;

    bool isOn = false, isThrowing = false;

    private void Start()
    {
        gun.StartShootingAction += StartVacuum;
        gun.StopShootingAction += StopVacuum;
    }

    private void Update()
    {
        vacuumMain.pitch = 0.0889f * ray.GetCurrentRadius() + 0.7556f;
        vacuumClick.pitch = 0.0889f * ray.GetCurrentRadius() + 0.7556f;
    }

    void StartVacuum(bool isThrowing)
    {
        if (!isOn)
        {
            vacuumClick.Stop();
            vacuumClick.PlayOneShot(vacuumOn);

            StopAllCoroutines();

            StartCoroutine(WaitForVacuum(isThrowing));

            isOn = true;
        }
    }

    IEnumerator WaitForVacuum(bool isThrowing)
    {
        float time = 0;
        while(time < 2.534f * (1f/vacuumMain.pitch))
        {
            time += Time.deltaTime;
            yield return null;
        }
        while(vacuumMain.volume < 0.99f)
        {
            vacuumMain.volume += Time.deltaTime * 100;
            yield return null;
        }
    }

    void StopVacuum()
    {
        if (isOn)
        {
            vacuumClick.Stop();
            vacuumClick.PlayOneShot(vacuumOff);

            StopAllCoroutines();

            StartCoroutine(WaitForVacuumOff());

            isOn = false;
        }
    }

    IEnumerator WaitForVacuumOff()
    {
        while (vacuumMain.volume > 0.01f)
        {
            vacuumMain.volume -= Time.deltaTime * 100;
            yield return null;
        }
    }
}
