using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurpriseHandler : MonoBehaviour
{
    [SerializeField] TextMesh text;
    [SerializeField] AudioSource alert;

    [SerializeField] Color red;
    [SerializeField] Color transparent;

    Transform cam;

    float time = 5;
    bool fading = false;

    private void Start()
    {
        cam = Camera.main.transform;
    }
    public void Sight(Guard state)
    {
        if (state == Guard.Pursue)
        {
            text.text = "!";
            alert.Play();
        }
        else if (state == Guard.Investigate)
            text.text = "?";
        else 
            return;

        text.color = red;
        time = 0;
        fading = true;
    }

    private void Update()
    {
        text.transform.forward = cam.forward;

        if (fading)
        {
            time += Time.deltaTime;
            text.color = Color.Lerp(red, transparent, time / 4);

            if (time > 4)
            {
                fading = false;
            }
        }

    }
}
