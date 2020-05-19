using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyBikeBrainMP : MonoBehaviour, IDamageable
{
    [HideInInspector]
    public float leanVal;
    [HideInInspector]
    public float turnVal;

    public float hoverHeight = 4.0f;
    public float hoverForce = 10.0f;
    public float hoverDamp = 1f;
    public float maxSpeed = 20;
    public float acceleration = 20;
    public float thrust = 40;
    public float turnSpeed = 75;
    public float attackCooldown;
    public float swordAppearanceDuration;
    public float meleeRange;
    public float attackAngle;
    public float sightRange;
    public GameObject bike, thrusters, sword, head;
    private bikeBrainMP[] players;
    private bikeBrainMP closestPlayer;
    private Rigidbody rb;
    private EnemyNavigationMP en;
    private bool boosting;
    public bool canSeePlayer { get; private set; }
    private bool attackCoroutineStarted;
    public bool isDead { get; private set; }
    private float boostTimer;
    private float rangeToPlayer;
    private int layerMask;

    private int health;
    public int totalHealth;

    public AudioClip engineSound;
    public AudioClip swordSwing;
    public AudioClip swordHit;
    public AudioClip explosion;
    [SerializeField]
    private AudioSource engineSoundSource;
    [SerializeField]
    private AudioSource swordSwingSource;
    [SerializeField]
    private AudioSource swordHitSource;
    [SerializeField]
    private AudioSource explosionSource;

    public float minEnginePitch;
    public float maxEnginePitch;
    public float minEngineVolume;
    public float maxEngineVolume;
    public float maxEngineNoiseTarget;

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
        attackAngle = attackAngle / 2;
        rb = bike.GetComponent<Rigidbody>();
        en = GetComponent<EnemyNavigationMP>();
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        players = new bikeBrainMP[playerObjs.Length];
        for (int i = 0; i < playerObjs.Length; i++)
        {
            players[i] = playerObjs[i].GetComponent<bikeBrainMP>();
        }
            
        sword.SetActive(false);
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        boosting = false;
        attackCoroutineStarted = false;
        health = totalHealth;
        canSeePlayer = false;
        layerMask = 1 << LayerMask.NameToLayer("Terrain");

        engineSoundSource.clip = engineSound;
        swordHitSource.clip = swordHit;
        swordSwingSource.clip = swordSwing;
        explosionSource.clip = explosion;

        engineSoundSource.Play();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            closestPlayer = GetClosestPlayer();

            if (!isDead && !closestPlayer.isDead)
            {
                Vector3 direction = closestPlayer.transform.position - transform.position;
                bool hit = Physics.Raycast(transform.position, direction, direction.magnitude, layerMask);
                rangeToPlayer = Vector3.Distance(transform.position, closestPlayer.transform.position);
                float angleTo = Mathf.Acos(Vector3.Dot(direction.normalized, transform.forward)) * Mathf.Rad2Deg;

                if (!hit && rangeToPlayer < sightRange && angleTo < attackAngle)
                {
                    Debug.Log("Can see");
                    canSeePlayer = true;
                }

                if (canSeePlayer && angleTo < attackAngle)
                {
                    head.transform.LookAt(closestPlayer.transform);
                    if (rangeToPlayer <= meleeRange && !attackCoroutineStarted)
                    {
                        StartCoroutine("Attack");
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient && !isDead)
        {
            RaycastHit hit;

            Ray downRay = new Ray(bike.transform.position, -Vector3.up);

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
                }
            }



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
        boostTimer = timer;
    }

    IEnumerator Attack()
    {
        attackCoroutineStarted = true;
        sword.SetActive(true);
        swordSwingSource.Play();
        en.ResetWeights(Random.Range(0.1f, 1f));

        yield return new WaitForSeconds(swordAppearanceDuration);

        sword.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);

        attackCoroutineStarted = false;
    }

    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        hoverHeight = 0;

        engineSoundSource.Stop();

        explosionSource.Play();

        isDead = true;

        EnemySpawner.enemiesRemaining--;

        Debug.Log("ohgodwhy");
    }

    public bikeBrainMP GetClosestPlayer()
    {
        float closest = Mathf.Infinity;
        bikeBrainMP closestPlayer = null;

        foreach (bikeBrainMP player in players)
        {
            if (!player.isDead)
            {
                float distance = Vector3.Distance(player.transform.position, transform.position);

                if (distance < closest)
                {
                    closest = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }
}
