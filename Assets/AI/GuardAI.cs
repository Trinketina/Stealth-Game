using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
        SwapState(Guard.Patrol);
    }

    private void SwapState(Guard state)
    {
        guard = state;

        switch (guard)
        {
            case Guard.Patrol:
                StartCoroutine(Patrol());
                break;
            case Guard.Investigate:
                StartCoroutine(Investigate());
                break;
            case Guard.Return:
                StartCoroutine(Return());
                break;
            case Guard.Pursue:
                StartCoroutine(Pursue());
                break;
        }
    }

    private IEnumerator Patrol()
    {
        bool patrolling = true;
        int index = 0;

        do
        {
            if (IsSeen())
            {
                SwapState(Guard.Pursue);
                patrolling = false;
                yield break;
            }

            if (IsHeard())
            {
                SwapState(Guard.Investigate);
                patrolling = false;
                yield break;
            }
            //otherwise patrol as per usual

            if (MoveTowards(patrolPath[index], maxspeed / 2))
            {
                yield return new WaitForSeconds(1f);
                index++;
                if (index == patrolPath.Count)
                    index = 0;
            }

            yield return new WaitForFixedUpdate();

        } while (patrolling);
    }

    private IEnumerator Investigate()
    {
        bool investigating = true;

        if (agent.CalculatePath(target.transform.position, navPath))
        {
            foreach (Vector3 node in navPath.corners)
            {
                pathNodes.Enqueue(node);
            }
            currentNode = pathNodes.Dequeue();
        }
        else
        {
            investigating = false;
            SwapState(Guard.Return);
            yield break;
        }

        do
        {
            if (IsSeen())
            {
                SwapState(Guard.Pursue);
                investigating = false;
                yield break;
            }

            if (MoveTowards(currentNode, maxspeed / 2) && !IsHeard())
            {
                if (pathNodes.Count > 0)
                {
                    currentNode = pathNodes.Dequeue();
                }
                else
                {
                    SwapState(Guard.Return);
                    investigating = false;
                    yield break;
                }
            }

            yield return new WaitForFixedUpdate();
        } while (investigating);
    }

    private IEnumerator Return()
    {
        bool returning = true;
        float time = 0;

        if (agent.CalculatePath(target.transform.position, navPath))
        {
            foreach (Vector3 node in navPath.corners)
            {
                pathNodes.Enqueue(node);
            }
            currentNode = pathNodes.Dequeue();
        }
        else
            yield break;

        while (returning)
        {
            if (IsSeen())
            {
                SwapState(Guard.Pursue);
                returning = false;
                yield break;
            }
            if (IsHeard())
            {
                SwapState(Guard.Investigate);
                returning = false;
                yield break;
            }

            if (time < 3f)
            {
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
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
                    returning = false;
                    yield break;
                }
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Pursue()
    {
        bool pursuing = true;
        do
        {
            if (!IsSeen())
            {
                SwapState(Guard.Investigate);
                pursuing = false;
                yield break;
            }

            if (MoveTowards(target.transform.position, maxspeed))
            {
                if (IsSeen()) //the player has been caught
                {
                    OnCaught.Invoke();
                    pursuing = false;
                    yield break;
                }
                else
                {
                    SwapState(Guard.Investigate);
                    pursuing = false;
                    yield break;
                }
            }

            yield return new WaitForFixedUpdate();
        } while (pursuing);

        yield return new WaitForEndOfFrame();
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
                Debug.Log("seen");
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
}
