using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Inspect,
    Return,
    Pursue
}
public class GuardAI : MonoBehaviour
{
    public UnityEvent<Guard> OnSwapState;


    [SerializeField] GameObject pathParent;
    List<Vector3> patrolPath = new();

    NavMeshPath navPath;
    Queue<Vector3> pathNodes = new();
    Vector3 currentNode;

    [SerializeField] float maxspeed;

    Rigidbody rb;
    NavMeshAgent agent;

    Guard guard;

    Transform target;
    Vector3 searchSpot;

    Quaternion lookLeft;
    Quaternion lookRight;

    int index = 0;
    float time = 0;
    [SerializeField] float waitFor;
    [SerializeField] float interpolation;

    public void InvestigateSpot(Vector3 position)
    {
        if (guard == Guard.Investigate)
        {
            time = 0;
            searchSpot = position;
            pathNodes.Clear();
            PathTo(searchSpot);
        }
        else
        {
            searchSpot = position;
            SwapState(Guard.Investigate);
        }
    }
    public void SpotEnemy(Transform enemy)
    {
        if (guard != Guard.Pursue)
        {
            target = enemy;
            SwapState(Guard.Pursue);
        }
    }

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
            case Guard.Inspect:
                Inspect(); 
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
        guard = state;
        time = 0;
        OnSwapState.Invoke(guard);

        switch (guard)
        {
            case Guard.Patrol:
                Debug.Log("Patrol");
                break;
            case Guard.Investigate:
                Debug.Log("Investigate");

                pathNodes.Clear();

                if (!PathTo(searchSpot))
                    SwapState(Guard.Return);

                break;
            case Guard.Inspect:
                Debug.Log("Inspect");

                lookLeft = transform.rotation;
                lookLeft.y -= Quaternion.Euler(0, 30f, 0).y;

                lookRight = transform.rotation;
                lookRight.y += Quaternion.Euler(0, 30f, 0).y;


                break;
            case Guard.Return:
                Debug.Log("Return");

                pathNodes.Clear();
                if (!PathTo(patrolPath[index]))
                    SwapState(Guard.Patrol);

                break;
            case Guard.Pursue:
                Debug.Log("Pursue");
                break;
        }
    }

    private void Patrol()
    {
        if (MoveTowards(patrolPath[index], maxspeed / 2))
        {
            index++;
            if (index == patrolPath.Count)
                index = 0;

            SwapState(Guard.Inspect);
        }
    }

    private void Investigate()
    {
        if (time < waitFor / 2)
        {
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation((searchSpot - transform.position).normalized, Vector3.zero), interpolation);
            time += Time.deltaTime;
            return;
        }
        if (MoveTowards(currentNode, maxspeed / 2))
        {
            if (pathNodes.Count > 0)
            {
                currentNode = pathNodes.Dequeue();
            }
            else
            {
                time = 0;
                SwapState(Guard.Inspect);
                return;
            }
        }
    }

    private void Inspect()
    {

        if (time < waitFor / 2)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookLeft, interpolation);
            time += Time.deltaTime;
        }
        else if (time < waitFor)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRight, interpolation);
            time += Time.deltaTime;
        }
        else
        {
            time = 0;
            SwapState(Guard.Return);
        }
    }

    private void Return()
    {
        if (time < waitFor / 2)
        {
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                Quaternion.LookRotation((patrolPath[index] - transform.position).normalized, Vector3.zero), interpolation);
            time += Time.deltaTime;
            return;
        }

        if (MoveTowards(currentNode, maxspeed / 2))
        {
            if (pathNodes.Count > 0)
            {
                currentNode = pathNodes.Dequeue();
            }
            else
            {
                SwapState(Guard.Patrol);
                time = 0;
                return;
            }
        }
    }

    private void Pursue()
    {
        if (MoveTowards(target.transform.position, maxspeed))
        {

        }
    }

    private bool PathTo(Vector3 position)
    {
        if (agent.CalculatePath(position, navPath))
        {
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
