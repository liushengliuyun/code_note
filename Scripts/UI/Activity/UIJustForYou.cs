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
    public class UIJustForYou : UIBase<UIJustForYou>
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


        public override void OnStart()
        {
            CloseButton.SetClick(Close);
            
            BuyButton.SetClick(OnBuyBtnClick);
            chargeGoodInfo = Root.Instance.RoomChargeInfo.AChargeInfo;
            
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
            var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.JustForYou);
            TimeText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            CheckActive();
        }

        private void CheckActive()
        {
            if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.JustForYou))
            {
                Close();
            }
        }

        void OnBuyBtnClick()
        {
            chargeGoodInfo.position = "JustForYou";
            MediatorRequest.Instance.Charge(chargeGoodInfo, ActivityType.JustForYou);
        }
        
        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }

        protected override void OnClose()
        {
            base.OnClose();
            //如果A活动没有购买,检查B活动是否打开
            if (Root.Instance.RoomChargeInfo.ShouldGenB)
            {
                MediatorRequest.Instance.SetRoomChargeBeginTime("B", () =>
                {
                    UserInterfaceSystem.That.ShowUI<UIBestOffer>(new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.Trigger
                    });
                });
            }
            
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.JustForYou
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
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_RoomChargeInfo, (sender, eventArgs) =>
            {
                CheckActive();
            });
        }
    }
}