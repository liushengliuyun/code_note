using CatLib.EventDispatcher;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Utils.Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeOpen : UIBase<UIChargeOpen>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button yesBtn;
        [SerializeField] private Button noBtn;
        [SerializeField] private Button closeBtn;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            
            yesBtn.onClick.AddListener(() =>
            {
                // 打勾
                EventDispatcher.Root.Raise(GlobalEvent.Toggle_Over18YearsOld);
                Close();
            });
            
            // 弹出该窗口说明没有打勾，所以不需要取消打勾
            noBtn.onClick.AddListener(() =>
            {
                Close();   
                UserInterfaceSystem.That.RemoveUIByName("UIChargeCtrl");
            });
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