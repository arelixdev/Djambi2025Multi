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
    public GameObject ready;

    private void Start() {
        ready.SetActive(false);
        colorPicker.SetActive(false);
    }

    public void SetPlayerName(string name) {
        playerName.text = name;
    }

    public void ToggleColorPicker() {
        colorPicker.SetActive(!colorPicker.activeSelf);
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
