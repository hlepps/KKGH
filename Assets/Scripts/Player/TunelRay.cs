using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunelRay : MonoBehaviour
{

    [SerializeField] GameObject indicator;
    [SerializeField] float radius = 2f;
    [SerializeField] float change = -0.1f/30f;
    [SerializeField] float delay = 1f/15f;
    float delayCounter;
    private void Update()
    {
        bool edit = false;
        float realchange = 0;
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
                    indicator.SetActive(true);
                    indicator.transform.position = hitinfo.point;
                    Map.GetInstance().ModifyCircle(hitinfo.point, radius, realchange);
                }
                else
                {
                    indicator.SetActive(false);
                }
            }
            else
                delayCounter -= Time.deltaTime;
        }
    }
}
