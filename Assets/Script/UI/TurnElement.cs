using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

public class TurnElement : MonoBehaviour
{
    [SerializeField] private Color activeColor;
    [SerializeField] private Color desactiveColor;
    [SerializeField] private GameObject chefElement;
    [SerializeField] private GameObject deadElement;

    void Start()
    {
        chefElement.SetActive(false);
        deadElement.SetActive(false);
    }

    public void ActiveTurn()
    {
        transform.localScale = new Vector3(1, 1, 1);
        GetComponent<Image>().color = activeColor;
    }

    public void DesactiveTurn()
    {
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        GetComponent<Image>().color = desactiveColor;
    }

    public void SetChef(bool isChef)
    {
        chefElement.SetActive(isChef);
    }

    public void SetDead(bool isDead)
    {
        deadElement.SetActive(isDead);
    }
}
