using System;
using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UI;
using UI.Effect;
using UI.UIWithDrawFlow;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

public enum EmailValidType
{
    /// <summary>
    /// 没有绑定游戏
    /// </summary>
    None,


    /// <summary>
    /// 已经绑定
    /// </summary>
    Binded
}

public class SettingPanelMono : MonoBehaviour
{
    public Text InfoText;
    public MyButton SaveAccountBtn;
    public MyButton SignBtn;
    public MyButton ChangePasswordBtn;
    public MyButton SwitchAccountBtn;
    public MyButton WithdrawBtn;
    public MyButton CashFlowBtn;
    public MyButton SettingsBtn;
    public MyButton HowtoPlayBtn;
    public MyButton SupportBtn;
    public MyButton LegalBtn;
    public MyButton FAQBtn;
    public MyButton GMLoginBtn;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        SaveAccountBtn.SetClick(() =>
        {
       
            UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Bind);
        });

        SignBtn.SetClick(() =>
        {
            //检查游客账号是否有充值
            if (Root.Instance.ChargeSuccessCount > 0)
            {
                //提示玩家
                UserInterfaceSystem.That.ShowUI<UIMailLoginConfirm>(MailLoginConfirmPanel.HasDeposit);
            }
            else
            {
                UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Sign);
            }
        });

        SwitchAccountBtn.SetClick(() => UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.SwitchAccount));

        WithdrawBtn.SetActive(!Root.Instance.IsNaturalFlow);

        WithdrawBtn.SetClick(() =>
        {
            if (WithdrawBtn.Gray)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_cancel_withdraw_desc1"));
                return;
            }
            YZWithdrawControl.YZOnOpenDetail();
        });
        
        timer?.Dispose();
        
        void RefreshWithDraw()
        {
            if (Root.Instance.UserInfo.InCancelCD)
            {
                WithdrawBtn.title = TimeUtils.Instance.ToDayHourMinuteSecond(Root.Instance.UserInfo.lastCancelCashTime);
            }
            else
            {
                WithdrawBtn.title = I18N.Get("key_withdraw");
            }

            WithdrawBtn.Gray = Root.Instance.UserInfo.InCancelCD;
        }

        RefreshWithDraw();
        if (Root.Instance.UserInfo.InCancelCD)
        {
            timer = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(
                l => { RefreshWithDraw(); }).AddTo(this);
        }
        
        CashFlowBtn.SetActive(!Root.Instance.IsNaturalFlow);
        
        CashFlowBtn.SetClick(()=> UserInterfaceSystem.That.ShowUI<UICashFlow>());

        ChangePasswordBtn.SetClick(() =>
            UserInterfaceSystem.That.ShowUI<UIMailLoginConfirm>(MailLoginConfirmPanel.ChangePassWord));

        HowtoPlayBtn.SetClick(() =>
            {
                Root.Instance.ClickHowToPlay = true;
                UserInterfaceSystem.That.ShowAnotherUI<UIBingo>(new GameData()
                {
                    ["guideType"] = BingoGuideType.Teaching,
                });
            }
        );

        LegalBtn.SetClick(() => { Application.OpenURL(YZDefineUtil.GetYZPrivacyPolicyUrl()); });

        SupportBtn.SetClick(() => { YZNativeUtil.ContactYZUS(EmailPos.Setting); });

        if (Debug.isDebugBuild)
        {
            InfoText.text = $"v{YZNativeUtil.GetYZAppVersion()} uid = {Root.Instance.UserId}";
            GMLoginBtn.SetClick(() =>
            {
                MediatorRequest.ResetEnvironment();
                UserInterfaceSystem.That.CloseAllUI();
                UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.GMLogin);
            });
        }
        else
        {
            InfoText.text = $"v{YZNativeUtil.GetYZAppVersion()}";
        }

        SettingsBtn.SetClick(() => UserInterfaceSystem.That.ShowUI<UISetting>());
    }

    private EmailValidType emailValidType;
    private IDisposable timer;

    public EmailValidType EmailValidType
    {
        set
        {
            switch (value)
            {
                case EmailValidType.None:
                    SaveAccountBtn.SetActive(true);
                    SignBtn.SetActive(true);
                    ChangePasswordBtn.SetActive(false);
                    SwitchAccountBtn.SetActive(false);
                    break;
                case EmailValidType.Binded:
                    SaveAccountBtn.SetActive(false);
                    SignBtn.SetActive(false);
                    ChangePasswordBtn.SetActive(true);
                    SwitchAccountBtn.SetActive(true);
                    break;
            }

            emailValidType = value;
        }
        get => emailValidType;
    }
}