using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemMono : MonoBehaviour
{
    // Start is called before the first frame update
    public MyButton Button;
    public Text ItemCountText;
    public Text VipText;
    [SerializeField] private Image Frame;
    public Image Icon;
    [SerializeField] private Transform lockTransform;

    private bool isLock;
    private bool isPremium;

    [SerializeField] private Transform selectTrans;

    public bool IsLock
    {
        get => isLock;
        set
        {
            lockTransform.SetActive(value);
            isLock = value;
        }
    }

    public bool IsPremium
    {
        get => isPremium;
        set
        {
            // Frame.sprite = value ? premiumFrame : basicFrame;
            // lockTransform.GetComponent<Image>().sprite = value ? premiumLock : basicLock;
            isPremium = value;
        }
    }

    private bool isGet;

    public bool IsGet
    {
        get => isLock;
        set
        {
            // transform.SetAlpha(value ? 0.5f : 1);
            Frame.SetActive(value);
            selectTrans.SetActive(value);
            isGet = value;
        }
    }
}