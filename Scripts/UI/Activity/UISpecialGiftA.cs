using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace UI.Activity
{
    public class UISpecialGiftA : UIBase<UISpecialGiftA>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private MyButton buyBtn;
        [SerializeField] private MyButton closeBtn;
        
        [SerializeField] private MyText cashText;
        [SerializeField] private MyText bonusText;
        [SerializeField] private MyText diamondText;
        [SerializeField] private MyText discountText;
        [SerializeField] private MyText priceText;
        [SerializeField] private MyText remainText;
        [SerializeField] private MyText timeText;
        
        [SerializeField] private MyText oldPriceText;

        public override void OnStart()
        {
            var entryType = GetArgsByIndex<ActivityEnterType>(0);
            activityEnterTYpe = entryType;
            MediatorActivity.Instance.AddPopCount(ActivityType.SpecialGift, entryType);
            
            // 更新信息
            MediatorRequest.Instance.UpdateSpecialGiftInfo();
            
            if (Root.Instance.Role.specialGiftInfo!= null && 
                Root.Instance.Role.specialGiftInfo.charge_info != null)
            {
                var chargeInfo = Root.Instance.Role.specialGiftInfo.charge_info;
                var diamondCountStr = chargeInfo.out_items["chips"].ToString();
                int diamondCount = diamondCountStr.ToInt();
                float cash = chargeInfo.amount.ToFloat();
                float bonus = chargeInfo.show_bonus;

                cashText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                bonusText.text = "$" + YZNumberUtil.FormatYZMoney(bonus.ToString());
                diamondText.text = diamondCount.ToString();

                discountText.text = YZNumberUtil.FormatYZMoney((bonus / cash * 100).ToString()) + "%";
                priceText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                if (oldPriceText != null)
                {
                    float old = cash + bonus;
                    oldPriceText.text = "$" + YZNumberUtil.FormatYZMoney(old.ToString());
                }

                //remainText.text = "Available:" + Root.Instance.Role.specialGiftInfo.special_gift_today_chance;
                int remainChance = Root.Instance.Role.specialGiftInfo.special_gift_today_chance;
                int allChance = 2;
                remainText.text = YZString.Format(I18N.Get("key_special_gift_remain"), 
                    remainChance, allChance);
                chargeId = chargeInfo.id;
                buyBtn.SetClick(() =>
                {
                    // 充值流程
                    Charge_configsItem chargeItemTest = new Charge_configsItem();
                    chargeItemTest.id = chargeInfo.id;
                    chargeItemTest.bonusValue = chargeInfo.show_bonus;
                    chargeItemTest.amount = chargeInfo.amount;
                    chargeItemTest.position = "SpecialGiftA";
                    LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
                    {
                        if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.SpecialGift))
                            UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                    });
                });
            }
            
            closeBtn.SetClick(OnCloseBtnClick);
        }

        private int chargeId;
        private ActivityEnterType activityEnterTYpe;
        
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.SpecialOffer
                , charge_id: chargeId
                , isauto: activityEnterTYpe.IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
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
            AddEventListener(GlobalEvent.Sync_Special_Gift, (sender, eventArgs) =>
            {
                int remainChance = Root.Instance.Role.specialGiftInfo.special_gift_today_chance;
                int allChance = 2;
                remainText.text = YZString.Format(I18N.Get("key_special_gift_remain"), 
                    remainChance, allChance);
                
                if (remainChance == 0)
                    Close();
            });
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
            var lessTime = Root.Instance.Role.specialGiftInfo.LessTime;
            timeText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            if (lessTime < 0)
            {
                Close();
            } 
        }
    }
}