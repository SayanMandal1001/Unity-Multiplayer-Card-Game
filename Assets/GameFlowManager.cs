using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public GameObject TitleScreen;
    public GameObject TitleScreenUI;
    public GameObject Board;
    public GameObject RelayUI;
    public GameObject WaitingAreaUI;
    public GameObject MainGameArea;
    public GameObject MainGameAreaUI;

    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        ToTitleScreen();
    }

    public void ToTitleScreen()
    {
        Show(TitleScreen);
        Show(TitleScreenUI);
        Hide(Board);
        Hide(RelayUI);
        Hide(WaitingAreaUI);
        Hide(MainGameArea);
        Hide(MainGameAreaUI);
    }

    public void ToRelayScreen()
    {
        Hide(TitleScreen);
        Hide(TitleScreenUI);
        Show(Board);
        Show(RelayUI);
        Hide(WaitingAreaUI);
    }

    public void ToWaitingArea()
    {
        Hide(RelayUI);
        Show(WaitingAreaUI);
    }

    public void ToMainGameArea()
    {
        Hide(WaitingAreaUI);
        Show(MainGameArea);
        Show(MainGameAreaUI);
        gameManager.setPlayerIdText();
        gameManager.SetCorrectOwnerToCardSlots();
    }

    private void Show(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    private void Hide(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
