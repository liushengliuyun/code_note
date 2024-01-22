using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WireMono : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Slider slider;
    [SerializeField] private Image KnobImage;
    [SerializeField] private Sprite grayKnob;
    [SerializeField] private Sprite greenKnob;

    private float fillAmount;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            slider.value = value;
            KnobImage.sprite = value >= 1 ? greenKnob : grayKnob;
            fillAmount = value;
        }
    }
}