using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunelRay : MonoBehaviour
{

    [SerializeField] GameObject indicator;
    [SerializeField] float radius = 2f;
    [SerializeField] float change = -0.1f/30f;
    [SerializeField] float delay = 1f/15f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] Gun gun;

    float delayCounter;

    private void Update()
    {
        bool edit = false;
        float realchange = 0;

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
        {
            edit = true;
            realchange = 0;
            radius += wheel * Time.deltaTime * scrollSpeed * (1f/Mathf.Sqrt(radius/2f));
            radius = Mathf.Clamp(radius, 0.5f, 10f);
            gun.SetRate(radius * 25);
        }

        if(Input.GetMouseButton(0))
        {
            edit = true;
            realchange = change;
        }
        if(Input.GetMouseButton(1))
        {
            edit = true;
            realchange = -change;
        }

        if(edit)
        {
            if (delayCounter <= 0)
            {
                delayCounter = delay;
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Chunk")))
                {
                    if (Vector3.Distance(transform.position, hitinfo.point) > 1)
                    {
                        indicator.SetActive(true);

                        if (realchange > 0)
                            gun.StartParticleSystem();
                        else
                            gun.StartParticleSystemReversed();

                        indicator.transform.position = hitinfo.point;
                        indicator.transform.localScale = Vector3.one * radius;
                        indicator.transform.LookAt(this.transform.position);
                        Map.GetInstance().ModifyCircle(hitinfo.point, radius, realchange);
                    }
                    else
                    {
                        gun.StopParticleSystem();
                        indicator.SetActive(false);
                    }
                }
                else
                {
                    indicator.SetActive(false);
                    gun.StopParticleSystem();
                }
            }
            else
                delayCounter -= Time.deltaTime;
        }
        else
        {
            indicator.SetActive(false);
            gun.StopParticleSystem();
        }    
    }
}
