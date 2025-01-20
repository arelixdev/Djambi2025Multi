using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class GameOverInterfaceManager : MonoBehaviour
{
    public static GameOverInterfaceManager instance;
    public GameObject gameOverInterface;

    public TextMeshProUGUI gameOverText;
    public Transform rematchIndicator;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameOverInterface.SetActive(false);
        ResetGameOverPanel();
    }

    public void ResetGameOverPanel()
    {
        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);
    }


    public void ShowGameOver(int winner)
    {
        gameOverInterface.SetActive(true);
        if (winner == 0)
        {
            gameOverText.text = "Red Team Wins!";
        }
        else if (winner == 1)
        {
            gameOverText.text = "Blue Team Wins!";
        }
        else if (winner == 2)
        {
            gameOverText.text = "Green Team Wins!";
        }
        else if (winner == 3)
        {
            gameOverText.text = "Yellow Team Wins!";
        }
    }

    public void OnRestartButton()
    {
        gameOverInterface.SetActive(false);
        
        DjambiBoard.Instance.RestartGame();
    }

    public void OnMenuButton()
    {
        //DjambiBoard.Instance.BackMenu();
    }


    public void ActivateRematch(int value)
    {
        rematchIndicator.transform.GetChild(value).gameObject.SetActive(true);
    }
}
