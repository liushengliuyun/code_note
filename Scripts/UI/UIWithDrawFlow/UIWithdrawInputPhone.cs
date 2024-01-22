using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawInputPhone: UIBase<UIWithdrawInputPhone>
    {
        private Button brbtnclose;
        private Button brbtnsendcode;
        private MyButton brbutton;

        private InputField brinputphonenumber;
        private Dropdown brdropdowncountrycode;

        private GameObject brtxttitle;
        private GameObject brtxtdesc;
        private GameObject brtxtsendcode;
        
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