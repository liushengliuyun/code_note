using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawVerifyFail : UIBase<UIWithdrawVerifyFail>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        [SerializeField] private Button brbtnclose;

        private GameObject brtxttitle;
        private GameObject brtxtdesc;
        private GameObject brtxtcontinue;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            brbtnclose.onClick.AddListener(() => 
            {
                Close();
            });

        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}