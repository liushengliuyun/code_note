using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeBonus : UIBase<UIChargeBonus>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button startBtn;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            closeBtn.onClick.AddListener(Close);
            startBtn.onClick.AddListener(Close);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}