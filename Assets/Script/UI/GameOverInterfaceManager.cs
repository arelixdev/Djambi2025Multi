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

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameOverInterface.SetActive(false);
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

    public void OnExitButton()
    {
        Application.Quit();
    }
}
