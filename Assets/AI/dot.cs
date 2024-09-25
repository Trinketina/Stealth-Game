using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dot : MonoBehaviour
{
    [SerializeField] Transform player;
    void Update()
    {
        Debug.Log(Vector3.Dot((player.position - transform.position).normalized, transform.forward));
    }
}
