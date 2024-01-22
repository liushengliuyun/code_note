using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace UI.Activity
{
    public class UISpecialOffer : UIBase<UISpecialOffer>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private MyButton buyBtn;
        [SerializeField] private MyButton closeBtn;
        
        [SerializeField] private MyText cashText;
        [SerializeField] private MyText bonusText;
        [SerializeField] private MyText diamondText;
        [SerializeField] private MyText priceText;
        [SerializeField] private MyText timeText;
        
        //[SerializeField] private MyText oldPriceText;

        private int chargeId;
        
        public override void OnStart()
        {
            if (Root.Instance.ChargeInfo.success_total > 0)
            {       
                Close();
                return;
            }

            if (Root.Instance.Role.SpecialOfferInfo!= null)
            {
                var chargeConfig = Root.Instance.SpecialOfferConfig[0];
                chargeId = chargeConfig.id;
                var diamondCountStr = chargeConfig.out_items["chips"].ToString();
                int diamondCount = diamondCountStr.ToInt();
                float cash = chargeConfig.amount;
                float bonus = chargeConfig.show_bonus;

                cashText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                bonusText.text = "$" + YZNumberUtil.FormatYZMoney(bonus.ToString());
                diamondText.text = diamondCount.ToString();

                priceText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                // if (oldPriceText != null)
                // {
                //     float old = cash + bonus;
                //     oldPriceText.text = "$" + YZNumberUtil.FormatYZMoney(old.ToString());
                // }

                buyBtn.SetClick(() =>
                {
                    // 充值流程
                    Charge_configsItem chargeItemTest = new Charge_configsItem();
                    chargeItemTest.id = chargeId;
                    chargeItemTest.bonusValue = chargeConfig.show_bonus;
                    chargeItemTest.amount = chargeConfig.amount.ToString();
                    chargeItemTest.position = "SpecialOffer";
                    LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
                    {
                        if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.SpecialOffer))
                            UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                    });
                });
            }
            
            closeBtn.SetClick(OnCloseBtnClick);
        }

        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.SpecialOffer
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

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Pass_Day, (sender, args) => { Close(); });
            // AddEventListener(GlobalEvent.Sync_Special_Offer, (sender, eventArgs) =>
            // {
            //     
            // });
        }

        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => {  });
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                .SetId(UIName);
        }

        private void Update()
        {
            if (Root.Instance.ChargeInfo.success_total > 0)
            {       
                Close();
                return;
            }
            
            var lessTime = Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow;
            timeText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            if (lessTime < 0)
            {
                Close();
            } 
        }
    }
}