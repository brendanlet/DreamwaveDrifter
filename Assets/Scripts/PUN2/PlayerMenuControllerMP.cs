using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class PlayerMenuControllerMP : MonoBehaviour
{
    private GameObject menu;
    private bikeBrainMP player;
    private bool inMenu = false;
    private bool stillPlaying = true;

    private TextMeshProUGUI flavorText;

    private void Start()
    {
        menu = transform.Find("UI/Navigation").gameObject;
        player = GetComponent<bikeBrainMP>();
        flavorText = transform.Find("UI/Flavor").gameObject.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        inMenu = menu.activeInHierarchy;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if (EnemySpawnerMP.enemiesRemaining <= 0 && stillPlaying)
        {
            flavorText.text = "YOU WIN!";
            flavorText.gameObject.SetActive(true);
            stillPlaying = false;
            inMenu = false;
            ToggleMenu();
        }

        if (player.isDead && stillPlaying)
        {
            flavorText.text = "YOU LOSE!";
            flavorText.gameObject.SetActive(true);
            stillPlaying = false;
            inMenu = false;
            ToggleMenu();
        }
    }

    public void LoadMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main_Menu");
    }

    public void ToggleMenu()
    {
        if (inMenu)
        {
            menu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            menu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
