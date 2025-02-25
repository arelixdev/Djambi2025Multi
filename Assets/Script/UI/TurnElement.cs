using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

public class TurnElement : MonoBehaviour
{
    [SerializeField] private Color activeColor;
    [SerializeField] private Color desactiveColor;

    [SerializeField] private GameObject yourColor;
    [SerializeField] private GameObject teamColor;
    [SerializeField] private GameObject chefElement;
    [SerializeField] private GameObject deadElement;

    void Start()
    {
        //chefElement.SetActive(false);
        //deadElement.SetActive(false);
        //yourColor.SetActive(false);
    }

    public void SetYourColor(bool isYourColor)
    {
        //yourColor.SetActive(isYourColor);
    }

    public void SetupColor(Color color)
    {
        activeColor = color;
        //chefElement.GetComponent<Image>().color = color;
        //deadElement.GetComponent<Image>().color = color;
        Color.RGBToHSV(color, out float h, out float s, out float v);

        // Réduction de la saturation
        v = Mathf.Clamp01(v - 0.4f); // Réduit de 10% et s'assure qu'on reste entre 0 et 1

        // Conversion en RGB
        desactiveColor = Color.HSVToRGB(h, s, v);
    }

    public void ActiveTurn()
    {
        transform.localScale = new Vector3(1, 1, 1);
        teamColor.GetComponent<Image>().color = activeColor;
    }

    public void DesactiveTurn()
    {
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        teamColor.GetComponent<Image>().color = desactiveColor;
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
