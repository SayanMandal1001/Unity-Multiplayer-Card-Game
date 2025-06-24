using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClick : MonoBehaviour
{
    private GameManager gameManager;
    private CardSlotManager cardSlot;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(cardSlot == null) cardSlot = this.GetComponent<CardSlotManager>();
    }

    public void OnMouseDown()
    {
        if(gameManager != null && gameManager.haveAllJoined)
        {
            bool foundCard = false;
            for (int i = 0; i < gameManager.Deck.Length; i++)
            {
                if (gameManager.Deck[i].getOwner() == cardSlot.getOwner() && gameManager.Deck[i].getPosition() == cardSlot.getPosition())
                {
                    foundCard = true;
                    break;
                }
            }
            if (foundCard && cardSlot.getOwner() != (owner)gameManager.kaboCalledByPlayerId)
            {
                owner correctedOwner = cardSlot.getOwner();
                if (gameManager.canReplace && correctedOwner == (owner)(int)gameManager.playerId)
                {
                    Debug.Log("Clicked on:" + gameObject.name);
                    gameManager.replaceCardRpc(cardSlot.getOwner(), cardSlot.getPosition(), gameManager.playerId);
                    gameManager.canSkip = false;
                    gameManager.canReplace = false;
                }
                else if (gameManager.canStack)
                {
                    Debug.Log("Stacking card on:" + gameObject.name);
                    gameManager.stackCardRpc(cardSlot.getOwner(), cardSlot.getPosition(), gameManager.playerId);
                    gameManager.canStack = false;
                }
                else if (gameManager.canSeenSwap && gameManager.canSeeOthersCard && correctedOwner != (owner)(int)gameManager.playerId && correctedOwner <= owner.Player4 && correctedOwner >= owner.Player1)
                {
                    Debug.Log("Selected others card for seen swipe :" + gameObject.name);
                    gameManager.setOthersSwapCardRpc(cardSlot.getOwner(), cardSlot.getPosition());
                    gameManager.seeCardRpc((owner)(int)gameManager.playerId, cardSlot.getOwner(), cardSlot.getPosition());
                    gameManager.canSeeOthersCard = false;
                }
                else if (gameManager.canSeenSwap && correctedOwner == (owner)(int)gameManager.playerId)
                {
                    Debug.Log("Selected own card for seen swipe :" + gameObject.name);
                    gameManager.setOwnSwapCardRpc(cardSlot.getOwner(), cardSlot.getPosition());

                }
                else if (gameManager.canUnseenSwap && correctedOwner != (owner)(int)gameManager.playerId && correctedOwner <= owner.Player4 && correctedOwner >= owner.Player1)
                {
                    Debug.Log("Selected others card for seen swipe :" + gameObject.name);
                    gameManager.setOthersSwapCardRpc(cardSlot.getOwner(), cardSlot.getPosition());
                    gameManager.canSeeOthersCard = false;
                }
                else if (gameManager.canUnseenSwap && correctedOwner == (owner)(int)gameManager.playerId)
                {
                    Debug.Log("Selected own card for seen swipe :" + gameObject.name);
                    gameManager.setOwnSwapCardRpc(cardSlot.getOwner(), cardSlot.getPosition());

                }
                else if (gameManager.canSeeSelfCard && correctedOwner == (owner)(int)gameManager.playerId)
                {
                    Debug.Log("Seeing self card: " + gameObject.name);
                    gameManager.seeCardRpc((owner)(int)gameManager.playerId, cardSlot.getOwner(), cardSlot.getPosition());
                    gameManager.canSeeSelfCard = false;
                }
                else if (gameManager.canSeeOthersCard && correctedOwner != (owner)(int)gameManager.playerId && correctedOwner <= owner.Player4 && correctedOwner >= owner.Player1)
                {
                    Debug.Log("Seeing others card: " + gameObject.name);
                    gameManager.seeCardRpc((owner)(int)gameManager.playerId, cardSlot.getOwner(), cardSlot.getPosition());
                    gameManager.canSeeOthersCard = false;

                }
                else if (gameManager.canGiveOtherACard && correctedOwner == (owner)(int)gameManager.playerId)
                {
                    Debug.Log("Give other player card: " + gameObject.name);
                    gameManager.giveOtherACardRpc(cardSlot.getOwner(), cardSlot.getPosition());
                }
            }
        }
    }
}
