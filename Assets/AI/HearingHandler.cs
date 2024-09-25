using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HearingHandler : MonoBehaviour
{
    public UnityEvent<Vector3> HeardSomething;

    [SerializeField] Transform player;
    [SerializeField] float range;

    bool sneaking = false;
    bool withinSight = false;
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

    void Update()
    {
        Debug.Log(Vector3.Distance(transform.position, player.position));
        if (Vector3.Distance(transform.position, player.position) < range && !sneaking && !withinSight) 
        {
            HeardSomething.Invoke(player.position);
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        //if a thrown thing enters the trigger, invoke UpdateLastHeard
    }
}
