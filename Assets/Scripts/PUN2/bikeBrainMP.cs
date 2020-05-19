using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class bikeBrainMP : MonoBehaviour, IDamageable
{
    private float leanVal, turnVal, helper;
    public float acceleration = 20;
    public float drag = .5f;
    public float turnSpeed = 75;
    public float hoverHeight = 4.0f;
    public float hoverForce = 10.0f;
    public float hoverDamp = .1f;
    public float maxSpeed = 40;
    public float boostScale = 1.5f;
    public float boostGrowthRate = .25f;
    public float minBoost = 15f; //If trying to boost with less than this amount, won't boost
    public float maxBoost = 30f;
    public float driftDrag = .05f;
    public float driftBoostTimeLimit = 3f;
    public GameObject bike, thrusters, cam, sword;

    private float boostAmount = 0f;

    private Rigidbody rb;
    private bool boosting;

    private bool drifting = false;
    private bool doneDrifting = true;
    private bool canBoost = true;
    private bool inAir = false;
    private float driftHoverHeight;
    private float hoverHeightHold;

    public bool isDead { get; private set; }
    private float boostTimer;
    private int layerMask;

    [HideInInspector]
    public int health;
    public int totalHealth;

    public AudioClip engineSound;
    public AudioClip swordSwing;
    public AudioClip swordHit;

    [SerializeField]
    private AudioSource engineSoundSource;
    [SerializeField]
    private AudioSource swordSwingSource;
    [SerializeField]
    private AudioSource swordHitSource;

    public float minEnginePitch;
    public float maxEnginePitch;
    public float minEngineVolume;
    public float maxEngineVolume;
    public float maxEngineNoiseTarget;

    public Slider boostSlider;
    public Slider healthSlider;

    private Image boostFill;

    // This works by measuring the distance to ground with a
    // raycast then applying a force that decreases as the object
    // reaches the desired levitation height.
    // The greater the thrust, the more hover power/height you need;
    // Vary the parameters below to
    // get different control effects. For example, reducing the
    // hover damping will tend to make the object bounce if it
    // passes over an object underneath.

    void Start()
    {
        // Fairly high drag makes the object easier to control.
        rb = bike.GetComponent<Rigidbody>();
        rb.drag = drag;
        rb.angularDrag = 0.5f;
        helper = turnSpeed;
        health = totalHealth;
        boosting = false;
        sword.SetActive(false);
        isDead = false;
        layerMask = 1 << LayerMask.NameToLayer("Terrain");
        swordSwingSource.clip = swordSwing;
        engineSoundSource.clip = engineSound;
        swordHitSource.clip = swordHit;

        engineSoundSource.Play();

        hoverHeightHold = hoverHeight;
        driftHoverHeight = hoverHeightHold * (3f / 4f);

        boostSlider = transform.Find("UI/Boost/Slider").GetComponent<Slider>();
        healthSlider = transform.Find("UI/Health/Slider").GetComponent<Slider>();
        boostFill = boostSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

        if (!gameObject.GetPhotonView().IsMine)
        {
            transform.Find("GameObject/Camera").GetComponent<Camera>().enabled = false;
            transform.Find("UI").gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameObject.GetPhotonView().IsMine && !isDead)
        {
            if (Input.GetMouseButton(0))
            {
                sword.SetActive(true);
                if (Input.GetMouseButtonDown(0))
                {
                    swordSwingSource.Play();
                }
            }
            else
            {
                sword.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        if (gameObject.GetPhotonView().IsMine)
        {
            if (!isDead)
            {
                RaycastHit hit;
                Ray downRay = new Ray(bike.transform.position, -Vector3.up);

                drifting = Input.GetKey(KeyCode.LeftShift);
                doneDrifting = Input.GetKeyUp(KeyCode.LeftShift);

                // Cast a ray straight downwards.
                if (Physics.Raycast(downRay, out hit, Mathf.Infinity, layerMask))
                {
                    // The "error" in height is the difference between the desired height
                    // and the height measured by the raycast distance.
                    float hoverError = hoverHeight - hit.distance;


                    // Only apply a lifting force if the object is too low (ie, let
                    // gravity pull it downward if it is too high).
                    if (hoverError > 0)
                    {
                        // Subtract the damping from the lifting force and apply it to
                        // the rigidbody.
                        float upwardSpeed = rb.velocity.y;
                        float lift = hoverError * hoverForce - upwardSpeed * hoverDamp;
                        rb.AddForce(lift * thrusters.transform.up);
                        inAir = false;
                        canBoost = true;

                    }
                    else if (hoverError < -1)
                    {
                        inAir = true;
                    }

                }




                if (doneDrifting && canBoost)
                {
                    if (boostAmount > minBoost)
                    {
                        SetBoost(true, driftBoostTimeLimit);

                        boostAmount = Mathf.Clamp(boostAmount, minBoost, maxBoost);

                        Vector3 boost = transform.forward * boostAmount * boostScale;
                        rb.velocity = boost;
                        print("BOOSTED " + boostAmount);

                        if (inAir)
                        {
                            canBoost = false;
                        }
                        hoverHeight = hoverHeightHold;
                    }

                    boostAmount = 0f;
                }

                float engine = Math.Abs(Input.GetAxis("Vertical"));

                if (!drifting)
                {
                    hoverHeight = hoverHeightHold;
                    rb.drag = drag;
                    engineSoundSource.pitch = Mathf.Clamp(Map(engine, 0, 1, minEnginePitch, maxEnginePitch), minEnginePitch, maxEnginePitch);

                    if (Input.GetKey(KeyCode.W))
                    {
                        rb.AddForce(transform.forward * acceleration);
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                        rb.AddForce(transform.forward * -acceleration);
                    }
                    else
                    {
                        // leanVal = Mathf.MoveTowardsAngle(leanVal, 0f, accelleration * Time.deltaTime);
                    }
                }
                else
                {
                    hoverHeight = driftHoverHeight;
                    rb.drag = driftDrag;
                }

                float smooth = Mathf.Abs(Input.GetAxis("Horizontal"));
                if (Input.GetKey(KeyCode.D))
                {
                    turnVal = Mathf.MoveTowardsAngle(turnVal, turnVal + helper, smooth * turnSpeed * Time.deltaTime);

                    if (drifting)
                    {
                        boostAmount += boostGrowthRate;
                    }

                }
                else if (Input.GetKey(KeyCode.A))
                {
                    turnVal = Mathf.MoveTowardsAngle(turnVal, turnVal - helper, smooth * turnSpeed * Time.deltaTime);
                    if (drifting)
                    {
                        boostAmount += boostGrowthRate;

                    }


                }
                bike.transform.localRotation = Quaternion.AngleAxis(turnVal, Vector3.up);


                if (rb.velocity.magnitude > maxSpeed && !boosting)
                {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
                }

                if (boosting)
                {
                    boostTimer = boostTimer - Time.deltaTime;
                    if (boostTimer <= 0.0f)
                    {
                        boosting = false;
                    }
                }

                if (boostAmount > minBoost && canBoost)
                {
                    boostFill.color = Color.magenta;
                }
                else
                {
                    boostFill.color = Color.gray;
                }

                boostSlider.value = Mathf.Clamp01(boostAmount / maxBoost);

            }
        }
    }

    //function to map a number from one range of values to another, used for purposes of pitch shifting
    private float Map(float value, float minFrom, float maxFrom, float minTo, float maxTo)
    {
        return (value - minFrom) / (maxFrom - minFrom) * (maxTo - minTo) + minTo;
    }

    public void SetBoost(bool set, float timer)
    {
        boosting = set;
        boostTimer += timer;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        swordHitSource.Play();

        Debug.Log("Player damage damage taken! Health left: " + health);

        UpdateHealth();

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        hoverHeight = 0;

        engineSoundSource.Stop();

        Debug.Log("ohgodwhy");

        isDead = true;
    }

    public void UpdateHealth()
    {
        healthSlider.value = Mathf.Clamp01(((float)health / totalHealth));
    }
}