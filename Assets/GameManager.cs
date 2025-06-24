using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum suit
{
    None,
    Spades,
    Clubs,
    Hearts,
    Diamonds,
}
public enum value
{
    None,
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
}

public enum owner
{
    None,
    Player1,
    Player2,
    Player3,
    Player4,
    Stack,
    Deck,
    ShowingPlayer1, ShowingPlayer2, ShowingPlayer3, ShowingPlayer4,
}

public enum position
{
    None,
    One,
    Two,
    Three,
    Four,
    PenaltyOne,
    PenaltyTwo,
}

public struct Card : INetworkSerializable, IEquatable<Card>
{
    private suit suit;
    private value value;
    private owner owner;
    private position position;
    private owner whoCanSeeCard;
    private int layerOrder;
    
    public Card(suit s, value v)
    {
        this.suit = s;
        this.value = v;
        this.owner = owner.Deck;
        this.position = position.None;
        this.whoCanSeeCard = owner.None;
        this.layerOrder = 0;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref suit);
        serializer.SerializeValue(ref value);
        serializer.SerializeValue(ref owner);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref whoCanSeeCard);
        serializer.SerializeValue(ref layerOrder);
    }
    public void setOwner(owner owner) { this.owner = owner; }
    public void setPosition(position position) {  this.position = position; }
    public void showCard(owner owner) { this.whoCanSeeCard = owner; }
    public void setLayerOrder(int order) {  this.layerOrder = order; }
    public int getLayerOrder() { return layerOrder;}
    public owner canShowTo() { return this.whoCanSeeCard; }
    public suit getSuit() { return suit; }
    public value getValue() { return value; }
    public owner getOwner() { return owner; }
    public position getPosition() { return position; }
    public bool Equals(Card rhs) { return (this.getSuit() == rhs.getSuit()) && (this.getValue()==rhs.getValue()); }
}

public class GameManager : NetworkBehaviour
{
    [NonSerialized] public string RelayCode;

    public Card[] Deck;
    public int numberOfPlayers = 4;
    public GameObject CardsObject;
    public ulong playerId = (ulong)0;
    public string playerName;
    public string[] playerNames = new string[4];
    public string playerAuthenticationID;
    public string[] playerAuthenticationIDs = new string[4];
    [SerializeField] private ulong currentTurnPlayerId;
    [NonSerialized] private ulong playerIdToStart = 0;
    [NonSerialized] private bool[] playerReady;
    [NonSerialized] public bool haveAllJoined = false;
    [NonSerialized] public bool[] playerJoinedForNextRound = new bool[4];
    [NonSerialized] public bool isStartRoundCalled = false;
    [NonSerialized] public bool sendPlayerNameToHost = false;
    [NonSerialized] public bool syncPlayerName = false;
    [NonSerialized] public bool syncPlayerAuthenticationID = false;

    [NonSerialized] public bool startRounds = false;
    [NonSerialized] public bool startTurn = false;
    [NonSerialized] private bool syncDeckData = false;
    [NonSerialized] public bool canPickFromDeckOrStack = false;
    [NonSerialized] public bool canReplace = false;
    [NonSerialized] public bool canSkip = false;
    [NonSerialized] public bool canStack = false;
    [NonSerialized] public int topStackCardIndex = 0;
    [NonSerialized] private int stackedCardLayerOrder = 0;
    [NonSerialized] private ulong stackOwnerPlayerId = 0;
    [NonSerialized] public bool canGiveOtherACard = false;
    [NonSerialized] public ulong giveCardToPlayerId = 0;
    [NonSerialized] public owner[] otherPlayerToBeGivenCard = new owner[3];
    [NonSerialized] public position[] otherPlayerPositionToBeGivenCard = new position[3];
    [NonSerialized] public int numberOfOtherPlayerToBeGivenCard = 0;
    [NonSerialized] public int numberOfOtherPlayerGivenCard = 0;

    [NonSerialized] public bool canSeeSelfCard = false;
    [NonSerialized] public bool canSeeOthersCard = false;
    [NonSerialized] public bool canSeenSwap = false;
    [NonSerialized] public bool canUnseenSwap = false;
    [NonSerialized] public int seenCardIndex = -1;
    [NonSerialized] public owner originalOwner;
    [NonSerialized] public position originalPosition;
    [NonSerialized] public int othersSwapCardIndex = -1;
    [NonSerialized] public int ownSwapCardIndex = -1;

    [NonSerialized] public ulong kaboCalledByPlayerId = 0;
    [NonSerialized] public bool isRoundOver = false;
    [NonSerialized] public bool isGameOver = false;
    [NonSerialized] public int[] playerScore = new int[4];
    [NonSerialized] public int[] currentRoundPlayerScore = new int[4];

    public Text LobbyCodeText;
    public GameObject[] playerIdText;
    public TextMeshPro[] playerNameText;
    public Text timerText;

    public GameObject StartButton;
    public GameObject ReadyButton;
    public GameObject DoneButton;
    public GameObject SkipButton;
    public GameObject SwapButton;

    public GameObject GameOverPanel;
    public WaitingAreaUIManager waitingAreaUIManager;

    [NonSerialized] public float t;

    public void SetPlayerName(string pName)
    {
        playerName = pName;
    }

    public int playerIdDiff(ulong playerId)
    {
        return (int)playerId - 1;
    }

    public Card[] getDeck()
    {
        Card[] cards = new Card[52];
        suit s = suit.Spades;
        for (int i = 0; i < 4; i++)
        {
            value v = value.Ace;
            for (int j = 0; j < 13; j++)
            {
                cards[13 * i + j] = new Card(s, v);
                v += 1;
            }
            s += 1;
        }
        return cards;
    }

    public void ShuffleDeck(Card[] cards)
    {
        int numberOfShuffles = Random.Range(5, 11);
        while (numberOfShuffles > 0)
        {
            int index = Random.Range(0, cards.Length);
            for (int i = 0; i < cards.Length; i++)
            {
                (cards[index], cards[i]) = (cards[i], cards[index]);
                index = Random.Range(0, cards.Length);
            }
            numberOfShuffles--;
        }
    }

    public void DistributeCards(Card[] cards)
    {
        owner[] players = new owner[numberOfPlayers];
        owner player = owner.Player1;
        position[] playerCardNumber = new position[numberOfPlayers];
        for (int i = 0; i < playerCardNumber.Length; i++) { playerCardNumber[i] = position.One; }
        for (int i = 0; i < players.Length; i++) { players[i] = player; player += 1; }
        for (int i = 0; i < numberOfPlayers * 4; i++)
        {
            cards[i].setOwner(players[i % 4]);
            cards[i].setPosition(playerCardNumber[i % 4]);
            //if((playerId == (ulong) i%4) && (playerCardNumber[i%4]>position.Two)) cards[i].showCard();
            playerCardNumber[i % 4] += 1;
        }
    }

    [ClientRpc]
    public void StartWhenAllConnectedClientRpc()
    {
        SetPlayerAuthenticationIDRpc(playerId, playerAuthenticationID);

        waitingAreaUIManager.Timer = waitingAreaUIManager.maxTimer;
        waitingAreaUIManager.StartingTimer.SetActive(true);
        waitingAreaUIManager.StartButton.SetActive(false);
        waitingAreaUIManager.BackButton.SetActive(false);
        waitingAreaUIManager.WaitingText.SetActive(false);
    }


    // Start is called before the first frame update
    void Awake()
    {
        for(int i=0; i<playerNames.Length; i++) { playerNames[i] = ""; }
        for(int i=0; i<playerAuthenticationIDs.Length; i++) { playerAuthenticationIDs[i] = ""; }

        LobbyCodeText.text = RelayCode;
        StartNewGame();
        //StartRound();
    }

    public void StartNewGame()
    {
        for (int i = 0; i < playerScore.Length; i++) { playerScore[i] = 0; }
        isGameOver = false;
    }

    public void StartRound()
    {
        if (!isStartRoundCalled)
        {
            setPlayerJoinedForNextRoundRpc(playerId);
        }   

        ReadyButton.SetActive(false);
        DoneButton.SetActive(false);
        SkipButton.SetActive(false);
        SwapButton.SetActive(false);
        GameOverPanel.SetActive(false);
        isRoundOver = false;

        kaboCalledByPlayerId = 0;
        playerIdToStart += 1;
        if (playerIdToStart == 5) playerIdToStart = 1;
        Deck = getDeck();
        playerReady = new bool[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++) { playerReady[i] = false; }
        startRounds = false;
    }

    [Rpc(SendTo.Server)]
    public void setPlayerJoinedForNextRoundRpc(ulong pId)
    {
        playerJoinedForNextRound[(int)pId - 1] = true;
    }

    public void StartGame()
    {
        ShuffleDeck(Deck);
        DistributeCards(Deck);
        showInitialCards();
        showReadyButtonClientRpc();
        StartButton.SetActive(false);
        syncDeckData = true;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Local Client Id: " + NetworkManager.Singleton.LocalClientId);
        playerId = NetworkManager.Singleton.LocalClientId + 1;
        if(playerId > 4)
        {
            ManageHigherPlayerIdRpc(playerId);
        }
        sendPlayerNameToHost = true;
    }

    [Rpc(SendTo.Server)]
    public void ManageHigherPlayerIdRpc(ulong pId)
    {
        for (int i = 0; i < 4; i++) 
        {
            if (playerNames[i] == "")
            {
                updatePlayerIdClientRpc(pId,(ulong)i+1); break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void OnClientLeaveRpc(ulong pID)
    {
        waitingAreaUIManager.StartButton.SetActive(false);
        for(int i=(int)pID+1; i<=4; i++)
        {
            updatePlayerIdClientRpc((ulong)i,(ulong)i-1);
            playerNames[i-2] = playerNames[i-1];
        }
        playerNames[3] = "";
        syncPlayerName = true;
    }

    [ClientRpc]
    public void updatePlayerIdClientRpc(ulong oldPId, ulong newPId)
    {
        if(playerId == oldPId) playerId = newPId;
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerNamesRpc(ulong pID, string name)
    {
        playerNames[(int)pID - 1] = name;
        syncPlayerName = true;
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerAuthenticationIDRpc(ulong pID, string aID)
    {
        playerAuthenticationIDs[(int)pID - 1] = aID;
        syncPlayerAuthenticationID = true;
    }

    [ClientRpc]
    public void SyncPlayerNamesClientRpc(int index, string name)
    {
        playerNames[index] = name;
    }

    [ClientRpc]
    public void SyncPlayerAuthenticationIDClientRpc(int index, string aID)
    {
        playerAuthenticationIDs[index] = aID;
    }


    public void SetCorrectOwnerToCardSlots()
    {
        GameObject[] cardSlots = GameObject.FindGameObjectsWithTag("CardSlot");
        for (int i = 0; i < cardSlots.Length; i++)
        {
            cardSlots[i].GetComponent<CardSlotManager>().setCorrectOwner();
        }
    }

    public void setPlayerIdText()
    {
        GameObject[] playerIdPositions = GameObject.FindGameObjectsWithTag("PlayerIdPosition");
        for (int i = 0; i < playerIdPositions.Length; i++)
        {
            owner player = getCorrectedSlotOwner((owner)playerIdPositions[i].gameObject.GetComponent<PlayerIdTextManager>().getPlayerId(), playerId);
            for (int j = 0; j < playerIdText.Length; j++)
            {
                if ((owner)playerIdText[j].gameObject.GetComponent<PlayerIdTextManager>().getPlayerId() == player)
                {
                    playerIdText[j].transform.position = playerIdPositions[i].transform.position;
                    playerIdText[j].transform.eulerAngles = playerIdPositions[i].transform.eulerAngles;
                    break;
                }
            }
        }

        for (int i = 0; i < playerNameText.Length; i++) 
        {
            owner player = getCorrectedSlotOwner((owner)i + 1, playerId);
            playerNameText[i].text = playerNames[(int)player-1];
        }

    }

    //[Rpc(SendTo.ClientsAndHost)]
    public void showInitialCards()
    {
        for (int i = 0; i < Deck.Length; i++)
        {
            Debug.Log("Searching...");
            if (Deck[i].getOwner() <= owner.Player4)
            {
                Debug.Log("Got owner:" + Deck[i].getOwner());
                if (Deck[i].getPosition() == position.Three || Deck[i].getPosition() == position.Four)
                {
                    Deck[i].showCard(Deck[i].getOwner());
                    Debug.Log(Deck[i].getSuit() + " " + Deck[i].getValue() + " " + Deck[i].getOwner() + " " + Deck[i].getPosition() + " " + Deck[i].canShowTo());
                }
            }
        }
    }

    public void printDeck()
    {
        for (int i = 0; i < Deck.Length; i++)
        {
            Debug.Log(Deck[i].getSuit() + " " + Deck[i].getValue() + " " + Deck[i].getOwner() + " " + Deck[i].getPosition() + " " + Deck[i].canShowTo());
        }
    }

    [ClientRpc]
    public void showReadyButtonClientRpc()
    {
        ReadyButton.SetActive(true);
    }

    public void ready()
    {
        setReadyRpc(playerId);
        ReadyButton.SetActive(false);
    }

    [Rpc(SendTo.Server)]
    public void setReadyRpc(ulong pID)
    {
        playerReady[(int)pID - 1] = true;
        for (int i = 0; i < Deck.Length; i++)
        {
            Debug.Log("Searching...");
            if (Deck[i].getOwner() == (owner)pID)
            {
                Debug.Log("Got owner:" + Deck[i].getOwner());
                if (Deck[i].getPosition() > position.Two)
                {
                    Deck[i].showCard(owner.None);
                    Debug.Log(Deck[i].getSuit() + " " + Deck[i].getValue() + " " + Deck[i].getOwner() + " " + Deck[i].getPosition() + " " + Deck[i].canShowTo());
                }
            }
        }
        syncDeckData = true;
    }

    [ClientRpc]
    public void startTurnClientRpc(ulong pID)
    {
        startTurn = false;
        currentTurnPlayerId = pID;
        if (playerId == pID) { canPickFromDeckOrStack = true; }
    }

    [ClientRpc]
    public void syncDeckDataClientRpc(Card[] cards)
    {
        Debug.Log("Running sync on client ID: " + playerId);
        for (int i = 0; i < Deck.Length; i++)
        {
            Deck[i] = cards[i];
        }
        Debug.Log("Data succesfully synced with player ID :" + playerId);
    }

    [ClientRpc]
    public void syncSeenCardIndexClientRpc(int seenCardI)
    {
        seenCardIndex = seenCardI;
    }

    [ClientRpc]
    public void syncOwnSwapCardIndexClientRpc(int ownSwapCardI)
    {
        ownSwapCardIndex = ownSwapCardI;
    }

    [ClientRpc]
    public void syncOthersSwapCardIndexClientRpc(int othersSwapCardI)
    {
        othersSwapCardIndex = othersSwapCardI;
    }

    [ClientRpc]
    public void syncSeenCardDataClientRpc(owner owner, position position)
    {
        originalOwner = owner;
        originalPosition = position;
    }

    [ClientRpc]
    public void startRoundsClientRpc() { startRounds = true; }

    public ulong getCurrentTurnPlayerId() { return currentTurnPlayerId; }

    [Rpc(SendTo.Server)]
    public void getTopDeckCardRpc(ulong pID)
    {
        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() == owner.Deck)
            {
                Deck[i].setOwner((owner)pID + 6);
                Deck[i].setPosition(position.None);
                Deck[i].showCard((owner)pID);
                break;
            }
        }
        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void getTopStackCardRpc(ulong pID)
    {
        Deck[topStackCardIndex].setOwner((owner)pID + 6);
        Deck[topStackCardIndex].setPosition(position.None);
        Deck[topStackCardIndex].showCard(owner.Stack);

        syncDeckData = true;
    }

    public int getCardIndex(owner owner, position position)
    {
        int index = -1;
        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() == owner && Deck[i].getPosition() == position)
            {
                index = i;
                break;
            }
        }

        return index;
    }

    public owner getCorrectedSlotOwner(owner owner, ulong pID)
    {
        owner correctedOwner = owner;
        if (owner <= owner.Player4)
        {
            correctedOwner = owner + playerIdDiff(pID);
            if (correctedOwner > owner.Player4) correctedOwner -= 4;
        } else if (owner >= owner.ShowingPlayer1)
        {
            correctedOwner = owner - 4 + playerIdDiff(pID);
            if (correctedOwner < owner.ShowingPlayer1) correctedOwner += 4;
        }
        return correctedOwner;
    }

    [Rpc(SendTo.Server)]
    public void replaceCardRpc(owner owner, position position, ulong pID)
    {
        int index = getCardIndex(owner, position);
        Deck[index].setOwner(owner.Stack);
        Deck[index].setPosition(position.None);
        Deck[index].showCard(owner.Stack);
        Deck[index].setLayerOrder(stackedCardLayerOrder);
        stackedCardLayerOrder += 1;
        topStackCardIndex = index;

        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() > (owner)6)
            {
                Deck[i].setOwner((owner)pID);
                Deck[i].setPosition(position);
                Deck[i].showCard(owner.None);
                break;
            }
        }
        startTimerClientRpc();
        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void skipCardRpc()
    {
        int skipCardIndex = 0;
        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() > (owner)6)
            {
                Deck[i].setOwner(owner.Stack);
                Deck[i].setPosition(position.None);
                Deck[i].showCard(owner.Stack);
                Deck[i].setLayerOrder(stackedCardLayerOrder);
                stackedCardLayerOrder += 1;
                skipCardIndex = i;
                topStackCardIndex = i;
                break;
            }
        }
        checkForSpecialSkipCardRpc(Deck[skipCardIndex].getValue());
        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void checkForSpecialSkipCardRpc(value value)
    {
        if (value == value.Seven || value == value.Eight)
        {
            startCanSeeSelfCardClientRpc(currentTurnPlayerId);
        }
        else if (value == value.Nine || value == value.Ten)
        {
            startCanSeeOthersCardClientRpc(currentTurnPlayerId);
        }
        else if (value == value.Jack)
        {
            startCanUnseenSwapClientRpc(currentTurnPlayerId);
        }
        else if (value == value.Queen)
        {
            startCanSeenSwapClientRpc(currentTurnPlayerId);
        }
        else
        {
            startTimerClientRpc();
        }
    }

    [ClientRpc]
    public void startCanSeeSelfCardClientRpc(ulong pID)
    {
        if (pID == playerId)
        {
            canSeeSelfCard = true;
            DoneButton.SetActive(true);
        }
    }

    [ClientRpc]
    public void startCanSeeOthersCardClientRpc(ulong pID)
    {
        if (pID == playerId)
        {
            canSeeOthersCard = true;
            DoneButton.SetActive(true);
        }
    }

    [ClientRpc]
    public void startCanSeenSwapClientRpc(ulong pID)
    {
        if (pID == playerId)
        {
            canSeenSwap = true;
            canSeeOthersCard = true;
            SkipButton.SetActive(true);
            SwapButton.SetActive(true);
        }
    }

    [ClientRpc]
    public void startCanUnseenSwapClientRpc(ulong pID)
    {
        if (pID == playerId)
        {
            canUnseenSwap = true;
            SkipButton.SetActive(true);
            SwapButton.SetActive(true);
        }
    }

    [Rpc(SendTo.Server)]
    public void seeCardRpc(owner showTo, owner owner, position position)
    {
        int index = getCardIndex(owner, position);
        seenCardIndex = index;
        syncSeenCardIndexClientRpc(seenCardIndex);
        originalOwner = Deck[index].getOwner();
        originalPosition = Deck[index].getPosition();
        syncSeenCardDataClientRpc(originalOwner, originalPosition);
        Deck[index].setOwner(showTo + 6);
        Deck[index].setPosition(position.None);
        Deck[index].showCard(showTo);
        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void hideCardRpc()
    {
        if (seenCardIndex != -1)
        {
            Deck[seenCardIndex].setOwner(originalOwner);
            Deck[seenCardIndex].setPosition(originalPosition);
            Deck[seenCardIndex].showCard(owner.None);
            seenCardIndex = -1;
            originalOwner = owner.None;
            originalPosition = position.None;
            syncSeenCardIndexClientRpc(seenCardIndex);
            syncSeenCardDataClientRpc(originalOwner, originalPosition);
            syncDeckData = true;
        }
    }

    [Rpc(SendTo.Server)]
    public void setOthersSwapCardRpc(owner owner, position position)
    {
        othersSwapCardIndex = getCardIndex(owner, position);
        syncOthersSwapCardIndexClientRpc(othersSwapCardIndex);
    }

    [Rpc(SendTo.Server)]
    public void setOwnSwapCardRpc(owner owner, position position)
    {
        ownSwapCardIndex = getCardIndex(owner, position);
        syncOwnSwapCardIndexClientRpc(ownSwapCardIndex);
    }

    public void swapButton()
    {
        swapCardsRpc();
    }

    [ClientRpc]
    public void hideSwapAndSkipButtonsClientRpc(ulong pID)
    {
        if (playerId == pID)
        {
            canSeenSwap = false;
            canUnseenSwap = false;
            SkipButton.SetActive(false);
            SwapButton.SetActive(false);
        }
    }

    [Rpc(SendTo.Server)]
    public void swapCardsRpc()  //Assigned to swap button
    {
        if (ownSwapCardIndex != -1 && othersSwapCardIndex != -1)
        {
            hideCardRpc();
            owner tempOwner = Deck[othersSwapCardIndex].getOwner();
            position tempPosition = Deck[othersSwapCardIndex].getPosition();
            Deck[othersSwapCardIndex].setOwner(Deck[ownSwapCardIndex].getOwner());
            Deck[othersSwapCardIndex].setPosition(Deck[ownSwapCardIndex].getPosition());
            Deck[ownSwapCardIndex].setOwner(tempOwner);
            Deck[ownSwapCardIndex].setPosition(tempPosition);
            ownSwapCardIndex = -1;
            othersSwapCardIndex = -1;
            syncOwnSwapCardIndexClientRpc(ownSwapCardIndex);
            syncOthersSwapCardIndexClientRpc(othersSwapCardIndex);
            hideSwapAndSkipButtonsClientRpc(currentTurnPlayerId);
            startTimerClientRpc();
            syncDeckData = true;
        }

    }

    public void skipSwapButton()
    {
        skipSwapingCardsRpc();
    }

    [Rpc(SendTo.Server)]
    public void skipSwapingCardsRpc()
    {
        othersSwapCardIndex = -1;
        ownSwapCardIndex = -1;
        syncOwnSwapCardIndexClientRpc(ownSwapCardIndex);
        syncOthersSwapCardIndexClientRpc(othersSwapCardIndex);
        hideCardRpc();
        hideSwapAndSkipButtonsClientRpc(currentTurnPlayerId);
        startTimerClientRpc();
    }

    public void doneButton()
    {
        doneWithSpecialCardPowersRpc();
        DoneButton.SetActive(false);
    }

    [Rpc(SendTo.Server)]
    public void doneWithSpecialCardPowersRpc()
    {
        hideCardRpc();
        startTimerClientRpc();
    }

    [ClientRpc]
    public void startTimerClientRpc() { t = 3; }

    [ClientRpc]
    public void setCanGiveOtherACardClientRpc(ulong pID)
    {
        if (playerId == pID)
        {
            if(hasCard(pID)) canGiveOtherACard = true;
        }
    }

    public bool hasCard(ulong pID)
    {
        bool found = false;
        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() == (owner)pID)
            {
                found = true;
                break;
            }
        }
        return found;
    }

    [Rpc(SendTo.Server)]
    public void stackCardRpc(owner owner, position position, ulong pID)
    {
        int toBeStackedIndex = getCardIndex(owner, position);
        if (Deck[topStackCardIndex].getOwner() == owner.Stack)
        {
            if (Deck[topStackCardIndex].getValue() == Deck[toBeStackedIndex].getValue())
            {
                if (stackOwnerPlayerId == 0)
                {
                    stackOwnerPlayerId = pID;
                    if(owner != (owner)pID)
                    {
                        otherPlayerToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = owner;
                        otherPlayerPositionToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = position;
                        numberOfOtherPlayerToBeGivenCard += 1;
                    }
                }
                else if (stackOwnerPlayerId != pID)
                {
                    otherPlayerToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = owner;
                    otherPlayerPositionToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = position;
                    numberOfOtherPlayerToBeGivenCard += 1;
                }else if (stackOwnerPlayerId == pID && owner != (owner)pID)
                {
                    otherPlayerToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = owner;
                    otherPlayerPositionToBeGivenCard[numberOfOtherPlayerToBeGivenCard] = position;
                    numberOfOtherPlayerToBeGivenCard += 1;
                }
                Deck[toBeStackedIndex].setOwner(owner.Stack);
                Deck[toBeStackedIndex].setPosition(position.None);
                Deck[toBeStackedIndex].showCard(owner.Stack);
                Deck[toBeStackedIndex].setLayerOrder(stackedCardLayerOrder);
                stackedCardLayerOrder += 1;
                topStackCardIndex = toBeStackedIndex;
            }
            else
            {
                Deck[toBeStackedIndex].showCard(owner.Stack);
                givePenaltyCardRpc(pID);
            }
        }

        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void giveOtherACardRpc(owner owner, position position)
    {
        int cardToBeGivenIndex = getCardIndex(owner, position);
        Deck[cardToBeGivenIndex].setOwner(otherPlayerToBeGivenCard[numberOfOtherPlayerGivenCard]);
        Deck[cardToBeGivenIndex].setPosition(otherPlayerPositionToBeGivenCard[numberOfOtherPlayerGivenCard]);
        Deck[cardToBeGivenIndex].showCard(owner.None);
        numberOfOtherPlayerGivenCard += 1;
        if (numberOfOtherPlayerGivenCard == numberOfOtherPlayerToBeGivenCard || !hasCard(stackOwnerPlayerId))
        {
            resetCanGiveOtherACardClientRpc(stackOwnerPlayerId);
            otherPlayerToBeGivenCard = new owner[3];
            otherPlayerPositionToBeGivenCard = new position[3];
            numberOfOtherPlayerToBeGivenCard = 0;
            numberOfOtherPlayerGivenCard = 0;
            stopTurnRpc();
        }
        syncDeckData = true;
    }

    [ClientRpc]
    public void resetCanGiveOtherACardClientRpc(ulong pID)
    {
        if(playerId == pID) canGiveOtherACard = false;
    }

    public int numberOfPenalties(ulong pID)
    {
        int count = 0;
        for (int i = 0; i < Deck.Length; i++) 
        {
            if (Deck[i].getOwner() == (owner)pID && Deck[i].getPosition() >= position.PenaltyOne) count += 1;
        }
        return count;
    }


    [Rpc(SendTo.Server)]
    public void givePenaltyCardRpc(ulong pID)
    {
        int penaltyIndex = numberOfPenalties(pID);
        if (penaltyIndex == 2) return;
        bool found=false;
        int topDeckCardIndex = -1;
        for (int i = 0; i < Deck.Length; i++)
        {
            if (Deck[i].getOwner() == owner.Deck)
            {
                found = true;
                topDeckCardIndex = i;
                break;
            }
        }
        if (!found) 
        {
            repopulateDeckRpc();
            for (int i = 0; i < Deck.Length; i++)
            {
                if (Deck[i].getOwner() == owner.Deck)
                {
                    topDeckCardIndex = i;
                    break;
                }
            }
        }
        Deck[topDeckCardIndex].setOwner((owner)pID);
        Deck[topDeckCardIndex].setPosition(position.PenaltyOne + penaltyIndex);
        Deck[topDeckCardIndex].showCard(owner.None);

        syncDeckData = true;
    }

    [Rpc(SendTo.Server)]
    public void hidePlayerCardsRpc()
    {
        for (int i = 0; i < Deck.Length; i++) 
        {
            if (Deck[i].getOwner() <= owner.Player4)
            {
                Deck[i].showCard(owner.None);
            }
        }
        syncDeckData = true;
    }


    [Rpc(SendTo.Server)]
    public void stopTurnRpc()
    {
        if(!hasCard(currentTurnPlayerId) && kaboCalledByPlayerId == 0)
        {
            kaboCalledRpc(currentTurnPlayerId);
        }
        else
        {
            hidePlayerCardsRpc();
            stackOwnerPlayerId = 0;
            currentTurnPlayerId += 1;
            if (currentTurnPlayerId == 5) { currentTurnPlayerId = 1; }
            if (currentTurnPlayerId != kaboCalledByPlayerId)
            {
                startTurnClientRpc(currentTurnPlayerId);
            }
            else
            {
                //showResult
                calculateScoresRpc();
            }
        }
        
    }

    [Rpc(SendTo.Server)]
    public void calculateScoresRpc()
    {
        for(int i=0; i<currentRoundPlayerScore.Length; i++) { currentRoundPlayerScore[i] = 0; }
        for(int i=0; i<Deck.Length; i++)
        {
            if (Deck[i].getOwner() <= owner.Player4)
            {
                Deck[i].showCard(owner.Stack);
                value value = Deck[i].getValue();
                if (value <= value.Ten)
                {
                    currentRoundPlayerScore[(int)Deck[i].getOwner() - 1] += (int)value;
                }else
                {
                    if(value == value.King && Deck[i].getSuit() >= suit.Hearts)
                    {
                        currentRoundPlayerScore[(int)Deck[i].getOwner() - 1] += -1;
                    }
                    else
                    {
                        currentRoundPlayerScore[(int)Deck[i].getOwner() - 1] += 10;
                    }
                }
            }
        }

        bool hasKaboCalledPlayerWon = true;

        for(int i=0; i< currentRoundPlayerScore.Length; i++)
        {
            if (hasCard(kaboCalledByPlayerId) && (i + 1) != (int)kaboCalledByPlayerId) 
            {
                if (currentRoundPlayerScore[(int)kaboCalledByPlayerId - 1] >= currentRoundPlayerScore[i]) { hasKaboCalledPlayerWon = false; break; }
            }
        }
        
        if (!hasKaboCalledPlayerWon)
        {
            int maxScore = 0;
            for(int i=0; i< currentRoundPlayerScore.Length;i++)
            {
                if (currentRoundPlayerScore[i] > maxScore && (i + 1) != (int)kaboCalledByPlayerId) { maxScore = currentRoundPlayerScore[i];}
            }
            currentRoundPlayerScore[(int)kaboCalledByPlayerId - 1] = maxScore;
        }

        for(int i=0; i<playerScore.Length; i++)
        {
            if (currentRoundPlayerScore[i]<0 || (!hasKaboCalledPlayerWon && (i + 1) != (int)kaboCalledByPlayerId)) currentRoundPlayerScore[i] = 0;
            playerScore[i] += currentRoundPlayerScore[i];
            if (playerScore[i]>=100) isGameOver=true;
        }
        
        showResultClientRpc(playerScore, currentRoundPlayerScore, isGameOver);
        syncDeckData = true;
    }

    [ClientRpc]
    public void showResultClientRpc(int[] score, int[] currentScore, bool gameOver)
    {
        for (int i = 0; i < score.Length; i++)
        {
            playerScore[i] = score[i];
            currentRoundPlayerScore[i] = currentScore[i];
        }
        for (int i=0; i< playerJoinedForNextRound.Length; i++) playerJoinedForNextRound[i] = false;
        GameOverPanel.SetActive(true);
        isGameOver = gameOver;
        isRoundOver = true;
    }

    [Rpc(SendTo.Server)]
    public void repopulateDeckRpc()
    {
        for(int i = 0; i<Deck.Length; i++)
        {
            if (Deck[i].getOwner() == owner.Stack)
            {
                Deck[i].setOwner(owner.Deck);
                Deck[i].setPosition(position.None);
                Deck[i].showCard(owner.None);
            }
        }
        ShuffleDeck(Deck);
    }

    [Rpc(SendTo.Server)]
    public void kaboCalledRpc(ulong pID)
    {
        Debug.Log("Kabo is called by " + pID);
        stopTurnRpc();
        kaboCalledByPlayerId = pID;
        kaboCalledClientRpc(pID);
    }

    [ClientRpc]
    public void kaboCalledClientRpc(ulong pID)
    {
        kaboCalledByPlayerId = pID;
        if (playerId == pID) canPickFromDeckOrStack = false;
    }

    private void FixedUpdate()
    {
        
        if (haveAllJoined)
        {
            
            if (isStartRoundCalled)
            {
                StartRound();
                if (IsServer && playerId == 1)
                {
                    StartButton.SetActive(true);
                }
                isStartRoundCalled = false;
            }
            else
            {
                if(IsServer && playerId == 1)
                {
                    bool canStart = true;
                    for(int i = 0; i< playerJoinedForNextRound.Length; i++)
                    {
                        if (playerJoinedForNextRound[i] == false) {canStart = false; break;}
                    }
                    if (canStart)
                    {
                        StartButton.SetActive(true);
                        for (int i = 0; i < playerJoinedForNextRound.Length; i++) playerJoinedForNextRound[i] = false;
                    }
                }
            }

            if (t > 0) { t -= Time.deltaTime; canStack = true; }
            else
            {
                if (canStack)
                {
                    canStack = false;
                    if (IsServer && playerId == 1)
                    {
                        if (numberOfOtherPlayerToBeGivenCard == 0) stopTurnRpc();
                        else setCanGiveOtherACardClientRpc(stackOwnerPlayerId);
                    }
                }
            }
            timerText.text = ((int)t).ToString();
            if (currentTurnPlayerId > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    playerIdText[i].transform.GetChild(1).gameObject.SetActive(false);
                }

                playerIdText[currentTurnPlayerId - 1].transform.GetChild(1).gameObject.SetActive(true);
            }

            if (IsServer && playerId == 1)
            {
                if (syncDeckData)
                {
                    Debug.Log("Syncing Data...");
                    syncDeckDataClientRpc(Deck);
                    syncDeckData = false;
                }

                if (!startRounds)
                {
                    bool canStart = true;
                    for (int i = 0; i < numberOfPlayers; i++)
                        if (playerReady[i] == false) canStart = false;
                    if (canStart)
                    {
                        startRounds = true;
                        startRoundsClientRpc();
                        currentTurnPlayerId = playerIdToStart;
                        startTurnClientRpc(currentTurnPlayerId);
                    }
                }
            }
        }
        else 
        {
            if (sendPlayerNameToHost && playerId<=4)
            {
                SetPlayerNamesRpc(playerId, playerName);
                sendPlayerNameToHost = false;
            }
            if (IsServer && playerId == 1)
            {
                if (syncPlayerName)
                {
                    for (int i = 0; i < numberOfPlayers; i++)
                    {
                        SyncPlayerNamesClientRpc(i, playerNames[i]);
                    }
                    syncPlayerName = false;
                }
                
            }
        }
        if(syncPlayerAuthenticationID && IsServer && playerId == 1)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                SyncPlayerAuthenticationIDClientRpc(i, playerAuthenticationIDs[i]);
            }
            syncPlayerAuthenticationID = false;
        }
    }
}
