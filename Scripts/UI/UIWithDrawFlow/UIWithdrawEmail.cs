using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawEmail : UIBase<UIWithdrawEmail>
    {
        [SerializeField] private Button brbtnback;

        [SerializeField] private GameObject brtxttitle;
        [SerializeField] private GameObject brtxtdesc;
        [SerializeField] private GameObject brtxtsubmit;
        [SerializeField] private GameObject brtxtnot;

        [SerializeField] private MyButton brbutton;
        [SerializeField] private string bremail;
        [SerializeField] private InputField brinput_email;
        [SerializeField] private string bremailcheck;
        [SerializeField] private InputField brinput_email_check;
        public override void InitEvents()
        {
        }
        
        private void OnChangeInputText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                bremail = text;
            }
            else
            {
                var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"\s", "");
                bremail = srcStr;
                //
                if (brinput_email != null)
                {
                    brinput_email.text = srcStr.ToString();
                    brinput_email.caretPosition = srcStr.Length;
                }
            }

            bool gray = string.IsNullOrEmpty(bremail) || string.IsNullOrEmpty(bremailcheck)
                                                      || !bremail.IsEmailAddress() || !bremailcheck.IsEmailAddress();
            brbutton.Gray = gray;
            brbutton.interactable = !gray;
        }
        
        private void OnChangeInputTextAgain(string text)
        {
            var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"\s", "");
            bremailcheck = srcStr;
            //
            if (brinput_email_check != null)
            {
                brinput_email_check.text = srcStr.ToString();
                brinput_email_check.caretPosition = srcStr.Length;
            }
            
            bool gray = string.IsNullOrEmpty(bremail) || string.IsNullOrEmpty(bremailcheck)                                                      
                                                      || !bremail.IsEmailAddress() || !bremailcheck.IsEmailAddress();
            brbutton.Gray = gray;
            brbutton.interactable = !gray;
        }

        public override void OnStart()
        {
            
            brbtnback.onClick.AddListener(() =>
            {
                Close();
            });
            
            brinput_email.onEndEdit.AddListener((str) =>
            {
                OnChangeInputText(str);
            });
            
            brinput_email_check.onEndEdit.AddListener((str) =>
            {
                OnChangeInputTextAgain(str);
            });

            brbutton.onClick.AddListener(() => {
                if (bremail != bremailcheck)
                {
                    //BRTopControl.BRShowAutoHideTips(BRLocal.GetLocal(BRLocalID.key_email_different));
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_email_different"));
                    return;
                }
                MediatorRequest.Instance.WithdrawBindEmail(bremail);
            });

            OnChangeInputText("");
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}