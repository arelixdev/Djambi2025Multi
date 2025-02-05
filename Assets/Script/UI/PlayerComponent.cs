using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerComponent : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public Image colorValue;
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
        colorPanel.SetActive(!colorPicker.activeSelf);
    }

    internal void SetReady()
    {
        ready.SetActive(true);
    }

    internal void SetNotReady()
    {
        ready.SetActive(false);
    }
}
