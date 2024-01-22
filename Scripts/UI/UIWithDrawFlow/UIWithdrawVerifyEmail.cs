using System;
using System.Collections;
using System.Collections.Generic;
using AndroidCShape;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawVerifyEmail : UIBase<UIWithdrawVerifyEmail>
    {
        [SerializeField] private Button brbtnback;
        [SerializeField] private Button brbtnreturn;
        [SerializeField] private Button brbtnresend;
        [SerializeField] private Button brbtnopenemail;
        [SerializeField] private Text brtxtbtnresend;
        [SerializeField] private Text brtxtemail;

        private GameObject brtxttitle;
        private GameObject brtxtverify;
        private GameObject brtxtcheck;
        private GameObject brtxtopenmail;
        private GameObject brtxtresend;
        private GameObject brtxtwrong;
        private GameObject brtxtnote;

        private string bremail;
        private float brresendtimer = 0;
        public override void InitEvents()
        {
           
        }

        public override void OnStart()
        {
            brbtnback.onClick.AddListener(() => {
                Close();
            });

            brbtnreturn.onClick.AddListener(() => {
                Close();
                UserInterfaceSystem.That.ShowUI<UIWithdrawEmail>();
            });

            brbtnopenemail.onClick.AddListener(() => {
#if (UNITY_ANDROID && !NO_SDK)
              YZAndroidPlugin.Shared.AndroidOpenEmailApp();  
#endif
            });

            brbtnresend.onClick.AddListener(()=>{
                //YZServerApiWithdraw.Shared.YZHyperResend(bremail);
                MediatorRequest.Instance.WithdrawResendEmail(bremail);
                brresendtimer = 300;
                brbtnresend.interactable = false;
            });

            brtxtemail.text = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);

            StartCoroutine(VirifyEamil());
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        private void Update()
        {
            if (brresendtimer > 0)
            {
                brresendtimer -= Time.deltaTime;
                if (brresendtimer > 0)
                {
                    brtxtbtnresend.text = YZString.Concat(I18N.Get("key_resend_email"), "(", (int)brresendtimer, "s)");
                }
                else
                {
                    brtxtbtnresend.text = I18N.Get("key_resend_email");
                    brbtnresend.interactable = true;
                }
            }
        }
        
        IEnumerator VirifyEamil()
        {
            string mail = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);
            while (YZDataUtil.GetYZInt(YZConstUtil.YZWithdrawMailVerified, 0) == 0)
            {
                yield return new WaitForSeconds(5.0f);
                MediatorRequest.Instance.PUSH_NOTIFY();
            }
            
            // 验证邮箱成功
            UserInterfaceSystem.That.ShowUI<UIWithdrawVerifySuccess>();
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                // string mail = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);
                MediatorRequest.Instance.PUSH_NOTIFY();
            }
        }
    }
}