using System.Collections;
using System.Collections.Generic;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;

namespace UI
{
    public class UIWaitNet : UIBase<UIWaitNet>
    {
        public override UIType uiType { get; set; } = UIType.Overlay;

        private int diceId = 0;
        [SerializeField] private List<UGUISpriteAnimation> dices;
        public override void InitEvents()
        {
        }

        new void Awake()
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
        
    }
}