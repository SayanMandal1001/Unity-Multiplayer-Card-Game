using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankListManager : MonoBehaviour
{
    public int[] rankPlayerId = new int[4];
    public int[] rankScore = new int[4];
    public int[] currentRankScore = new int[4];

    public GameManager gameManager;
    public GameObject NextRoundButton;
    public GameObject NextGameButton;

    private void setRankNameAndScore()
    {
        for(int i = 0; i < rankScore.Length; i++)
        {
            Transform rankObject = transform.GetChild(i);
            rankObject.GetChild(1).gameObject.GetComponent<Text>().text = rankPlayerId[i].ToString();
            rankObject.GetChild(2).gameObject.GetComponent<Text>().text = rankScore[i].ToString() + " (" + currentRankScore[i].ToString() + ")";
        }
    }

    private void reorderPlayerRank()
    {
        for (int i = 0; i < rankPlayerId.Length; i++) rankPlayerId[i] = i+1;
        for (int i = 0; i < rankScore.Length; i++) 
        {
            for (int j = i; j < rankScore.Length; j++) 
            {
                if (rankScore[i] < rankPlayerId[j])
                {
                    (rankScore[i], rankScore[j]) = (rankScore[j], rankScore[i]);
                    (currentRankScore[i], currentRankScore[j]) = (currentRankScore[j], currentRankScore[i]);
                    (rankPlayerId[i], rankPlayerId[j]) = (rankPlayerId[j], rankPlayerId[i]);
                }
            }
        }
    }

    private void Update()
    {
        if (gameManager.isRoundOver)
        {
            if (gameManager.isGameOver == true)
            {
                NextGameButton.SetActive(true);
                NextRoundButton.SetActive(false);
            }
            else
            {
                NextGameButton.SetActive(false);
                NextRoundButton.SetActive(true);
            }

            for (int i = 0; i < rankScore.Length; i++)
            {
                rankScore[i] = gameManager.playerScore[i];
                currentRankScore[i] = gameManager.currentRoundPlayerScore[i];
            }

            reorderPlayerRank();
            setRankNameAndScore();

            gameManager.isRoundOver = false;
        }
    }
}
