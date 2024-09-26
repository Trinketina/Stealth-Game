using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeatGame: MonoBehaviour
{
    [SerializeField] AudioSource source;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            source.Play();
            Time.timeScale = 0f;
        }
    }
}
