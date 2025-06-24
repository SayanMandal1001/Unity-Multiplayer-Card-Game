using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaboButtonClick : MonoBehaviour
{
    public GameManager gameManager;
    private bool valid = false;

    private void Awake()
    {
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void OnMouseDown()
    {
        if (valid)
        {
            if (gameManager.playerId == gameManager.getCurrentTurnPlayerId() && gameManager.startRounds && !gameManager.startTurn && gameManager.kaboCalledByPlayerId == 0)
                gameManager.kaboCalledRpc(gameManager.playerId);
            valid = false;
        }
    }
    private void Update()
    {
        if (gameManager.playerId == gameManager.getCurrentTurnPlayerId() && gameManager.startRounds && !gameManager.startTurn && gameManager.kaboCalledByPlayerId == 0)
        {
            valid = true;
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            valid = false;
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
