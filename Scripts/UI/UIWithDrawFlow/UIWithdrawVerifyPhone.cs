using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawVerifyPhone : UIBase<UIWithdrawVerifyPhone>
    {
        private Button brbtnclose;
        private Button brbtnverify;
        private MyButton brbutton;

        private Text brtxtdes;
        private InputField brinputverifycode;

        private GameObject brtxttitle;
        private GameObject brtxtplaceholder;
        private GameObject brtxtverify;

        private string brphonenumber;
        public override void InitEvents()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        public override void InitVm()
        {
            throw new System.NotImplementedException();
        }

        public override void InitBinds()
        {
            throw new System.NotImplementedException();
        }
    }
}