using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeClose : UIBase<UIChargeClose>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button leaveBtn;
        [SerializeField] private Button stayBtn;
        [SerializeField] private Button closeBtn;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            leaveBtn.onClick.AddListener(() =>
            {
                Close();
                UserInterfaceSystem.That.RemoveUIByName("UIChargeCtrl");
            });
            
            stayBtn.onClick.AddListener(Close);
            closeBtn.onClick.AddListener(Close);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}