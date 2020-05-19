using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public Text playerCount;
    public Button playButton;

    private void Awake()
    {
        // Disables the play button to wait for people to join the room first.
        playButton.gameObject.SetActive(false);

        // Makes sure that all people load the game when the master client hits the button.
        PhotonNetwork.AutomaticallySyncScene = true;

        // If we aren't already connected, connect.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void LoadMPScene()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(2);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        playButton.gameObject.SetActive(false);
    }

    // When we join the room...
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined new room.");
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " Players Connected.";

        // If we are the master client, we may push the button to start the game for everyone.
        if (PhotonNetwork.IsMasterClient)
            playButton.gameObject.SetActive(true);
    }

    // When someone joins or leaves the room, update the player count display.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " Players Connected.";
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + " Players Connected.";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            playButton.gameObject.SetActive(true);
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }
    }

    // Log if we connect to master server. Then attempt to join a random room.
    public override void OnConnectedToMaster()
    {
        Debug.Log("Joined master server.");
    }

    // If we cannot find a room to join, create one with a random name.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Could not find room, creating new one...");
        PhotonNetwork.CreateRoom("Room" + Random.Range(0, 100), new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4 });
    }

    // If we cannot create a room with this name, generate a new name and try again.
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Could not create room, retrying...");
        PhotonNetwork.CreateRoom("Room" + Random.Range(0, 100), new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 4 });
    }
}
