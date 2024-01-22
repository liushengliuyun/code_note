using System;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;

namespace UI
{
    public class UIBingoPropRule : UIBase<UIBingoPropRule>
    {
        
        [SerializeField] private MyButton OkBtn;

        public override UIType uiType { get; set; } = UIType.Window;

        public override void InitEvents()
        {
            
        }

        public override void OnStart()
        {
            OkBtn.SetClick(Close);
        }

        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }
    }
}