using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlotManager : MonoBehaviour
{
    [SerializeField] private owner Owner;
    [SerializeField] private position Position;
    private GameManager gameManager;

    private void Start()
    {
        if (Owner <= (owner)6) transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;

    }

    public owner getOwner() { return  Owner; }
    public position getPosition() { return Position; }

    public void setCorrectOwner()
    {
        if (gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        Owner = gameManager.getCorrectedSlotOwner(Owner, gameManager.playerId);
    }

    public Card getCard()
    {
        Card card = new Card(suit.None,value.None);
        if (gameManager.haveAllJoined && !gameManager.isStartRoundCalled)
        {
            if (gameManager.seenCardIndex != -1 && gameManager.originalOwner == Owner && gameManager.originalPosition == Position)
            {
                card = gameManager.Deck[gameManager.seenCardIndex];
            }
            else
            {
                for (int i = 0; i < gameManager.Deck.Length; i++)
                {
                    if (gameManager.Deck[i].getOwner() == Owner && gameManager.Deck[i].getPosition() == Position)
                    {
                        card = gameManager.Deck[i];
                    }
                }
            }
        }
        return card;
    }

    private void Update()
    {
        if(gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(gameManager != null && gameManager.haveAllJoined)
        {
            if (Owner <= (owner)4)
            {
                Card card = getCard();
                if (!card.Equals(new Card(suit.None, value.None)) && Owner != (owner)gameManager.kaboCalledByPlayerId)
                {
                    this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                    if (Owner == (owner)gameManager.playerId)
                    {
                        if (gameManager.canReplace || gameManager.canStack || gameManager.canSeeSelfCard || gameManager.canSeenSwap || gameManager.canUnseenSwap || gameManager.canGiveOtherACard) transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        else transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        if (gameManager.canStack || gameManager.canSeeOthersCard || gameManager.canUnseenSwap) transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        else transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (gameManager.seenCardIndex != -1)
                    {
                        if (Owner == gameManager.originalOwner && Position == gameManager.originalPosition) transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        else transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (gameManager.ownSwapCardIndex != -1 && Owner == (owner)gameManager.getCurrentTurnPlayerId())
                    {
                        if (gameManager.Deck[gameManager.ownSwapCardIndex].Equals(card)) transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        else transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (gameManager.othersSwapCardIndex != -1 && Owner != (owner)gameManager.getCurrentTurnPlayerId())
                    {
                        if (gameManager.Deck[gameManager.othersSwapCardIndex].Equals(card) || (Owner == gameManager.originalOwner && Position == gameManager.originalPosition)) transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                        else transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (gameManager.seenCardIndex == -1 && gameManager.ownSwapCardIndex == -1 && gameManager.othersSwapCardIndex == -1) transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
                else
                {
                    transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
            }

            if (Owner == (owner)5)
            {
                if (gameManager.canSkip || gameManager.canPickFromDeckOrStack) transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                else transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            if (Owner == (owner)6)
            {
                if (gameManager.canPickFromDeckOrStack) transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                else transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}
