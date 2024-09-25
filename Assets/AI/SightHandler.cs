using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

public class SightHandler : MonoBehaviour
{
    bool spotted = false;
    public UnityEvent<Transform> GainedSight;
    public UnityEvent<Vector3> LostSight;

    [SerializeField] Transform player;
    [SerializeField] float range;

    void Update()
    {
        Vector3 diff = (player.position - transform.position).normalized;
        Debug.Log(Vector3.Dot(diff, transform.forward));

        if (Vector3.Dot(diff, transform.forward) > .85f 
            && Vector3.Distance(transform.position, player.position) < range)
        {
            if (!spotted)
                GainedSight.Invoke(player);
            spotted = true;
        }
        else if (spotted)
        {
            spotted = false;
            LostSight.Invoke(player.position);
        }
    }
}
