using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour
{
    public GameObject startButton;

    private void Start()
    {
        startButton.SetActive(true);        
    }
    public void JoinAsHost()
    {
        this.GetComponent<NetworkManager>().StartHost();
        startButton.SetActive(true);
    }
    public void JoinAsClient()
    {
        this.GetComponent<NetworkManager>().StartClient();
    }
}
