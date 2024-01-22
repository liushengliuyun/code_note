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
using System.Collections;
using System.Linq;
using AndroidCShape;
using Castle.Core.Internal;
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
using UI.Effect;
using UI.UIWithDrawFlow;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

public enum MailLoginPanel
{
    /// <summary>
    /// 绑定当前的游客账号
    /// </summary>
    Bind,

    /// <summary>
    /// 从游客账号 登陆邮箱账号
    /// </summary>
    Sign,

    /// <summary>
    /// 从邮箱账号， 登陆另一个邮箱账号
    /// </summary>
    SwitchAccount,

    /// <summary>
    ///  提示玩家打开邮箱确认 是否收到了确认邮件
    /// </summary>
    BindCheckMail,

    /// <summary>
    /// 修改密码后， 发送邮件
    /// </summary>
    ChangePasswordCheck
}

public class UIMailLogin : UIBase<UIMailLogin>
{
    #region UI Variable Statement

    [SerializeField] private MyText OpenEmailDesc;

    [SerializeField] private MyButton ForGetPasswordBtn;

    [SerializeField] private Transform InputPanel;
    [SerializeField] private Transform CheckMailPanel;
    [SerializeField] private MyButton CloseBtn;

    /// <summary>
    /// 错误密码格式的提示
    /// </summary>
    [SerializeField] private MyText PasswordValidTip;

    /// <summary>
    /// 错误邮箱格式的提示
    /// </summary>
    [SerializeField] private MyText EmailValidTip;

    [SerializeField] private MyText Title;

    [SerializeField] private MyText EMailNeedVerifyText;

    [SerializeField] private MyText CheckPanelDesc;

    [SerializeField] private MyText MailCheckText;

    [SerializeField] private MyButton OpenEmailBtn;
    [SerializeField] private MyText InputDescText;

    [SerializeField] private InputField EmailInputField;

    [SerializeField] private MyText EmailTitle;
    [SerializeField] private InputField PassWordInputField;

    [SerializeField] private Toggle KeyToggle;

    [SerializeField] private MyText PassWordTitle;

    [SerializeField] private MyButton SignBtn;

    #endregion

    public override UIType uiType { get; set; } = UIType.Window;

    private const int notFocusAny = 0;
    private const int focusOnEmail = 1;
    private const int focusOnPassword = 2;

    // 是否从提现按钮进入
    private bool isFromWithdraw = false;

    enum vname
    {
        Panel,
        Email,
        Password,
        Focus,
    }

    public override void OnStart()
    {
        if (args != null)
            isFromWithdraw = GetArgsByIndex<bool>(2);

        ForGetPasswordBtn.SetClick(() =>
        {
            UserInterfaceSystem.That.ShowUI<UIMailLoginConfirm>(MailLoginConfirmPanel.ForGetPassWord,
                EmailInputField.text);
        });

        CloseBtn.SetClick(() =>
        {
            bool isMailCheckPanel = CheckMailPanel.gameObject.activeSelf;
            Close();
            if (isMailCheckPanel && isFromWithdraw)
            {
                UserInterfaceSystem.That.ShowUI<UIWithdrawDetails>();
                Root.Instance.Role.HaveSavedMail = true;
            }
        });
        EmailInputField.OnSelectAsObservable().Subscribe((data) =>
        {
            vm[vname.Focus.ToString()].ToIObservable<int>().Value = focusOnEmail;
        });

        EmailInputField.OnDeselectAsObservable().Subscribe((data) =>
        {
            vm[vname.Focus.ToString()].ToIObservable<int>().Value = notFocusAny;
        });

        PassWordInputField.OnSelectAsObservable().Subscribe(data =>
        {
            vm[vname.Focus.ToString()].ToIObservable<int>().Value = focusOnPassword;
        });

        PassWordInputField.OnDeselectAsObservable().Subscribe(data =>
        {
            vm[vname.Focus.ToString()].ToIObservable<int>().Value = notFocusAny;
        });

        SignBtn.onClick.AddListener(SignOrBindBtnClick);

        OpenEmailBtn.SetClick(OpenMailApp);

        EmailInputField.contentType = InputField.ContentType.EmailAddress;

        EmailValidTip.SetActive(false);

        KeyToggle.isOn = true;
        PassWordInputField.contentType = InputField.ContentType.Password;
        //只支持6-20位的数字
        KeyToggle.onValueChanged.AddListener(arg0 =>
        {
            PassWordInputField.contentType = arg0 ? InputField.ContentType.Password : InputField.ContentType.Standard;
            if (!PassWordInputField.text.IsNullOrEmpty())
            {
                PassWordInputField.SetActive(false);
                PassWordInputField.SetActive(true);
            }
        });

        PassWordInputField.onValueChanged.AddListener(s =>
        {
            vm[vname.Password.ToString()].ToIObservable<string>().Value = s;
        });

        EmailInputField.onValueChanged.AddListener(s =>
        {
            vm[vname.Email.ToString()].ToIObservable<string>().Value = s;
        });
    }

    public override void InitVm()
    {
        var email = GetArgsByIndex<string>(1);
        var panel = GetArgsByIndex<MailLoginPanel>(0);
        vm[vname.Focus.ToString()] = new ReactivePropertySlim<int>();
        vm[vname.Panel.ToString()] = new ReactivePropertySlim<MailLoginPanel>(panel);

        vm[vname.Email.ToString()] = new ReactivePropertySlim<string>(email ?? "");

        vm[vname.Password.ToString()] = new ReactivePropertySlim<string>();
    }

    public override void InitBinds()
    {
        vm[vname.Email.ToString()].ToIObservable<string>().Subscribe(value =>
        {
            CheckInputValid();
            EmailInputField.text = value;
        });

        vm[vname.Password.ToString()].ToIObservable<string>().Subscribe(value => { CheckInputValid(); });

        vm[vname.Focus.ToString()].ToIObservable<int>().Subscribe(value =>
        {
            CheckInputValid();

            switch (value)
            {
                case focusOnPassword:
                    EmailTitle.SetActive(false);
                    PassWordTitle.SetActive(true);
                    break;
                case focusOnEmail:
                    EmailTitle.SetActive(true);
                    PassWordTitle.SetActive(false);
                    break;
                default:
                    EmailTitle.SetActive(false);
                    PassWordTitle.SetActive(false);
                    break;
            }
        });


        vm[vname.Panel.ToString()].ToIObservable<MailLoginPanel>().Subscribe(value =>
        {
            switch (value)
            {
                case MailLoginPanel.Bind:
                case MailLoginPanel.Sign:
                case MailLoginPanel.SwitchAccount:
                    InputPanel.SetActive(true);
                    CheckMailPanel.SetActive(false);
                    break;
                case MailLoginPanel.BindCheckMail:
                case MailLoginPanel.ChangePasswordCheck:
                    MailCheckText.text = EmailInputField.text;
                    InputPanel.SetActive(false);
                    CheckMailPanel.SetActive(true);
                    break;
            }

            switch (value)
            {
                case MailLoginPanel.Bind:
                    SignBtn.title = I18N.Get("key_save_account");
                    Title.text = I18N.Get("key_save_account");
                    InputDescText.text = I18N.Get("save_account_desc");
                    break;
                case MailLoginPanel.Sign:
                    SignBtn.title = I18N.Get("key_sign_in");
                    Title.text = I18N.Get("key_sign_in");
                    InputDescText.text = I18N.Get("key_sign_in_des");
                    break;
                case MailLoginPanel.SwitchAccount:
                    SignBtn.title = I18N.Get("key_sign_in");
                    Title.text = I18N.Get("key_change_account");
                    InputDescText.text = I18N.Get("key_change_account_des");
                    break;
                case MailLoginPanel.BindCheckMail:
                    SignBtn.title = I18N.Get("key_open_email_app");
                    Title.text = I18N.Get("key_check_your_email");


                    OpenEmailDesc.text = Root.Instance.IsNaturalFlow
                        ? I18N.Get("key_forget_password_valid")
                        : I18N.Get("key_a_verification");
                    break;
                case MailLoginPanel.ChangePasswordCheck:
                    SignBtn.title = I18N.Get("key_open_email_app");
                    Title.text = I18N.Get("key_check_your_email");
                    OpenEmailDesc.text = I18N.Get("key_forget_password_valid");
                    break;
            }

            switch (value)
            {
                case MailLoginPanel.Bind:
                case MailLoginPanel.BindCheckMail:
                case MailLoginPanel.ChangePasswordCheck:
                    ForGetPasswordBtn.SetActive(false);
                    break;
                case MailLoginPanel.Sign:
                case MailLoginPanel.SwitchAccount:
                    ForGetPasswordBtn.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        });
    }

    private void CheckInputValid()
    {
        CheckPassWord(out var notvilid);

        var email = vm[vname.Email.ToString()].ToIObservable<string>().Value;
        bool isEmailNotValid = CheckMailAddress(email);

        var panelType = vm[vname.Panel.ToString()].ToIObservable<MailLoginPanel>().Value;
        
        SignBtn.Gray = isEmailNotValid  || (panelType is not MailLoginPanel.Bind && notvilid);
    }

    private bool CheckMailAddress(string email)
    {
        var isNotValid = !email.IsEmailAddress();
        var focus = vm[vname.Focus.ToString()].ToIObservable<int>().Value;

        if (email.IsNullOrEmpty())
        {
            EmailValidTip.SetActive(focus == focusOnEmail);
        }
        else
        {
            EmailValidTip.SetActive(isNotValid);
        }

        return isNotValid;
    }

    private void CheckPassWord(out bool notvilid)
    {
        var value = vm[vname.Password.ToString()].ToIObservable<string>().Value;
        const int minLength = 6;
        //只支持6-20位的数字
        var focus = vm[vname.Focus.ToString()].ToIObservable<int>().Value;
        notvilid = value.IsNullOrEmpty() || value.Length is > 20 or < minLength || !value.IsNumericOrLetter();

        if (value.IsNullOrEmpty())
        {
            PasswordValidTip.SetActive(focus == focusOnPassword);
        }
        else
        {
            PasswordValidTip.SetActive(notvilid);
        }
    }

    public override void InitEvents()
    {
        AddEventListener(GlobalEvent.Success_Email_virify,
            (sender, eventArgs) =>
            {
                UserInterfaceSystem.That.ShowUI<UIMailLoginConfirm>(MailLoginConfirmPanel.VerifiedGroup, null,
                    isFromWithdraw);
            });

        AddEventListener("email validate success", (sender, eventArgs) =>
        {
            var panel = vm[vname.Panel.ToString()].ToIObservable<MailLoginPanel>().Value;
            if (panel == MailLoginPanel.BindCheckMail)
            {
                Close();
            }
        });
    }

    private void SignOrBindBtnClick()
    {
        CheckPassWord(out var notvilid);
        if (SignBtn.transform.IsGray())
        {
            // UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_password_digit_tip"));
            return;
        }

        var panelType = vm[vname.Panel.ToString()].ToIObservable<MailLoginPanel>().Value;
        var email = EmailInputField.text;
        var password = PassWordInputField.text;
     
        switch (panelType)
        {
            case MailLoginPanel.Bind:
                if (notvilid)
                {
                    UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                    {
                        desc = I18N.Get("key_password_digit_letter_tip"),
                        Type = UIConfirmData.UIConfirmType.OneBtn,
                        confirmTitle = I18N.Get("key_ok")
                    });    
            
                    return;
                }
                
                MediatorRequest.Instance.BindMail(email, password, () =>
                {
                    if (vm.Any())
                    {
                        vm[vname.Panel.ToString()].ToIObservable<MailLoginPanel>().Value = MailLoginPanel.BindCheckMail;
                    }

                    StartCoroutine(VirifyEamil());
                });
                break;
            case MailLoginPanel.Sign:
            case MailLoginPanel.SwitchAccount:
                MediatorRequest.Instance.EmailLogin(email, password);
                break;
        }
    }

    IEnumerator VirifyEamil()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            MediatorRequest.Instance.PUSH_NOTIFY();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            MediatorRequest.Instance.PUSH_NOTIFY();
        }
    }

    /// <summary>
    /// 打开其他邮件应用
    /// </summary>
    private void OpenMailApp()
    {
        YZNativeUtil.OpenYZEmailApp();
        Root.Instance.Role.HaveSavedMail = true;
    }
}