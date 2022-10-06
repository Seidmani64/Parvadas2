using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
	internal FlockController controller;
    private new Rigidbody rigidbody;
    private Vector3 randomize;
    public Transform leftTarget, rightTarget;  //Green Ray to left and right

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.LookAt(controller.target);

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 50;
        Debug.DrawRay(transform.position, forward, Color.green);
    }
    void FixedUpdate()
    {
        if (controller)
        {
            Vector3 relativePos = Steer() * Time.deltaTime;

            if(relativePos != Vector3.zero)
                rigidbody.velocity = relativePos;

            float speed = rigidbody.velocity.magnitude;

            if (speed > controller.maxVelocity)
                rigidbody.velocity = rigidbody.velocity.normalized * controller.maxVelocity;
                
            else if (speed < controller.minVelocity) 
                rigidbody.velocity = rigidbody.velocity.normalized * controller.minVelocity;
        }
    }

    //Calculate flock steering Vector based on the Craig Reynold's algorithm (Cohesion, Alignment, Follow leader and Seperation)
    private Vector3 Steer() 
    {
        Vector3 center = controller.flockCenter - transform.localPosition;          // cohesion
        Vector3 velocity = controller.flockVelocity - rigidbody.velocity;           // alignment
        Vector3 follow = controller.target.localPosition - transform.localPosition; // follow leader
        Vector3 separation = Vector3.zero; 											// separation
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 left = Vector3.RotateTowards(transform.forward, leftTarget.position - transform.position, 0.2f, 30f);    //Rotate to left
        Vector3 right = Vector3.RotateTowards(transform.forward, rightTarget.position - transform.position, 0.2f, 30f);    //Rotate to right
        Vector3 avoid = Vector3.zero;
        RaycastHit rayHitStraight;
        RaycastHit rayHitSide;
        Debug.DrawRay(transform.position, left, Color.green);
        Debug.DrawRay(transform.position, right, Color.green);
        if(Physics.Raycast(transform.position, forward, out rayHitStraight, 50))
        {
            //If the flock collides with an "Obstacle" tag object
            if(rayHitStraight.collider.tag == "Obstacle")
            {
                Debug.Log("Obstacle straight ahead!");
                if(!(Physics.Raycast(transform.position, left, out rayHitSide, 30)))
                {
                    avoid = left - rayHitStraight.normal;
                }
                else if(!(Physics.Raycast(transform.position, right, out rayHitSide, 30)))
                {
                    avoid = right - rayHitStraight.normal;
                }
                else
                {
                    avoid = right - rayHitStraight.normal;
                }
            }
        }

        foreach (Flock flock in controller.flockList) 
        {
            if (flock != this) 
            {
                Vector3 relativePos = transform.localPosition - flock.transform.localPosition;
                separation += relativePos.normalized;
            }
        }

        // Randomize the direction every 2 seconds
        if(Time.time % 2==0)
        {
            randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
            randomize.Normalize();
        }

        Debug.DrawRay(transform.position, avoid, Color.red);

        return (controller.centerWeight * center +
                controller.velocityWeight * velocity +
                controller.separationWeight * separation +
                controller.followWeight * follow +
                controller.randomizeWeight * randomize +
                controller.avoidWeight * avoid);
    }	
}
