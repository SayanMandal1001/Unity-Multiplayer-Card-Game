using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public GameObject Board;
    public GameObject RelayUI;
    public GameObject WaitingAreaUI;
    public GameObject MainGameArea;
    public GameObject MainGameAreaUI;

    public GameManager gameManager;

    public bool isGameScene;

    // Start is called before the first frame update
    void Start()
    {
        //ToTitleScreen();
        if(isGameScene)
        {
            Show(Board);
            Show(RelayUI);
            Hide(WaitingAreaUI);
            Hide(MainGameArea);
            Hide(MainGameAreaUI);
        }
    }

    public void ToTitleScreen()
    {
        /*Show(TitleScreen);
        Show(TitleScreenUI);
        Hide(Board);
        Hide(RelayUI);
        Hide(WaitingAreaUI);
        Hide(MainGameArea);
        Hide(MainGameAreaUI);*/
        SceneManager.LoadSceneAsync("TitleScreen");
    }

    public void ToRelayScreen()
    {
        //Hide(TitleScreen);
        //Hide(TitleScreenUI);
        SceneManager.LoadSceneAsync("Game");
        
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
        if (gameManager.playerId == 1) gameManager.generalText.text = "Press START to deal out the cards";
        if (gameManager.playerId != 1) gameManager.generalText.text = "Waiting for the host to deal cards";
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
