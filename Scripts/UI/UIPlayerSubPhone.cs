using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using Reactive.Bindings;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityTimer;
using Utils;

namespace UI
{
    public class UIPlayerSubPhone : UIBase<UIPlayerSubPhone>
    {
        public Gradient2[] Gradient2s;
        
        public MyButton SubmitBtn;

        public MyButton CloseBtn;

        public Text NotValidTip;

        public Text PhoneValidTip;

        public Text EmailValidTip;

        public Text EmailTitle;

        public Text PhoneTitle;

        [SerializeField] private InputField EmailInputField;

        [SerializeField] private InputField PhoneInputField;

        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {
            //提交弹窗打点
            YZFunnelUtil.SendYZEvent("pop_contact");

            NotValidTip.SetActive(false);

            CloseBtn.SetClick(OnCloseBtnClick);

            SubmitBtn.SetClick(OnSubmitBtnClick);

            EmailInputField.OnDeselectAsObservable().Subscribe((data) =>
            {
                if (!EmailInputField.text.IsNullOrEmpty())
                {
                    CheckEmail(out var valid);
                    // if (!valid)
                    // {
                    //     UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_sub_phone_tip3"));
                    // }
                }

                vm[vname.Focus.ToString()].ToIObservable<int>().Value = notFocusAny;
            });

            EmailInputField.OnSelectAsObservable().Subscribe((data) =>
            {
                vm[vname.Focus.ToString()].ToIObservable<int>().Value = focusOnEmail;
            });

            PhoneInputField.OnDeselectAsObservable().Subscribe((data) =>
            {
                if (!PhoneInputField.text.IsNullOrEmpty())
                {
                    CheckPhoneNumber(out var valid);
                    // if (!valid)
                    // {
                    //     UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_sub_phone_tip4"));
                    // }
                }

                vm[vname.Focus.ToString()].ToIObservable<int>().Value = notFocusAny;
            });

            PhoneInputField.OnSelectAsObservable().Subscribe((data) =>
            {
                vm[vname.Focus.ToString()].ToIObservable<int>().Value = focusOnPhoneNumber;
            });

            PhoneInputField.onValueChanged.AddListener(s =>
            {
                vm[vname.PhoneNumber.ToString()].ToIObservable<string>().Value = s;
            });

            EmailInputField.onValueChanged.AddListener(s =>
            {
                vm[vname.Email.ToString()].ToIObservable<string>().Value = s;
            });
        }

        enum vname
        {
            Focus,
            Email,
            PhoneNumber,
        }

        public override void InitVm()
        {
            vm[vname.Focus.ToString()] = new ReactivePropertySlim<int>();

            vm[vname.Email.ToString()] = new ReactivePropertySlim<string>();

            vm[vname.PhoneNumber.ToString()] = new ReactivePropertySlim<string>();
        }

        private const int notFocusAny = 0;
        private const int focusOnEmail = 1;
        private const int focusOnPhoneNumber = 2;

        public override void InitBinds()
        {
            vm[vname.Focus.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                CheckInputValid(out bool phoneValid, out bool emailValid);

                // switch (value)
                // {
                //     case focusOnPhoneNumber:
                //         EmailTitle.SetActive(false);
                //         PhoneTitle.SetActive(true);
                //         break;
                //     case focusOnEmail:
                //         EmailTitle.SetActive(true);
                //         PhoneTitle.SetActive(false);
                //         break;
                //     default:
                //         EmailTitle.SetActive(false);
                //         PhoneTitle.SetActive(false);
                //         break;
                // }
            });

            vm[vname.Email.ToString()].ToIObservable<string>().Subscribe(value =>
            {
                if (!value.IsNullOrEmpty())
                {
                    NotValidTip.SetActive(false);
                }

                CheckInputValid(out bool phoneValid, out bool emailValid);
            });

            vm[vname.PhoneNumber.ToString()].ToIObservable<string>().Subscribe(value =>
            {
                if (!value.IsNullOrEmpty())
                {
                    NotValidTip.SetActive(false);
                }

                CheckInputValid(out bool phoneValid, out bool emailValid);
            });
        }

        private void CheckInputValid(out bool phoneValid, out bool emailValid)
        {
            CheckPhoneNumber(out var phone_valid);

            CheckEmail(out var email_valid);

            phoneValid = phone_valid;

            emailValid = email_valid;

            SubmitBtn.Gray = EmailInputField.text.IsNullOrEmpty() && PhoneInputField.text.IsNullOrEmpty();
        }

        private void CheckPhoneNumber(out bool valid)
        {
            var value = vm[vname.PhoneNumber.ToString()].ToIObservable<string>().Value;

            //只支持6-20位的数字
            var focus = vm[vname.Focus.ToString()].ToIObservable<int>().Value;

            valid = !value.IsNullOrEmpty() && value.IsNumeric();

            if (value.IsNullOrEmpty())
            {
                PhoneValidTip.SetActive(focus == focusOnPhoneNumber);
            }
            else
            {
                PhoneValidTip.SetActive(!valid);
            }
        }

        private void CheckEmail(out bool valid)
        {
            var email = vm[vname.Email.ToString()].ToIObservable<string>().Value;

            valid = email.IsEmailAddress();

            var focus = vm[vname.Focus.ToString()].ToIObservable<int>().Value;

            if (email.IsNullOrEmpty())
            {
                EmailValidTip.SetActive(focus == focusOnEmail);
            }
            else
            {
                EmailValidTip.SetActive(!valid);
            }
        }

        public override void InitEvents()
        {
        }

        private Timer timer;
        
        void OnSubmitBtnClick()
        {
            if (EmailInputField.text.IsNullOrEmpty() && PhoneInputField.text.IsNullOrEmpty())
            {
                NotValidTip.SetActive(true);

                timer?.Cancel();
                
                foreach (var gradient2 in Gradient2s)
                {
                    gradient2.Offset = -1;
                    gradient2.LoopSpeed = 3;
                    gradient2.enabled = true;
                }

                timer = this.AttachTimer(1.4f, () =>
                {
                    foreach (var gradient2 in Gradient2s)
                    {
                        gradient2.enabled = false;
                    }
                });
                
                return;
            }

            CheckInputValid(out bool phoneValid, out bool emailValid);

            if (phoneValid || emailValid)
            {
                Close();
                string email = null;
                string phone = null;

                if (emailValid)
                {
                    email = EmailInputField.text;
                }

                if (phoneValid)
                {
                    phone = PhoneInputField.text;
                }
                
                MediatorRequest.Instance.BindVIPInfo(email, phone);
                UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Bind, email);
            }
            else
            {
                string descValue = I18N.Get("key_sub_phone_tip4");
                if (!EmailInputField.text.IsNullOrEmpty())
                {
                    descValue = I18N.Get("key_sub_phone_tip3");
                }

                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    desc = descValue,
                    Type = UIConfirmData.UIConfirmType.OneBtn,
                    confirmTitle = I18N.Get("key_ok")
                });
            }
        }
    }
}