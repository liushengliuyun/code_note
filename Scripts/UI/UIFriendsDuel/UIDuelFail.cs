using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;

namespace UI
{
    public class UIDuelFail : UIBase<UIDuelFail>
    {
        [SerializeField] private MyButton okBtn;
        [SerializeField] private MyButton closeBtn;
        public override UIType uiType { get; set; } = UIType.Window;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            okBtn.SetClick(Close);
            closeBtn.SetClick(OnCloseBtnClick);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}