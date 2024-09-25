using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    NavMeshAgent agent;
    Rigidbody rb;

    [SerializeField] Transform target;
    [SerializeField] float speed;

    NavMeshPath navPath;
    Queue<Vector3> pathNodes = new();
    Vector3 targetPoint;

    bool finished = false;
    void Start()
    {
        navPath = new NavMeshPath();

        //agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent.CalculatePath(target.position, navPath))
        {
            foreach (Vector3 node in navPath.corners)
            {
                pathNodes.Enqueue(node);
            }

            targetPoint = pathNodes.Dequeue();
        }
    }

    private void Update()
    {
        Vector3 newpos = (targetPoint - transform.position).normalized;
        newpos.y = 0;
        transform.forward = newpos;

        if (Vector3.Distance(transform.position, targetPoint) < 1.5)
        {
            if (pathNodes.Count > 0)
            {
                Debug.Log("dequeue");
                targetPoint = pathNodes.Dequeue();
            }
            else
            {
                finished = true;
            }

        }
    }

    void FixedUpdate()
    {
        if (!finished)
            rb.velocity = transform.forward * speed;
        else
            rb.velocity = new();
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
