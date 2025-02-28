using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class nav : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;
    Animator anim;

    Vector3 earlyPosition;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        earlyPosition = transform.position;
        InvokeRepeating("capturePos", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
       
        if(agent.velocity.magnitude > 0)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        Vector2 direction = (transform.position - earlyPosition).normalized;

      
        anim.SetFloat("x", direction.x);
        anim.SetFloat("y", direction.y);

    }

    void capturePos()
    {

        earlyPosition = transform.position;
    }
}
