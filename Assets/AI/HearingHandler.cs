using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HearingHandler : MonoBehaviour
{
    public UnityEvent<Vector3> HeardSomething;

    bool sneaking = false;
    bool inRange = false;
    bool withinSight = false;

    Transform player;
    Vector3 lastPosition = new();

    public void Sneak(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            sneaking = false;
        }
        else
        {
            sneaking = true;
        }
    }

    public void InSights()
    {
        withinSight = true;
    }
    public void LostSight()
    {
        withinSight = false;
    }

    void Start()
    {
        StaticPlayerInput.Input.Player.Sneaking.started += Sneak;
        StaticPlayerInput.Input.Player.Sneaking.canceled += Sneak;
    }

    private void Update()
    {
        if (!sneaking && inRange && !withinSight) 
        { 
            if (Vector3.Distance(player.position, lastPosition) > 3 && Unobstructed(player.position))
            {
                lastPosition = player.position;
                HeardSomething.Invoke(lastPosition);
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
        else if (other.gameObject.layer == 8)
        {
            lastPosition = other.transform.position;
            HeardSomething.Invoke(lastPosition);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            inRange = false;
        }
    }
}
