using System;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Activity
{
    [Obsolete]
    public class UIMonthCard : UIBase<UIMonthCard>
    {
        public Text WeeklyBonusText;
        public Text MonthlyBonusText;

        public Text WeeklyCashText;
        public Text MonthlyCashText;

        public Text WeeklyChangeText;
        public Text MonthlyChangeText;

        public Text WeeklyLastText;

        public Text MonthlyLastText;

        public MyButton WeeklyBtn;
        public MyButton MonthlyBtn;
        public MyButton CloseBtn;

        //购买后消失
        public Transform WeeklyGetImmediatelyGroup;

        public Transform WeeklyGetDailyGroup;

        public Transform MonthlyGetImmediatelyGroup;

        public Transform MonthlyGetDailyGroup;

        public Transform WeeklyLockGroup;

        public Transform MonthlyLockGroup;

        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {
            CloseBtn.SetClick(OnCloseBtnClick);
            WeeklyBtn.SetClick(OnWeeklyBtnClick);
            MonthlyBtn.SetClick(OnMonthlyBtnClick);

            RegisterInterval(1f, SetBuyBtns);
        }

        private void OnWeeklyBtnClick()
        {
            if (Root.Instance.MonthCardInfo.IsWeeklyPassLock)
            {
                // 充值
                MediatorRequest.Instance.Charge(Root.Instance.MonthCardInfo.WeeklyChargeGoodInfo, ActivityType.MonthCard);
            }
            else
            {
                //领奖
                if (Root.Instance.MonthCardInfo.CanWeeklyPassClaim)
                {
                    MediatorRequest.Instance.GetMonthCardReward(1);
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_come_back_tomorrow"));
                }
            }
        }

        private void OnMonthlyBtnClick()
        {
            if (Root.Instance.MonthCardInfo.IsMonthlyPassLock)
            {
                // 充值
                MediatorRequest.Instance.Charge(Root.Instance.MonthCardInfo.MonthlyChargeGoodInfo, ActivityType.MonthCard);
            }
            else
            {
                //领奖
                if (Root.Instance.MonthCardInfo.CanMonthlyPassClaim)
                {
                    MediatorRequest.Instance.GetMonthCardReward(2);
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_come_back_tomorrow"));
                }
            }
        }

        public override void InitVm()
        {
            var data = Root.Instance.MonthCardInfo;
            WeeklyLockGroup.SetActive(data.IsWeeklyPassLock);
            MonthlyLockGroup.SetActive(data.IsMonthlyPassLock);

            WeeklyGetImmediatelyGroup.SetActive(data.IsWeeklyPassLock);
            MonthlyGetImmediatelyGroup.SetActive(data.IsMonthlyPassLock);
            //周卡未购买 ， 或者未领取
            if (data.CanWeeklyPassClaim || data.IsWeeklyPassLock)
            {
                WeeklyChangeText.text = I18N.Get("key_get_daily");
            }
            else
            {
                WeeklyChangeText.text = I18N.Get("key_come_back_tomorrow");
            }

            if (data.CanMonthlyPassClaim || data.IsMonthlyPassLock)
            {
                MonthlyChangeText.text = I18N.Get("key_get_daily");
            }
            else
            {
                MonthlyChangeText.text = I18N.Get("key_come_back_tomorrow");
            }

            WeeklyGetDailyGroup.transform.localPosition =
                data.IsWeeklyPassLock ? new Vector3(-120, -38, 0) : new Vector3(-120, 20, 0);

            MonthlyGetDailyGroup.transform.localPosition =
                data.IsMonthlyPassLock ? new Vector3(-120, -38, 0) : new Vector3(-120, 20, 0);

            WeeklyLastText.transform.parent.SetActive(!data.IsWeeklyPassLock);

            MonthlyLastText.transform.parent.SetActive(!data.IsMonthlyPassLock);

            WeeklyLastText.text = I18N.Get("key_last_days", data.time_list.week);

            MonthlyLastText.text = I18N.Get("key_last_days", data.time_list.month);

            WeeklyCashText.text = I18N.Get("key_money_count", data.WeekBuyBonus);
            MonthlyCashText.text = I18N.Get("key_money_count", data.MonthBuyBonus);

            WeeklyBonusText.text = I18N.Get("key_money_count", data.WeeklyChargeGoodInfo.amount);
            MonthlyBonusText.text = I18N.Get("key_money_count", data.MonthlyChargeGoodInfo.amount);

            SetBuyBtns();
        }

        private void SetBuyBtns()
        {
            MonthCardInfo data = Root.Instance.MonthCardInfo;
            if (data == null)
            {
                return;
            }

            //还没有购买
            if (data.IsWeeklyPassLock)
            {
                WeeklyBtn.title = I18N.Get("key_money_count", data.WeeklyChargeGoodInfo.amount);
            }
            else
            {
                //可领取
                if (data.CanWeeklyPassClaim)
                {
                    WeeklyBtn.title = I18N.Get("key_go_claim");
                }
                else
                {
                    WeeklyBtn.title = TimeUtils.Instance.FormatTimeToTomorrow();
                }
            }

            if (data.IsMonthlyPassLock)
            {
                MonthlyBtn.title = I18N.Get("key_money_count", data.MonthlyChargeGoodInfo.amount);
            }
            else
            {
                //可领取
                if (data.CanMonthlyPassClaim)
                {
                    MonthlyBtn.title = I18N.Get("key_go_claim");
                }
                else
                {
                    MonthlyBtn.title = TimeUtils.Instance.FormatTimeToTomorrow();
                }
            }
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.SYNC_MONTH_CARD_INFO, (sender, eventArgs) => { Refresh(); });
        }

        protected override void OnAnimationIn()
        {
            var root = transform.GetChild(0);
            var child = root.GetChild(1);
            child.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            child.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        
        
        protected override void OnAnimationOut()
        {
            var uimain = UserInterfaceSystem.That.Get<UIMain>();
            var root = transform.GetChild(0);
            if (!uimain.StoreToggle.isOn)
            {
                root.GetChild(0).SetActive(false);
                var panel = root.GetChild(1);
                var tween1 = panel.DOScale(Vector3.one * 0.1f, 0.5f).SetEase(Ease.OutCubic);

                var tween2 = panel.DOMove(uimain.StoreToggle.transform.position, 0.5f);
                DOTween.Sequence().Append(tween1).Join(tween2).SetId(UIName);
            }
            else
            {
                root.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                    .SetId(UIName);
            }
        }
    }
}