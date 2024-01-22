using Core.Services.UserInterfaceService.Internal;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UnityEngine.UI;
using Utils;

namespace UI.Activity
{
    public class UIBestOffer : UIBase<UIBestOffer>
    {
        public Text CashText;
        public Text BonusText;
        public Text DiamondText;
        public Text TimeText;
        public Text MoreText;
        public MyButton BuyButton;
        public MyButton CloseButton;
        
        public override UIType uiType { get; set; } = UIType.Window;

        private ChargeGoodInfo chargeGoodInfo;
        private int chargeId;

        protected override void OnClose()
        {
            base.OnClose();

            YZFunnelUtil.YZFunnelActivityPop(ActivityType.BestOffer
                , charge_id: chargeId
                , isauto: GetOpenType().IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }

        private ActivityEnterType GetOpenType()
        {
            return GetTableValue<ActivityEnterType>("ActivityEnterType");
        }
        
        public override void OnStart()
        {
            CloseButton.SetClick(Close);
            BuyButton.SetClick(OnBuyBtnClick);
            
            chargeGoodInfo = Root.Instance.RoomChargeInfo.BChargeInfo;

            chargeId = chargeGoodInfo.id;
            MoreText.text = chargeGoodInfo.MoreValue + "%";
            CashText.text = chargeGoodInfo.amount.ToString();
            BonusText.text = chargeGoodInfo.ShowBonus.ToString();
            DiamondText.text = chargeGoodInfo.ShowGems.ToString();
            
            BuyButton.title = I18N.Get("key_money_count", chargeGoodInfo.amount.ToString());
            RegisterInterval(1f, Interval, true);
        }

        
        private void Interval()
        {
            var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.BestOffer);
            TimeText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            
            CheckActive();
        }

        private void CheckActive()
        {
            if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.BestOffer))
            {
                Close();
            }
        }

        void OnBuyBtnClick()
        {
            chargeGoodInfo.position = "BestOffer";
            MediatorRequest.Instance.Charge(chargeGoodInfo, ActivityType.BestOffer);
        }
        
        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }

        public override void InitEvents()
        {
           AddEventListener(GlobalEvent.Sync_RoomChargeInfo, (sender, eventArgs) =>
           {
               CheckActive();
           });
        }
    }
}