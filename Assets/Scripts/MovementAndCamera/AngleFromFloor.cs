using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleFromFloor : MonoBehaviour
{
    public Rigidbody rb;
    public float AdjustmentScale = 1;
    [SerializeField]
    private float ForwardLeanLimit = 35;
    [SerializeField]
    private float BackwardLeanLimit = 325;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {

        RestrictXRotation(ForwardLeanLimit, BackwardLeanLimit);
        AddTorqueFromFloorAngleDifference();

    }

    private void RestrictXRotation(float forwardLimit, float backLimit)
    {
        Vector3 newEulers = transform.eulerAngles;
        if (newEulers.x > forwardLimit && newEulers.x < 180)
        {
            rb.AddRelativeTorque(new Vector3(-rb.angularVelocity.x, 0, 0));
        }
        else if (newEulers.x < backLimit && newEulers.x >= 180)
        {
            rb.AddRelativeTorque(new Vector3(-rb.angularVelocity.x, 0, 0));
        }

        Debug.Log(newEulers.x);

        //transform.eulerAngles = newEulers;
        
        
    }

    

    private void AddTorqueFromFloorAngleDifference()
    {
        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -Vector3.up);


        // Cast a ray straight downwards.
        if (Physics.Raycast(downRay, out hit))
        {


            Vector3 referenceVec = transform.forward; //The way the object is facing
            //referenceVec.y = 0; //Setting the y to 0 makes the vector parallel to flat ground

            float angleDiff = 90 - Vector3.Angle(referenceVec, hit.normal);
            Debug.Log(angleDiff);
            rb.AddRelativeTorque(new Vector3(angleDiff * AdjustmentScale/1000, 0, 0));

        }
    }
}
