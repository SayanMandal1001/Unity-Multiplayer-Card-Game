using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotClick : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void OnMouseDown()
    {
        if(gameManager != null && gameManager.haveAllJoined)
        {
            bool foundCard = false;
            for (int i = 0; i < gameManager.Deck.Length; i++)
            {
                if (gameManager.Deck[i].getOwner() == owner.Deck && gameManager.Deck[i].getPosition() == position.None)
                {
                    foundCard = true;
                    break;
                }
            }
            if (foundCard)
            {
                if (gameManager.getCurrentTurnPlayerId() == gameManager.playerId && gameManager.canPickFromDeckOrStack)
                {
                    Debug.Log("Picked a card from deck");
                    gameManager.getTopDeckCardRpc(gameManager.playerId);
                    gameManager.canReplace = true;
                    gameManager.canSkip = true;
                    gameManager.canPickFromDeckOrStack = false;
                    gameManager.startTurn = true;
                }
            }
            else
            {
                gameManager.repopulateDeckRpc();
            }
        }
    }
}
