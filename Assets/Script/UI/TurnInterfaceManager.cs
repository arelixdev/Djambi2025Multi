using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInterfaceManager : MonoBehaviour
{
    public static TurnInterfaceManager instance;
    [SerializeField] private List<TurnElement> turnElements;

    [SerializeField] private List<TurnElement> turnElementsNew;

    private void Awake()
    {
        instance = this;
    }

    public List<TurnElement> GetTurnElements()
    {
        return turnElements;
    }

    public void SetTurn(int teamTurn)
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            if (i == teamTurn)
            {
                turnElements[i].ActiveTurn();
            }
            else
            {
                turnElements[i].DesactiveTurn();
            }
        }
    }

    public void SetYourColor(int teamColor)
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            if (i == teamColor)
            {
                turnElements[i].SetYourColor(true);
            }
            else
            {
                turnElements[i].SetYourColor(false);
            }
        }
    }

    public void ActivateChef(int teamTurn)
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            if (i == teamTurn)
            {
                turnElements[i].SetChef(true);
            }
            else
            {
                turnElements[i].SetChef(false);
            }
        }
    }

    public void DesactivateChef(int teamTurn)
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            if (i == teamTurn)
            {
                turnElements[i].SetChef(false);
            }
        }
    }

    public void SetDead(int teamTurn)
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            if (i == teamTurn)
            {
                turnElements[i].SetDead(true);
            }
        }
    }

    internal void RestartGame()
    {
        for (int i = 0; i < turnElements.Count; i++)
        {
            turnElements[i].SetChef(false);
            turnElements[i].SetDead(false);
        }

    }
}
