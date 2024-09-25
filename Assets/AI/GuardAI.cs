using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum Guard
{
    Patrol,
    Investigate,
    Return,
    Pursue
}
public class GuardAI : MonoBehaviour
{
    public UnityEvent OnPursue, OnInvestigate, OnReturn, OnCaught;


    [SerializeField] GameObject pathParent;
    List<Vector3> patrolPath = new();

    NavMeshPath navPath;
    Queue<Vector3> pathNodes = new();
    Vector3 currentNode;

    [SerializeField] float maxspeed;

    Rigidbody rb;
    NavMeshAgent agent;

    Guard guard;

    Player target;
    Vector3 searchSpot;

    bool withinHearing = false, withinSight = false;
    int index = 0;
    float time = 0;

    //IEnumerator currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        navPath = new();

        foreach (Transform t in pathParent.transform)
        {
            patrolPath.Add(t.position);
        }

        guard = Guard.Patrol;
    }

    private void Update()
    {
        switch (guard)
        {
            case Guard.Patrol:
                Patrol();
                break;
            case Guard.Investigate:
                Investigate();
                break;
            case Guard.Return:
                Return();
                break;
            case Guard.Pursue:
                Pursue();
                break;
        }
    }


    private void SwapState(Guard state)
    {
        

        switch (guard)
        {
            case Guard.Patrol:
                Debug.Log("Patrol");
                guard = state;
                break;
            case Guard.Investigate:
                PathTowards(target.transform.position);

                guard = state;
                Debug.Log("Investigate");
                OnInvestigate.Invoke();
                break;
            case Guard.Return:
                if (!PathTowards(patrolPath[index]))
                {
                    SwapState(Guard.Patrol);
                    break;
                }

                time = 0;
                guard = state;
                Debug.Log("Return");
                OnReturn.Invoke();
                break;
            case Guard.Pursue:
                guard = state;
                Debug.Log("Pursue");
                OnPursue.Invoke();
                break;
        }
    }

    private void Patrol()
    {
        if (IsSeen())
        {
            SwapState(Guard.Pursue);
            return;
        }

        if (IsHeard())
        {
            SwapState(Guard.Investigate);
            return;
        }

        //otherwise patrol as per usual
        if (MoveTowards(patrolPath[index], maxspeed / 2))
        {
            index++;
            if (index == patrolPath.Count)
                index = 0;
        }
    }

    private void Investigate()
    {
        if (IsSeen())
        {
            SwapState(Guard.Pursue);
            return;
        }

        if (MoveTowards(currentNode, maxspeed / 2))
        {
            Debug.Log(pathNodes.Count);
            if (pathNodes.Count > 0)
            {
                currentNode = pathNodes.Dequeue();
            }
            else
            {
                SwapState(Guard.Return);
                return;
            }
        }
    }

    private void Return()
    {
        if (IsSeen())
        {
            SwapState(Guard.Pursue);
            return;
        }
        if (IsHeard())
        {
            SwapState(Guard.Investigate);
            return;
        }

        if (time < 3f)
        {
            time += Time.deltaTime;
            return;
        }
        else if (MoveTowards(currentNode, maxspeed / 2))
        {
            if (pathNodes.Count > 0)
            {
                currentNode = pathNodes.Dequeue();
            }
            else
            {
                SwapState(Guard.Patrol);
                return;
            }
        }
    }

    private void Pursue()
    {
        if (!IsSeen())
        {
            SwapState(Guard.Investigate);
            return;
        }

        if (MoveTowards(target.transform.position, maxspeed))
        {
            if (IsSeen()) //the player has been caught
            {
                OnCaught.Invoke();
                return;
            }
            else
            {
                SwapState(Guard.Investigate);
                return;
            }
        }
    }

    private bool PathTowards(Vector3 position)
    {
        navPath = new();

        if (agent.CalculatePath(position, navPath))
        {
            pathNodes.Clear();
            foreach (Vector3 node in navPath.corners)
            {
                pathNodes.Enqueue(node);
            }
            currentNode = pathNodes.Dequeue();
            return true;
        }

        return false;
    }
    
    private bool MoveTowards(Vector3 position, float speed) //true if it has reached its destination
    {
        if (Vector3.Distance(transform.position, position) < 1.2f)
        {
            rb.velocity = Vector3.zero;
            return true;
        }

        Vector3 goTo = (position - transform.position).normalized;
        goTo.y = 0;
        transform.forward = goTo;

        transform.rotation = Quaternion.LookRotation(goTo);
        rb.velocity = transform.forward * speed;

        
        return false;
    }

    private bool IsHeard()
    {
        if (withinHearing)
        {
            if (!target.Sneaking)
            {
                searchSpot = target.transform.position;
                return true;
            }
        }

        return false;
    }

    private bool IsSeen()
    {
        if (withinSight)
        {
            Vector3 diff = (target.transform.position - transform.position).normalized;
            if (Vector3.Dot(diff, transform.forward) > .75f)
            {
                searchSpot = target.transform.position;
                return true;
            }
        }

        return false;
    }

    public void EnterSightRange(Player player)
    {
        target = player;
        withinSight = true;
    }
    public void EnterHearingRange(Player player)
    {
        target = player;
        withinHearing = true;
    }
    public void ExitSightRange(Player player)
    {
        withinSight = false;
    }
    public void ExitHearingRange(Player player)
    {
        withinHearing = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            SceneManager.LoadScene(0);
        }
    }


    private void OnDrawGizmos()
    {
        if (navPath == null)
            return;
        Gizmos.color = Color.red;
        foreach (Vector3 node in navPath.corners)
        {
            Gizmos.DrawWireSphere(node, 1);
        }
    }
}
