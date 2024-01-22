using System;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Mono
{
    public class SpecialGiftMonoB : MonoBehaviour
    {
        public Text TimeText;
        public MyButton selfBtn;
        public MyButton buyBtn;

        public Text buyText;
        public Text cashText;
        public Text bonusText;
        public Text diamondText;
        public Text discountText;
        
        public Text remainText;

        public void Init()
        {
            var chargeInfo = Root.Instance.Role.specialGiftInfo.charge_info;

            if (chargeInfo == null)
            {
                return;
            }

            buyBtn.SetClick(() =>
            {
                Buy(chargeInfo);
            });

            selfBtn.SetClick(() =>
            {
                Buy(chargeInfo);
            });
            
            float cash = chargeInfo.amount.ToFloat();
            float bonus = chargeInfo.show_bonus;
            
            buyText.text =I18N.Get("key_money_count", YZNumberUtil.FormatYZMoney(chargeInfo.amount));
            cashText.text = I18N.Get("key_money_count", YZNumberUtil.FormatYZMoney(chargeInfo.amount));
            bonusText.text = I18N.Get("key_money_count", 
                YZNumberUtil.FormatYZMoney(chargeInfo.show_bonus.ToString()));
            diamondText.text = chargeInfo.out_items["chips"].ToString();
            
            discountText.text = YZNumberUtil.FormatYZMoney((bonus / cash * 100).ToString()) + "%";
        }

        void Buy(charge_info data)
        {
            Charge_configsItem chargeItemTest = new Charge_configsItem();
            chargeItemTest.id = data.id;
            chargeItemTest.bonusValue = data.show_bonus;
            chargeItemTest.amount = data.amount.ToString();
            LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1,
                () =>
                {
                    if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.SpecialGift))
                        UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                });
        }

        private void Update()
        {
            if (Root.Instance.Role.specialGiftInfo == null)
            {
                return;
            }
            var lessTime = Root.Instance.Role.specialGiftInfo.special_gift_end_time - TimeUtils.Instance.UtcTimeNow;
            TimeText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            if (lessTime < 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            int remainChance = Root.Instance.Role.specialGiftInfo.special_gift_today_chance;
            int allChance = 2;
            remainText.text = YZString.Format(I18N.Get("key_special_gift_remain"), 
                remainChance, allChance);
        }
    }
}