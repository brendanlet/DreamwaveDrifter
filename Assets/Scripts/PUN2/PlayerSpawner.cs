using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    private void Awake()
    {
        GameObject myPlayer = PhotonNetwork.Instantiate("bikeObjV2_MP", new Vector3(Random.Range(0, 401), 5, 0), Quaternion.identity);
        myPlayer.transform.Find("GameObject/Camera").gameObject.AddComponent<AudioListener>();
    }
}
