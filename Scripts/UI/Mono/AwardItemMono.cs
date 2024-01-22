using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using UI.Effect;
using UnityEngine;
using UnityEngine.UI;

public class AwardItemMono : MonoBehaviour
{
    // Start is called before the first frame update
    public Text ItemCountText;
    public Image Icon;
    public MyButton ItemBtn;

    [SerializeField] private Transform BgNotSelected;
    [SerializeField] private Transform BgSelected;
    [SerializeField] private Transform DayTextTrans;
    [SerializeField] private Transform DayTextNotSelecetedTrans;
    [SerializeField] private Transform ItemCheck;
    [SerializeField] private Transform LightTrans;

    public enum ClaimState
    {
        CanClaim,
        Claimed,
        CanNotClaim,
    }

    private ClaimState state = ClaimState.Claimed;

    public ClaimState ClamState
    {
        get => state;
        set
        {
            SetGrey(false);
            BgNotSelected.SetActive(false);
            BgSelected.SetActive(false);
            ItemCheck.SetActive(false);

            switch (value)
            {
                case ClaimState.CanClaim:
                {
                    SetGrey(false);
                    DayTextNotSelecetedTrans.SetActive(false);
                    DayTextTrans.SetActive(true);
                    BgNotSelected.SetActive(false);
                    BgSelected.SetActive(true);
                    ItemCheck.SetActive(false);
                    LightTrans.SetActive(true);
                    break;
                }
                case ClaimState.Claimed:
                {
                    SetGrey(true);
                    DayTextNotSelecetedTrans.SetActive(true);
                    DayTextTrans.SetActive(false);
                    BgNotSelected.SetActive(true);
                    BgSelected.SetActive(false);
                    ItemCheck.SetActive(true);
                    LightTrans.SetActive(false);
                    break;
                }
                case ClaimState.CanNotClaim:
                {
                    SetGrey(false);
                    DayTextNotSelecetedTrans.SetActive(true);
                    DayTextTrans.SetActive(false);
                    BgNotSelected.SetActive(true);
                    BgSelected.SetActive(false);
                    ItemCheck.SetActive(false);
                    LightTrans.SetActive(false);
                    break;
                }
            }

            state = value;
        }
    }

    private void SetGrey(bool enable)
    {
        float darkness = 180.0f;
        var grey = new Color(darkness / 255.0f, darkness / 255.0f, darkness / 255.0f, 255.0f / 255.0f);
        if (enable)
        {
            BgNotSelected.GetComponent<Image>().color = grey;
            Icon.color = grey;
            ItemCountText.GetComponent<Text>().color = grey;
            DayTextNotSelecetedTrans.GetComponent<Text>().color = grey;
            var colorOutline = DayTextNotSelecetedTrans.GetComponent<Text2DOutline>().OutlineColor;
            colorOutline.a = 100.0f / 255.0f;
            DayTextNotSelecetedTrans.GetComponent<Text2DOutline>().OutlineColor = colorOutline;
        }
        else
        {
            BgNotSelected.GetComponent<Image>().color = Color.white;
            Icon.color = Color.white;
            ItemCountText.GetComponent<Text>().color = Color.white;
            DayTextNotSelecetedTrans.GetComponent<Text>().color = Color.white;
            var colorOutline = DayTextNotSelecetedTrans.GetComponent<Text2DOutline>().OutlineColor;
            colorOutline.a = 1;
            DayTextNotSelecetedTrans.GetComponent<Text2DOutline>().OutlineColor = colorOutline;
        }
    }
}