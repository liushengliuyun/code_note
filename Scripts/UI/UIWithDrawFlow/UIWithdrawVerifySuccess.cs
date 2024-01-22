using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawVerifySuccess : UIBase<UIWithdrawVerifySuccess>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button brbtnclose;
        [SerializeField] private Button brbtnconfirm;

        private GameObject brtxttitle;
        private GameObject brtxtdesc;
        private GameObject brtxtcontinue;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            brbtnclose.onClick.AddListener(() => {
                Close();
                JumpToNewUI();
            });

            brbtnconfirm.onClick.AddListener(() => {
                //BRWithdrawControl.BROnHyperWallet();
                Close();
                JumpToNewUI();
            });
        }

        void JumpToNewUI()
        {
            bool isNameAddAddressVerified = Root.Instance.GetHyperNameAndAddressVerified() > 0;
            if (!isNameAddAddressVerified)
                UserInterfaceSystem.That.ShowUI<UIWithdrawName>();
            else
                // 全都验证过了
                //UserInterfaceSystem.That.ShowUI<UIWithdrawRecord>();
                MediatorRequest.Instance.WithdrawApply(Root.Instance.WithdrawAmount);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}