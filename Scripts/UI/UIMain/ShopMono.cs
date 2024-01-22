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
using DataAccess.Utils.Static;
using UI.Activity;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityTimer;

namespace UI
{
    public class ShopMono : MonoBehaviour
    {
        [SerializeField] private Transform Goods;

        //临时
        [FormerlySerializedAs("MonthCardIcon")] [SerializeField]
        private MyButton MonthCardGroup;

        [SerializeField] private MyButton MonthCardBtn;

        /// <summary>
        /// 每日金币奖励【废弃】
        /// </summary>
        [SerializeField] private MyButton GetDailyRewardBtn;

        [SerializeField] private MyButton ShowSignBtn;
        [SerializeField] private MyButton dailyRewardSpine;
        [SerializeField] private MyButton ShowSignSpine;

        /// <summary>
        /// 商场直售礼包
        /// </summary>
        [SerializeField] private Transform GiftParent;

        [SerializeField] private MyButton HotSelling;
        [SerializeField] private MyButton OtherGoods;

        [SerializeField] private Transform HotSellingPanel;
        [SerializeField] private Transform OtherGoodsPanel;

        private void Start()
        {
            Refresh();

            this.AttachTimer(1f, RefreshFreeGift, isLooped: true, useRealTime: true);

            if (Root.Instance.DailyRewardList is not { Count: > 0 }) return;

            MonthCardGroup.SetClick(OnMonthCardClick);
            MonthCardBtn.SetClick(OnMonthCardClick);

            var dailyReward = Root.Instance.DailyRewardList[0];

            void TryClaimDailyReward()
            {
                if (Root.Instance.DailyRewardChance > 0)
                {
                    MediatorRequest.Instance.ClaimDailyReward(dailyReward.order, RefreshFreeGift);
                }
            }

            dailyRewardSpine.SetClick(TryClaimDailyReward);
            // GetDailyRewardBtn.SetClick(TryClaimDailyReward);

            ShowSignBtn.SetClick(ShowUISign);
            ShowSignSpine.SetClick(ShowUISign);

            Interval();
            this.AttachTimer(1f, Interval, isLooped: true);

            //如果hotSelling有活动 todo 
            if (MediatorActivity.Instance.HaveHotSellings())
            {
                ChangePanelToHotSelling();
            }
            else
            {
                ChangePanelToOtherGoods();
            }


            HotSelling.SetClick(() => { ChangePanelToHotSelling(); });

            OtherGoods.SetClick(() => { ChangePanelToOtherGoods(); });
        }

        private void Interval()
        {
            var formatTimeToTomorrow = TimeUtils.Instance.FormatTimeToTomorrow();
            
            if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.MonthCard))
            {
                MonthCardGroup.SetActive(false);
            }
            else
            {
                MonthCardGroup.SetActive(Root.Instance.WeekInfo != null);

                if (Root.Instance.WeekInfo != null && !Root.Instance.WeekInfo.NotBuy)
                {
                    MonthCardBtn.SetActive(true);
                    if (!Root.Instance.WeekInfo.NotBuy)
                    {
                        MonthCardBtn.title = Root.Instance.WeekInfo.HaveRewardToClaim
                            ? I18N.Get("key_go_claim")
                            : formatTimeToTomorrow;
                    }
                }
                else
                {
                    MonthCardBtn.SetActive(false);
                }
            }
            
            ShowSignBtn.title = formatTimeToTomorrow;
        }

        private void ShowUISign()
        {
            UserInterfaceSystem.That.ShowUI<UISign>(new GameData()
            {
                ["isjustShow"] = true
            });
        }

        private void ChangePanelToOtherGoods()
        {
            HotSellingPanel.SetActive(false);
            OtherGoodsPanel.SetActive(true);

            HotSelling.gameObject.FindChild("Text").SetActive(false);
            HotSelling.gameObject.FindChild("OffText").SetActive(true);

            OtherGoods.gameObject.FindChild("Text").SetActive(true);
            OtherGoods.gameObject.FindChild("OffText").SetActive(false);

            HotSelling.gameObject.FindChild("OnImage").SetActive(false);
            OtherGoods.gameObject.FindChild("OnImage").SetActive(true);
        }

        private void ChangePanelToHotSelling()
        {
            HotSellingPanel.SetActive(true);
            OtherGoodsPanel.SetActive(false);
            //显示字颜色
            HotSelling.gameObject.FindChild("Text").SetActive(true);
            HotSelling.gameObject.FindChild("OffText").SetActive(false);

            OtherGoods.gameObject.FindChild("Text").SetActive(false);
            OtherGoods.gameObject.FindChild("OffText").SetActive(true);
            //显示外框
            HotSelling.gameObject.FindChild("OnImage").SetActive(true);
            OtherGoods.gameObject.FindChild("OnImage").SetActive(false);
        }

        private void InitDirectSale()
        {
            if (Root.Instance.ShopConfig == null)
            {
                return;
            }

            for (int i = 0; i < Root.Instance.ShopConfig.Count; i++)
            {
                var data = Root.Instance.ShopConfig[i];
                var child = GiftParent.GetChild(i).GetComponent<GiftMono>();
                var giftBtn = child.Button;
                giftBtn.SetClick(() => { Charge(data); });
                child.Icon.sprite = MediatorBingo.Instance.GetSpriteByUrl($"uishop/gift_icon_{i + 1}");


                child.CashText.text = I18N.Get("key_money_count", GameUtils.TocommaStyle(data.money));

                child.BonusText.text = I18N.Get("key_bonus_count", GameUtils.TocommaStyle(data.bonus));
            }
        }

        private void Charge(ShopInfo data)
        {
            Charge_configsItem chargeItemTest = new Charge_configsItem();
            chargeItemTest.id = data.id;
            chargeItemTest.bonusValue = data.bonus;
            chargeItemTest.amount = data.money.ToString();
            chargeItemTest.position = "Shop";
            LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1,
                () =>
                {
                    if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.StoreCharge))
                        UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                });
        }

        public void Refresh()
        {
            InitDirectSale();

            // RefreshFreeGift();
        }

        public void RefreshFreeGift()
        {
            dailyRewardSpine.Gray = Root.Instance.DailyRewardChance <= 0;
            GetDailyRewardBtn.Gray = Root.Instance.DailyRewardChance <= 0;


            GetDailyRewardBtn.gameObject.FindChild("RedPoint/RedPointGroup")
                .SetActive(Root.Instance.DailyRewardChance > 0);

            if (Root.Instance.DailyRewardChance > 0)
            {
                GetDailyRewardBtn.title = I18N.Get("key_free");
            }
            else
            {
                GetDailyRewardBtn.title = TimeUtils.Instance.FormatTimeToTomorrow();
            }
        }

        private void OnMonthCardClick()
        {
            MediatorRequest.Instance.GetWeekCardInfo();
            UserInterfaceSystem.That.ShowUI<UIMonthCardNew>(new GameData()
            {
                ["ActivityEnterType"] = ActivityEnterType.Click
            });
        }
    }
}