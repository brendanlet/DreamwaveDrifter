using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerupController : MonoBehaviour
{

    public IPowerup powerup;
    public TextMeshProUGUI powerupLabel;
    private bool dropped = false;
    public GameObject heldPowerup;


    private void Start()
    {
        //powerupLabel = GameObject.Find("bikeObjV2/UI/Powerup/Label").GetComponent<TextMeshProUGUI>();
       powerupLabel = GameObject.FindGameObjectWithTag("Player").transform.Find("UI/Powerup/Label").GetComponent<TextMeshProUGUI>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            powerup.Activate();
            Destroy(this);
        }

        /*
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            dropped = true;
            Destroy(this);
        }
        */
    }

    private void OnDestroy()
    {
        if(dropped)
        {
            Instantiate(heldPowerup, transform.position, Quaternion.identity);
            Debug.Log("COOL BEANS");
        }
        else
        {
            powerup.Destroy();
        }
        
        powerupLabel.text = "None";

    }


}
