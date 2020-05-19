using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bikeControllerV2 : MonoBehaviour
{
    private Rigidbody rb;
    public float thrustSpeed, rotateSpeed, maxSpeed;
    public GameObject cam;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        
        float vert = Input.GetAxis("Vertical");
        rb.AddForce(transform.forward * thrustSpeed * vert);
        
        float horiz = Input.GetAxis("Horizontal");
        transform.localRotation = Quaternion.AngleAxis(cam.transform.localRotation.y, Vector3.up);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        
    }
}
