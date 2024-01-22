using System;
using Core.Extensions;
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
using Utils;
using Text = UnityEngine.UI.Text;

namespace UI.Activity
{
    public class UIMonthCardNew : UIBase<UIMonthCardNew>
    {
        public Text WeeklyBonusText;
        public Text MonthlyBonusText;

        public Text WeeklyCashText;
        public Text MonthlyCashText;

        public Text WeeklyChangeText;
        public Text MonthlyChangeText;

        public Text WeeklyLastText;

        public Text MonthlyLastText;
        
        public Text CashLeftText1;
        /// <summary>
        /// 周卡
        /// </summary>
        public Text CashRightText1;
        public Text MoreText1;
        
        public Text CashLeftText2;
        /// <summary>
        /// 周卡2
        /// </summary>
        public Text CashRightText2;
        public Text MoreText2;

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

        public GameObject Card1;
        public GameObject Card2;


        private InfiniteWeekConfig _config;
        private int _currentChargeCard;

        public override UIType uiType { get; set; } = UIType.Window;

        int GetCardNum()
        {
            return YZDataUtil.GetYZInt(YZConstUtil.YZBuyWeekCardNum, 0);
        }
        
        void RefreshCards()
        {
            if (Root.Instance.WeekInfo.buy_level == 0 || Root.Instance.WeekInfo.IsWeeklyPassLock)
            {
                Card1.SetActive(true);
                Card2.SetActive(true);
            }
            else
            {
                int cardNum = GetCardNum();
                if (cardNum != 0)
                {
                    Card1.SetActive(cardNum == 1);
                    Card2.SetActive(cardNum == 2);
                }
                else
                {
                    int buy_level = Root.Instance.WeekInfo.buy_level;
                    if (buy_level == 1 || Root.Instance.WeekInfo.buy_level == 3 ||
                        Root.Instance.WeekInfo.buy_level == 4)
                    {
                        Card1.SetActive(true);
                        Card2.SetActive(false);
                    }
                    else
                    {
                        Card1.SetActive(false);
                        Card2.SetActive(true);
                    }
                }
            }
        }

        public override void OnStart()
        {
            var data = Root.Instance.WeekInfo;
            _config = Root.Instance.WeekConfig;
            if (data == null || _config == null)
            {
                Close();
                return;
            }

            RefreshCards();

            CloseBtn.SetClick(OnCloseBtnClick);
            WeeklyBtn.SetClick(OnWeeklyBtnClick);
            MonthlyBtn.SetClick(OnMonthlyBtnClick);
            RegisterInterval(1f, SetBuyBtns);
            
            

            int level = data.level;
            if (level == 0)
                level = 1;
            if (level == 5)
                level = 4;
            if (level == 6)
                level = 5;
            int next = level == 1 ? 5 : 1;
            if (level == 2) next = 0;

            if (Card1.activeSelf) // 周卡1显示
            {
                WeeklyLockGroup.SetActive(data.IsWeeklyPassLock);
                WeeklyGetImmediatelyGroup.SetActive(data.IsWeeklyPassLock);
                //周卡未购买 ， 或者未领取
                if (data.CanWeeklyPassClaim || data.IsWeeklyPassLock)
                {
                    WeeklyChangeText.text = I18N.Get("key_get_daily");
                }
                else
                {
                    WeeklyChangeText.text = I18N.Get("key_come_back_tomorrow");
                }
                WeeklyGetDailyGroup.transform.localPosition =
                    data.IsWeeklyPassLock ? new Vector3(-100, -30, 0) : new Vector3(-100, 10, 0);
                WeeklyLastText.text = I18N.Get("key_last_days", 
                    data.IsWeeklyPassLock? _config.bonus_info[level].day : data.time_list.week);
            
            
                WeeklyBonusText.text = I18N.Get("key_money_count", 
                    data.IsWeeklyPassLock?  _config.bonus_info[level].week:
                    _config.bonus_info[data.buy_level].week);
                //
                WeeklyCashText.text = I18N.Get("key_money_count", _config.charge_info.week[level-1].amount);
                CashLeftText1.text = "$" + _config.charge_info.week[level - 1].amount + " +";
                CashRightText1.text = "$" + (_config.bonus_info[level].day * _config.bonus_info[level].week);
                MoreText1.text = Math.Round((_config.bonus_info[level].day * _config.bonus_info[level].week)
                    / _config.charge_info.week[level - 1].amount * 100) + "%More";
            }

            if (Card2.activeSelf) // 周卡2显示
            {
                MonthlyLockGroup.SetActive(data.IsWeeklyPassLock);
                MonthlyGetImmediatelyGroup.SetActive(data.IsWeeklyPassLock);
                if (data.CanWeeklyPassClaim || data.IsWeeklyPassLock)
                {
                    MonthlyChangeText.text = I18N.Get("key_get_daily");
                }
                else
                {
                    MonthlyChangeText.text = I18N.Get("key_come_back_tomorrow");
                }

                MonthlyGetDailyGroup.transform.localPosition =
                    data.IsWeeklyPassLock ? new Vector3(-100, -30, 0) : new Vector3(-100, 10, 0);

                var fixLevel = data.IsWeeklyPassLock ? level + next : data.buy_level;

                MonthlyLastText.text = I18N.Get("key_last_days",
                    data.IsWeeklyPassLock ? _config.bonus_info[fixLevel].day : data.time_list.week);
                MonthlyBonusText.text = I18N.Get("key_money_count",
                    data.IsWeeklyPassLock ? _config.bonus_info[fixLevel].week : _config.bonus_info[data.buy_level].week);
                MonthlyCashText.text = I18N.Get("key_money_count", _config.charge_info.week[level].amount);
                CashLeftText2.text = "$" + _config.charge_info.week[fixLevel - 1].amount + " +";
                CashRightText2.text = "$" + (_config.bonus_info[fixLevel].day * _config.bonus_info[fixLevel].week);
                MoreText2.text = Math.Round((_config.bonus_info[fixLevel].day * _config.bonus_info[fixLevel].week)
                    / _config.charge_info.week[level].amount * 100) + "%More";
            }
            
            SetBuyBtns();
        }

        private void OnWeeklyBtnClick()
        {
            _currentChargeCard = 1;
            int level = Root.Instance.WeekInfo.level;
            if (level == 0)
                level = 1;
            if (level == 5)
                level = 4;
            if (Root.Instance.WeekInfo.IsWeeklyPassLock)
            {
                // 充值
                MediatorRequest.Instance.Charge(_config.charge_info.week[level-1], ActivityType.MonthCard);
            }
            else
            {
                //领奖
                if (Root.Instance.WeekInfo.CanWeeklyPassClaim)
                {
                    MediatorRequest.Instance.GetWeekCardReward();
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_come_back_tomorrow"));
                }
            }
        }

        private void OnMonthlyBtnClick()
        {
            _currentChargeCard = 2;
            int level = Root.Instance.WeekInfo.level;
            if (level == 0)
                level = 1;
            if (level == 5)
                level = 4;
            // 首充的礼包换成新的level
            if (level == 1)
                level = 5;
            if (Root.Instance.WeekInfo.IsWeeklyPassLock)
            {
                // 充值
                MediatorRequest.Instance.Charge(_config.charge_info.week[level], ActivityType.MonthCard);
            }
            else
            {
                //领奖
                if (Root.Instance.WeekInfo.CanWeeklyPassClaim)
                {
                    MediatorRequest.Instance.GetWeekCardReward();
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_come_back_tomorrow"));
                }
            }
        }

        public override void InitVm()
        {
        }

        // private int chargeId;
        // private ActivityEnterType activityEnterTYpe;
        protected override void OnClose()
        {
            base.OnClose();
            
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.MonthCard
                // , charge_id: chargeId
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
        
        private void SetBuyBtns()
        {
            var data = Root.Instance.WeekInfo;
            if (data == null || _config == null)
            {
                return;
            }
            
            int level = data.level;
            if (level == 0)
                level = 1;
            if (level == 5)
                level = 4;

            RefreshCards();
            
            //还没有购买
            if (data.IsWeeklyPassLock)
            {
                WeeklyBtn.title = I18N.Get("key_money_count", 
                    _config.charge_info.week[level - 1].amount);
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
            
            if (data.IsWeeklyPassLock)
            {
                MonthlyBtn.title = I18N.Get("key_money_count", _config.charge_info.week[level].amount);
            }
            else
            {
                //可领取
                if (data.CanWeeklyPassClaim)
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
            AddEventListener(GlobalEvent.SYNC_WEEK_CARD_INFO, (sender, eventArgs) => { OnStart(); });
            AddEventListener(GlobalEvent.CHARGE_SUCCESS, (sender, eventArgs) =>
            {
                YZDebug.Log("新周卡购买成功 card = " + _currentChargeCard);
                
                Card1.SetActive(_currentChargeCard == 1);
                Card2.SetActive(_currentChargeCard == 2);
            });
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
#if DAI_TEST
            if(true)
 #else
                  if (!uimain.StoreToggle.isOn)
#endif
          
            {
                var mask = root.GetChild(0);
                mask.SetActive(false);
                var panel = root.GetChild(1);
                // var tween1 = panel.DOScale(Vector3.one * 0.1f, 0.5f).SetEase(Ease.OutCubic);
                //
                // var tween2 = panel.DOMove(uimain.StoreToggle.transform.position, 0.5f);
                // DOTween.Sequence().Append(tween1).Join(tween2).SetId(UIName);
                
                //0.5f时间长度是为了让动画播完 ui queue能连续用
                DOTween.Sequence().SetDelay(0.5f).SetId(UIName);
                UIEffectUtils.Instance.CaptureAndShrink(panel, UICanvas, uimain.StoreToggle.transform);
            }
            else
            {
                root.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                    .SetId(UIName);
            }
        }
    }
}