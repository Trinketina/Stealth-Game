using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerSystem : MonoBehaviour
{
    public UnityEvent<Player> Enter;
    public UnityEvent<Player> Exit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
            Enter.Invoke(other.GetComponent<Player>());
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
            Exit.Invoke(other.GetComponent<Player>());
    }
}
