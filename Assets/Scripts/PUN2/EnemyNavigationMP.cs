using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyNavigationMP : MonoBehaviour
{
    private Rigidbody rb;
    private EnemyBikeBrainMP ebb;
    private GameObject[] patrolRoute;
    public float epsilon;
    public float distanceOffset;
    private int patrolIndex;
    private float modifier;
    protected float retreatWeight;
    protected float pursueWeight;

    // Start is called before the first frame update
    void Awake()
    {
        modifier = 1;
        retreatWeight = 0;
        pursueWeight = 1;
        rb = GetComponent<Rigidbody>();
        ebb = GetComponent<EnemyBikeBrainMP>();
        patrolRoute = GameObject.FindGameObjectsWithTag("Patrol Point");
        patrolIndex = Random.Range(0, patrolRoute.Length);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient && !ebb.isDead)
        {
            Vector3 direction;

            if (!ebb.canSeePlayer)
            {
                direction = transform.position - patrolRoute[patrolIndex].transform.localPosition;
                direction.y = 0;
                Debug.Log("Approaching " + patrolIndex);
                float dotRight = Vector3.Dot(transform.right.normalized, direction.normalized);
                float dotForward = Vector3.Dot(transform.forward.normalized, direction.normalized);
                Debug.Log("Dot right: " + dotRight);
                Debug.Log("Dot forward: " + dotForward);

                if (dotRight < (0 - epsilon))
                {
                    SteerRight();
                    Debug.Log("Steering right");
                }
                else if (dotRight > (0 + epsilon))
                {
                    SteerLeft();
                    Debug.Log("Steering left");
                }
                else if (dotForward < (-1 + epsilon))
                {
                    MoveForward();
                    Debug.Log("Moving forward");
                }
                else if (dotForward > (1 - epsilon))
                {
                    SteerLeft();
                    Debug.Log("Steering left");
                }
            }
            else
            {
                direction = transform.position - ebb.GetClosestPlayer().gameObject.transform.localPosition;
                direction.y = 0;

                float dot = Vector3.Dot(transform.right, direction);

                if (pursueWeight > retreatWeight)
                {
                    if (dot < (0 - epsilon))
                    {
                        SteerRight();
                        Debug.Log("Steering right, pursuing");
                    }
                    else if (dot > (0 + epsilon))
                    {
                        SteerLeft();
                        Debug.Log("Steering left, pursuing");
                    }

                    MoveForward();
                }
                else
                {
                    if (dot < (0 - epsilon))
                    {
                        SteerLeft();
                        Debug.Log("Steering left, retreating");
                    }
                    else if (dot > (0 + epsilon))
                    {
                        SteerRight();
                        Debug.Log("Steering right, retreating");
                    }

                    MoveForward();

                    if (pursueWeight <= retreatWeight)
                    {
                        pursueWeight += Time.deltaTime * modifier;
                    }
                }
            }

            if (rb.velocity.magnitude > ebb.maxSpeed)
            {
                Vector3.ClampMagnitude(rb.velocity, ebb.maxSpeed);
            }

            float distance = direction.magnitude;

            Debug.Log(distance);

            if (distance < distanceOffset)
            {
                int newPatrolPoint;

                do
                {
                    newPatrolPoint = Random.Range(0, patrolRoute.Length);
                } while (newPatrolPoint == patrolIndex);

                patrolIndex = newPatrolPoint;
            }
        }
    }

    void MoveForward()
    {
        rb.AddForce(transform.forward * ebb.acceleration);
    }

    void SteerLeft()
    {
        ebb.turnVal = Mathf.MoveTowardsAngle(ebb.turnVal, ebb.turnVal - ebb.turnSpeed, ebb.turnSpeed * Time.deltaTime);
        ebb.bike.transform.localRotation = Quaternion.AngleAxis(ebb.turnVal, Vector3.up);
    }

    void SteerRight()
    {
        ebb.turnVal = Mathf.MoveTowardsAngle(ebb.turnVal, ebb.turnVal + ebb.turnSpeed, ebb.turnSpeed * Time.deltaTime);
        ebb.bike.transform.localRotation = Quaternion.AngleAxis(ebb.turnVal, Vector3.up);
    }

    public void ResetWeights(float _modifier)
    {
        pursueWeight = 0;
        retreatWeight = 1;

        modifier = _modifier;
    }
}
