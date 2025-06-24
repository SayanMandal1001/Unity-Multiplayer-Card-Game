using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackSlotClick : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {

    }

    private void Update()
    {
        if (gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void OnMouseDown()
    {
        if(gameManager != null && gameManager.haveAllJoined)
        {
            bool foundCard = false;
            for (int i = 0; i < gameManager.Deck.Length; i++)
            {
                if (gameManager.Deck[i].getOwner() == owner.Stack && gameManager.Deck[i].getPosition() == position.None)
                {
                    foundCard = true;
                    break;
                }
            }
            if (foundCard)
            {
                if (gameManager.getCurrentTurnPlayerId() == gameManager.playerId && gameManager.canPickFromDeckOrStack)
                {
                    Debug.Log("Picked a card from stack");
                    gameManager.getTopStackCardRpc(gameManager.playerId);
                    gameManager.canReplace = true;
                    gameManager.canPickFromDeckOrStack = false;
                    gameManager.startTurn = true;
                }
            }

            if (gameManager.canSkip)
            {
                Debug.Log("Placing the card on stack");
                gameManager.skipCardRpc();
                gameManager.canSkip = false;
                gameManager.canReplace = false;
            }
        }
    }
}
