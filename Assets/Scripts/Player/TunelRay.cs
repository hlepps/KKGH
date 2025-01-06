using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunelRay : MonoBehaviour
{

    [SerializeField] GameObject indicator;
    [SerializeField] float radius = 2f;
    [SerializeField] float change = -0.1f / 30f;
    [SerializeField] float delay = 1f / 15f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] Gun gun;

    float delayCounter;
    bool lockVacuum = false;
    public void SetVacuumLock(bool value) => lockVacuum = value;
    public bool GetVacuumLock() => lockVacuum;

    public float GetCurrentRadius() => radius;

    float cooldown = 0f;

    private void Update()
    {
        bool edit = false;
        float realchange = 0;
        bool updateNormals = false;

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
        {
            edit = true;
            realchange = 0;
            radius += wheel * Time.deltaTime * scrollSpeed * (1f / Mathf.Sqrt(radius / 2f));
            radius = Mathf.Clamp(radius, 0.5f, 5f);
            gun.SetRate(radius * 25);
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            //Debug.Log("Mouse button up");
            edit = true;
            realchange = 0;
            updateNormals = true;
            delayCounter = 0;
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                edit = true;
                realchange = change;
            }
            if (Input.GetMouseButton(1))
            {
                edit = true;
                realchange = -change;
            }
        }
        if (edit && !lockVacuum)
        {
            if (delayCounter <= 0)
            {
                delayCounter = delay;
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Chunk", "Ghost")))
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
                        if (hitinfo.collider.gameObject.tag == "Ghost" && realchange < 0)
                        {
                            hitinfo.collider.gameObject.GetComponent<Ghost>().Damage(Time.deltaTime * radius);
                        }
                        else
                        {
                            Map.GetInstance().ModifyCircle(hitinfo.point, radius, realchange * cooldown);
                            if (updateNormals)
                            {
                                //Debug.Log("Updating normals");
                                Map.GetInstance().UpdateNormals(hitinfo.point, radius);
                                updateNormals = false;
                            }
                        }
                        //Debug.Log(cooldown);

                        cooldown += Time.deltaTime * 2f;
                        if (cooldown > 1) cooldown = 1;

                        
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

            cooldown -= Time.deltaTime / 5f;
            if (cooldown < 0) cooldown = 0;
        }
    }
}
