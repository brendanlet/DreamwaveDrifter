using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PowerupHandlerMP : MonoBehaviour
{

    private IPowerup powerup;
    public TextMeshProUGUI powerupLabel;
    public GameObject powerupPrefab;
    private GameObject prefab;
    public GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        powerup = GetComponent<IPowerup>();
        powerupLabel = GameObject.FindGameObjectWithTag("Player").transform.Find("UI/Powerup/Label").GetComponent<TextMeshProUGUI>();
        prefab = (GameObject)Resources.Load(powerupPrefab.name);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        model.transform.eulerAngles = new Vector3(-90, model.transform.eulerAngles.y + 5, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collided = other.gameObject;
        if (collided.tag == "Player" || collided.tag == "Sword")
        {
            if (collided.tag == "Sword")
            {
                collided = collided.GetComponent<SwordBlade>().bikePrefab;
            }

            if (collided.GetComponent<PowerupController>() == null)
            {
                var controller = collided.AddComponent<PowerupController>();
                controller.powerup = powerup;
                controller.heldPowerup = prefab;
                powerup.SetOwner(collided);
                powerupLabel.text = powerup.GetName();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
