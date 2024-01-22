using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UnityEngine.UI;
using UnityTimer;

namespace UI
{
    public class UINewPlayerBonus : UIBase<UINewPlayerBonus>
    {
        public Text CoinText;

        public Text GemsText;

        public MyButton MaskBtn;

        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {
            MaskBtn.SetClick(Close);
            if (Root.Instance.NewPlayerBonus != null) 
            {
                var success =  Root.Instance.NewPlayerBonus.TryGetValue("coin", out var coinCount);
                if (success)
                {
                    CoinText.text = coinCount.ToString();
                }
                var success1 =  Root.Instance.NewPlayerBonus.TryGetValue("chips", out var diamondCount);
                if (success1)
                {
                    GemsText.text = diamondCount.ToString();
                }
            }

            MediatorItem.Instance.SetItemText(Const.Chips, GemsText);
            MediatorItem.Instance.SetItemText(Const.Coin, CoinText);
            
            Timer.Register(5f, Close, autoDestroyOwner: this);
        } 

        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }
        
        public override void InitEvents()
        {
            
        }

        protected override void OnClose()
        {
            base.OnClose();
            MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.BEFORE_ENTER_ROOM);
        }
    }
}