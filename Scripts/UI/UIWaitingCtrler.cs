using System;
using System.Collections;
using System.Collections.Generic;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;

namespace UI
{
    public class UIWaitingCtrler : UIBase<UIWaitingCtrler>
    {
        public override UIType uiType { get; set; } = UIType.Overlay;

        public override void InitEvents()
        {
        }

        void Awake()
        {
            base.Awake();
        }

        public override void OnStart()
        {
            
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        

        // private static UIWaitingCtrler inst;
        //
        // public static UIWaitingCtrler Shared()
        // {
        //     return inst;
        // }
    }
}