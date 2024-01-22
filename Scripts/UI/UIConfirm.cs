//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using System;
using UnityEngine;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Utils.Static;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;

public class UIConfirmData
{
    public enum UIConfirmType
    {
        TwoBtn,
        OneBtn
    }

    public UIConfirmType Type;
    public Action confirmCall;
    public Action cancleCall;
    public string title;
    public string desc;
    public string confirmTitle;
    public string cancelTitle;
    public bool HideCloseBtn;
    public bool IsNetWorkError;

    /// <summary>
    /// 是否显示重连按钮
    /// </summary>
    public bool ShowTryAgain;

    /// <summary>
    /// 点击后, 等待条件满足 才关闭 confirm
    /// </summary>
    public bool WaitCloseCallback;

    public Action SendTryAgain;

    public Vector2? Position;

    /// <summary>
    /// rect transform 的width 和height
    /// </summary>
    public Vector2? Rect2D;

    public TextAnchor AligmentType = TextAnchor.MiddleCenter;
}

public class UIConfirm : UIBase<UIConfirm>
{
    #region UI Variable Statement

    [SerializeField] private Image CancelBtnMask;
    [SerializeField] private Transform NetWorkErrorGroup;
    [SerializeField] private Text netWorkErrorText;
    [SerializeField] private Transform TwoBtnGroup;
    [SerializeField] private Transform OneBtnGroup;
    [SerializeField] private MyButton SingleConfirmBtn;

    [SerializeField] private Button CloseBtn;

    [SerializeField] private Text titleText;

    [SerializeField] private Text descText;
    [SerializeField] private Button button_CancelBtn;

    [SerializeField] private Text cancelBtnTitle;

    [SerializeField] private Button button_ComfirmBtn;

    [SerializeField] private Text confirmBtntitle;

    #endregion

    public override UIType uiType { get; set; } = UIType.Window;

    public override void Refresh()
    {
        base.Refresh();
        OnStart();
    }

    private TweenerCore<float, float, FloatOptions> tryAgainDisposable;

    private int tryAgainCount;

    public override void OnStart()
    {
        var data = UIConfirmData;
     
        //
        CloseBtn.onClick.RemoveAllListeners();
        button_CancelBtn.onClick.RemoveAllListeners();
        button_ComfirmBtn.onClick.RemoveAllListeners();
        SingleConfirmBtn.onClick.RemoveAllListeners();
        
        descText.alignment = data.AligmentType;
        var panel = transform.GetChild(1);
        if (panel != null)
        {
            var rectTransform = panel.GetComponent<RectTransform>();
            if (data.Rect2D != null)
            {
                var rect2d = (Vector2)data.Rect2D;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect2d.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect2d.y);
            }

            if (data.Position != null)
            {
                rectTransform.anchoredPosition = (Vector2)data.Position;
            }
        }

        void Confirm()
        {
            try
            {
                data.confirmCall?.Invoke();
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
            }

            if (!data.WaitCloseCallback)
            {
                Close();
            }
        }

        void Cancel()
        {
            try
            {
                data.cancleCall?.Invoke();
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
            }

            if (!data.WaitCloseCallback)
            {
                Close();
            }
        }

        void OnCloseBtnClick()
        {
            try
            {
                data.cancleCall?.Invoke();
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
            }

            Close();
        }
        
        button_CancelBtn.onClick.AddListener(Cancel);
        button_ComfirmBtn.onClick.AddListener(Confirm);
        SingleConfirmBtn.onClick.AddListener(Confirm);

        if (data.ShowTryAgain)
        {
            SetTryAgainTween(data);
        }

        if (data.HideCloseBtn)
        {
            CloseBtn.SetActive(false);
        }

        LoadStyle(data.Type);

        CloseBtn.onClick.AddListener(OnCloseBtnClick);

        var desc = data.desc;
        if (data.IsNetWorkError)
        {
            netWorkErrorText.text = desc;
        }
        else
        {
            descText.text = desc;
        }

        descText.SetActive(!data.IsNetWorkError);
        NetWorkErrorGroup.SetActive(data.IsNetWorkError);

        var title = data.title;
        if (!title.IsNullOrEmpty())
        {
            titleText.text = title;
        }

        var confirmText = data.confirmTitle;
        if (!confirmText.IsNullOrEmpty())
        {
            confirmBtntitle.text = confirmText;
            SingleConfirmBtn.title = confirmText;
        }

        var cancelTitle = data.cancelTitle;
        if (!cancelTitle.IsNullOrEmpty())
        {
            cancelBtnTitle.text = cancelTitle;
        }
    }

    void LoadStyle(UIConfirmData.UIConfirmType uiConfirmType)
    {
        switch (uiConfirmType)
        {
            case UIConfirmData.UIConfirmType.TwoBtn:
                TwoBtnGroup.SetActive(true);
                OneBtnGroup.SetActive(false);

                break;
            case UIConfirmData.UIConfirmType.OneBtn:
                TwoBtnGroup.SetActive(false);
                OneBtnGroup.SetActive(true);
                break;
        }
    }

    private void SetTryAgainTween(UIConfirmData data)
    {
        tryAgainDisposable?.Kill();
        tryAgainDisposable = DOTween.To(() => CancelBtnMask.fillAmount,
            x => CancelBtnMask.fillAmount = x,
            1,
            5f
        ).OnComplete(() => { TryTryAgain(data); });
    }

    public override void InitVm()
    {
    }

    public override void InitBinds()
    {
    }

    public override void InitEvents()
    {
        var data = UIConfirmData;
        if (data.ShowTryAgain)
        {
            AddEventListener(GlobalEvent.Try_Again, (sender, eventArgs) =>
            {
                TryTryAgain(data);

                SetTryAgainTween(data);
            });
        }
    }

    private UIConfirmData uiConfirmData;
    private UIConfirmData UIConfirmData =>uiConfirmData ??= GetArgsByIndex<UIConfirmData>(0) ?? new UIConfirmData();


    private void TryTryAgain(UIConfirmData data)
    {
        tryAgainCount++;
        CancelBtnMask.fillAmount = 0;
        //自动重连
        if (tryAgainCount < 3)
        {
            data.SendTryAgain?.Invoke();
            SetTryAgainTween(data);
        }
        else
        {
            data.SendTryAgain?.Invoke();
            //只显示relogin
            LoadStyle(UIConfirmData.UIConfirmType.OneBtn);
            descText.text = I18N.Get("key_net_relogin");
        }
    }

    public override void Close()
    {
        OnAnimationOut();
    }

    protected override void OnAnimationIn()
    {
        transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
        transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    protected override void OnAnimationOut()
    {
        transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
            .OnComplete(() => base.Close());
    }
}