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

    bool inRange = false;
    Transform player;

    void FixedUpdate()
    {
        if (inRange)
        {
            Vector3 diff = (player.position - transform.position).normalized;
            //Debug.Log(Vector3.Dot(diff, transform.forward));

            if (Vector3.Dot(diff, transform.forward) > .85f && Unobstructed(player.position))
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

    private bool Unobstructed(Vector3 position)
    {
        Vector3 dir = (position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, position) + 5f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, distance))
        {
            if (hit.transform.GetComponent<Player>() != null)
                return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            player = other.transform;
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            LostSight.Invoke(player.position);
            spotted = false;
            inRange = false;
        }
    }
}
