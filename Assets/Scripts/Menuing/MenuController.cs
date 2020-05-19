using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MenuController : MonoBehaviourPunCallbacks
{
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject lobbyPanel;
    public GameObject controlsPanel;

    private void Awake()
    {
        // Making sure scene syncing is on from the getgo, multiple other checks later.
        PhotonNetwork.AutomaticallySyncScene = true;

        // Attempting to connect right away.
        Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Log when we connect to master server.
    public override void OnConnectedToMaster()
    {
        Debug.Log("Joined master server.");
    }


    public void DemoScene()
    {
        SceneManager.LoadScene("PGC_Mesh Demo");
    }

    public void ShowCreditsPanel()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        creditsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        creditsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void JoinRandomLobby()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
