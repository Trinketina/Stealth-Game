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
    Return,
    Pursue
}
public class GuardAI : MonoBehaviour
{

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

    bool withinHearing = false, withinSight = false;
    int index = 0;
    float time = 0;

    

    public void InvestigateSpot(Vector3 position)
    {
        if (guard == Guard.Investigate)
        {
            if (Vector3.Distance(position, searchSpot) > 10)
            {
                searchSpot = position;
                PathTo(searchSpot);
            }
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

        switch (guard)
        {
            case Guard.Patrol:
                Debug.Log("Patrol");
                break;
            case Guard.Investigate:
                Debug.Log("Investigate");

                pathNodes.Clear();
                PathTo(searchSpot);

                break;
            case Guard.Return:
                Debug.Log("Return");
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
        }
    }

    private void Investigate()
    {
        if (MoveTowards(currentNode, maxspeed / 2))
        {
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
        if (MoveTowards(currentNode, maxspeed / 2))
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
