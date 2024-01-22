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
using System.Linq;
using Core.Extensions;
using UnityEngine;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using UI;
using UI.UIWithDrawFlow;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

public enum MailLoginConfirmPanel
{
    /// <summary>
    /// 账号不存在
    /// </summary>
    AccountDoseNotExist,

    ForGetPassWord,

    /// <summary>
    /// 提示玩家该游客账号已有充值， 是否需要绑定
    /// </summary>
    HasDeposit,

    /// <summary>
    /// 玩家验证成功
    /// </summary>
    VerifiedGroup,
    ChangePassWord
}

public class UIMailLoginConfirm : UIBase<UIMailLoginConfirm>
{
    #region UI Variable Statement

    [SerializeField] private Transform[] AllPanels;

    [SerializeField] private MyButton SaveAccountBtn;
    [SerializeField] private MyButton GotItLineBtn;
    [SerializeField] private MyButton EmailLineBtn;
    [SerializeField] private MyButton ForGetLineBtn;

    [SerializeField] private MyButton ContinueBtn;


    [SerializeField] private MyButton ResetPassWordBtn;

    [SerializeField] private MyButton CloseBtn;

    [SerializeField] private Transform AccountDoseNotExist;
    [SerializeField] private Transform ForGetPassWord;
    [SerializeField] private Transform HasDeposit;
    [SerializeField] private Transform VerifiedGroup;

    [SerializeField] private Text Title;

    [SerializeField] private InputField ForGetPassWordInput;
    [SerializeField] private Text ForGetMailTitle;

    #endregion

    public override UIType uiType { get; set; } = UIType.Window;
    private bool isFromWithdraw;

    enum vname
    {
        Panel,
    }

    public override void OnStart()
    {
        for (int i = 0; i < AllPanels.Length; i++)
        {
            AllPanels[i].SetActive(false);
        }

        ContinueBtn.SetClick(Close);

        CloseBtn.SetClick(OnCloseBtnClick);

        //玩家确认 ， 继续登陆其他账号
        GotItLineBtn.SetClick(() =>
        {
            UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Sign);
            Close();
        });

        SaveAccountBtn.SetClick(() =>
        {
            Close();
            UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Bind);
        });

        //初始化不显示
        ForGetMailTitle.SetActive(false);

        ForGetPassWordInput.contentType = InputField.ContentType.EmailAddress;

        ForGetPassWordInput.OnSelectAsObservable().Subscribe((eventData) => { ForGetMailTitle.SetActive(true); });

        ForGetPassWordInput.OnDeselectAsObservable().Subscribe((eventData) => { ForGetMailTitle.SetActive(false); });


        ForGetPassWordInput.onValueChanged.AddListener(content => { ResetPassWordBtn.Gray = content.IsNullOrEmpty(); });

        ResetPassWordBtn.Gray = true;

        ResetPassWordBtn.SetClick(() =>
        {
            if (ResetPassWordBtn.Gray)
            {
                return;
            }

            if (!ForGetPassWordInput.text.IsEmailAddress())
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_email_limit_tip"));
                return;
            }

            MediatorRequest.Instance.SendChangePsswordMail(ForGetPassWordInput.text);
        });
    }

    private void OnClose()
    {
        if (isFromWithdraw && VerifiedGroup.gameObject.activeSelf)
        {
            UserInterfaceSystem.That.ShowUI<UIWithdrawDetails>();
            Root.Instance.Role.HaveSavedMail = true;
        }
    }

    private string initEmail;

    public override void InitVm()
    {
        var panel = GetArgsByIndex<MailLoginConfirmPanel>(0);
        initEmail = GetArgsByIndex<string>(1);
        isFromWithdraw = GetArgsByIndex<bool>(2);
        vm[vname.Panel.ToString()] = new ReactivePropertySlim<MailLoginConfirmPanel>(panel);
    }

    public override void InitBinds()
    {
        vm[vname.Panel.ToString()].ToIObservable<MailLoginConfirmPanel>().Subscribe(value =>
        {
            switch (value)
            {
                case MailLoginConfirmPanel.AccountDoseNotExist:
                    AccountDoseNotExist.SetActive(true);
                    break;
                case MailLoginConfirmPanel.ForGetPassWord:
                case MailLoginConfirmPanel.ChangePassWord:


                    ForGetPassWord.SetActive(true);
                    break;
                case MailLoginConfirmPanel.HasDeposit:
                    HasDeposit.SetActive(true);
                    break;
                case MailLoginConfirmPanel.VerifiedGroup:
                    VerifiedGroup.SetActive(true);
                    break;
            }

            switch (value)
            {
                case MailLoginConfirmPanel.ForGetPassWord:
                case MailLoginConfirmPanel.ChangePassWord:
                    ForGetPassWordInput.text = initEmail;
                    break;
            }

            //文本
            switch (value)
            {
                case MailLoginConfirmPanel.AccountDoseNotExist:
                    Title.text = I18N.Get("key_sign_in");
                    break;
                case MailLoginConfirmPanel.ForGetPassWord:
                    Title.text = I18N.Get("key_sign_in_forgot");
                    break;
                case MailLoginConfirmPanel.HasDeposit:
                    Title.text = I18N.Get("key_opps");
                    break;
                case MailLoginConfirmPanel.VerifiedGroup:
                    break;
                case MailLoginConfirmPanel.ChangePassWord:
                    Title.text = I18N.Get("key_change_password");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        });
    }

    public override void InitEvents()
    {
        AddEventListener(Proto.CHANGE_PASSWORD, (sender, eventArgs) =>
        {
            if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
            {
                // UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_mail_send_success"));
                UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.ChangePasswordCheck,
                    ForGetPassWordInput.text);
                Close();
            }
        });
    }
}