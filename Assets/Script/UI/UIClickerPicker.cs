using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIClickerPicker : MonoBehaviour
{
    private Image image;
    public ColorPicker colorPicker;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = colorPicker.color;
        colorPicker.onColorChanged += OnColorChanged;
    }


    public void OnColorChanged(Color c)
    {
        image.color = c;
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
            colorPicker.onColorChanged -= OnColorChanged;
    }
}
