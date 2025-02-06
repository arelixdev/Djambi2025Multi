using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerComponent : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public Image colorObject;
    public GameObject colorPicker;
    public GameObject colorPanel;

    public List<GameObject> colors;
    public GameObject ready;

    private void Start() {
        ready.SetActive(false);
        colorPicker.SetActive(false);
        colorPanel.SetActive(false);

    }

    public void SetPlayerName(string name) {
        playerName.text = name;
    }

    public void ToggleColorPicker() {
        colorPanel.SetActive(!colorPanel.activeSelf);
        LockColor();
    }

    public void SetupColor(int colorValue){
        colorObject.color = PlayerManager.Instance.colorList[colorValue];
    }

    public void DesactivateBtn()
    {
        colorObject.GetComponent<Button>().interactable = false;
    }

    internal void SetReady()
    {
        ready.SetActive(true);
    }

    internal void SetNotReady()
    {
        ready.SetActive(false);
    }

    public void ChangeColor(int colorValue)
    {
        Debug.Log("ChangeColor");
        UpdateColor(colorValue);
        PlayerManager.Instance.SetColorValue(colorValue);
        //TODO send to server and broadcast to all clients
        NetUpdateColorLobby ucl = new NetUpdateColorLobby();
        ucl.colorValue = colorValue;
        ucl.playerValue = PlayerManager.Instance.GetPlayerValue();
        Client.Instance.SendToServer(ucl);

        colorPanel.SetActive(false);
    }

    public void UpdateColor(int colorValue)
    {
        colorObject.color = PlayerManager.Instance.colorList[colorValue];
    }

    internal void LockColor()
    {
        foreach(GameObject color in colors)
        {
            color.GetComponent<Button>().interactable = true;
        }

        foreach(ClientInformation client in PlayerManager.Instance.clients)
        {
            for(int i = 0; i < colors.Count; i++)
            {
                if(client.colorValue == i)
                {
                    colors[i].GetComponent<Button>().interactable = false;
                }
            }
        }   
    }
}
