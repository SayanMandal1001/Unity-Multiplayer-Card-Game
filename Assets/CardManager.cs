using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public Card card;
    private GameManager gameManager;
    public GameObject[] cardSlot;

    public suit Suit;
    public value Value;

    public bool showCard = false;

    public float positionXRandomness;
    public float positionYRandomness;
    public float rotationRandomness;

    public void findCard()
    {
        if (gameManager.haveAllJoined && !gameManager.isStartRoundCalled)
        {
            for (int i = 0; i < gameManager.Deck.Length; i++)
            {
                if (gameManager.Deck[i].Equals(card))
                {
                    card = gameManager.Deck[i];
                    break;
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        card = new Card(Suit, Value);
        //Debug.Log(card.getSuit() + " " + card.getValue() + " " + card.getOwner() + " " + card.getPosition());

        cardSlot = GameObject.FindGameObjectsWithTag("CardSlot");
        positionXRandomness = Random.Range(-0.5f, 0.5f);
        positionYRandomness = Random.Range(-0.5f, 0.5f);
        rotationRandomness = Random.Range(-180f, 180f);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager == null) gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (gameManager != null && gameManager.haveAllJoined)
        {
            findCard();
            if (card.getOwner() != owner.None)
            {
                for (int i = 0; i < cardSlot.Length; i++)
                {
                    CardSlotManager slotManager = cardSlot[i].GetComponent<CardSlotManager>();
                    if (card.getOwner() == owner.Stack && slotManager.getOwner() == owner.Stack)
                    {
                        Vector3 position = cardSlot[i].transform.position;
                        float angle = cardSlot[i].transform.eulerAngles.z;
                        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(position.x + positionXRandomness, position.y + positionYRandomness, position.z), 0.1f);
                        this.transform.eulerAngles = new Vector3(0f, 0f, angle + rotationRandomness);
                    }
                    else if ((slotManager.getOwner() == card.getOwner()) && (slotManager.getPosition() == card.getPosition()))
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, cardSlot[i].transform.position, 0.1f);
                        this.transform.eulerAngles = new Vector3(0f, 0f, cardSlot[i].transform.eulerAngles.z);
                    }
                }
            }

            if (card.canShowTo() == (owner)(int)gameManager.playerId || card.canShowTo() == owner.Stack) showCard = true;
            else showCard = false;

            if (showCard)
            {
                transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sortingOrder = card.getLayerOrder();
        }
    }
}
