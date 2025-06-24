using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaitingAreaUIManager : MonoBehaviour
{
    public Text RoomCodeText;
    public Text[] playerNameText;
    public GameObject StartingTimer;
    public Text TimerText;

    public float Timer = 0f;
    public float maxTimer = 4f;
    private bool hasTimerStarted = false;

    public GameManager gameManager;
    public GameFlowManager gameFlowManager;
    public GameObject StartButton;
    public GameObject WaitingText;
    public GameObject BackButton;
    

    private void Start()
    {
        Timer = 0f;
        StartButton.SetActive(false);
        WaitingText.SetActive(false);
        StartingTimer.SetActive(false);
        hasTimerStarted = false;
    }

    public void OnPressStartButton()
    {
        gameManager.StartWhenAllConnectedClientRpc();
    }

    private void Update()
    {
        RoomCodeText.text = "Room Code: " + gameManager.RelayCode; 
        for (int i = 0; i < playerNameText.Length; i++)
        {
            playerNameText[i].text = gameManager.playerNames[i];
        }

        if (Timer<=0 && !hasTimerStarted)
        {
            bool condition = true;
            for (int i = 0; i < 4; i++)
            {
                if (gameManager.playerNames[i] == "")
                {
                    condition = false;
                    playerNameText[i].text = "Connecting...";
                }
            }
            hasTimerStarted = condition;
            if(gameManager.playerId != 1) WaitingText.SetActive(false);
        }

        if (hasTimerStarted)
        {
            if(gameManager.playerId==1) StartButton.SetActive(true);
            else WaitingText.SetActive(true);
            hasTimerStarted = false;
        }
        if (Timer>0)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                gameManager.haveAllJoined = true;
                gameManager.isStartRoundCalled = true;
                gameFlowManager.ToMainGameArea();
            }
            TimerText.text = "Starting in " + ((int)Timer).ToString() + "...";
        }
    }
}
