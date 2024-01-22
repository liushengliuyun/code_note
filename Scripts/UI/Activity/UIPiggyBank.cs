using System;
using Core.Extensions;
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
using Reactive.Bindings;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Activity
{
    public class UIPiggyBank : UIBase<UIPiggyBank>
    {
        public Button CloseBtn;

        public MyButton BuyBtn;

        public GameObject showGameObject;

        public Transform FullTrans;

        public Text TimeText;

        public Transform NotFullTrans;
        public override UIType uiType { get; set; } = UIType.Window;

        public SkeletonGraphic spine;

        public override void OnStart()
        {
            CloseBtn.onClick.AddListener(OnCloseBtnClick);

            var entryType = GetArgsByIndex<ActivityEnterType>(0);
            activityEnterTYpe = entryType;
            MediatorActivity.Instance.AddPopCount(ActivityType.PiggyBank, entryType);
            RegisterInterval(1f, SetTimeText, true);
            
            MediatorUnlock.Instance.RecordShowUI(ClassType);
        }

        void SetTimeText()
        {
            var activityLessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.PiggyBank);
            TimeText.text = TimeUtils.Instance.ToHourMinuteSecond(activityLessTime);

            if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank))
            {
                Close();
            }
        }

        void SetFullState(bool isFull)
        {
            // FullTrans.SetActive(isFull);
            // NotFullTrans.SetActive(!isFull);
            string animationName = isFull ? "idea2" : "idea";
            spine.AnimationState.SetAnimation(0, animationName, true);
        }

        enum vname
        {
            /// <summary>
            /// 充值挡位
            /// </summary>
            gear
        }

        public override void InitVm()
        {
            vm[vname.gear.ToString()] = new ReactivePropertySlim<int>(Root.Instance.PiggyBankInfo.piggy_level);
        }

        public override void InitBinds()
        {
            vm[vname.gear.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                var cashText = showGameObject.FindChild<Text>("cash group/cash text");

                var bonusText = showGameObject.FindChild<Text>("bonus group/bonusText");

                cashText.text = I18N.Get("key_money_count", Root.Instance.PiggyBankInfo.Cash);

                bonusText.text = I18N.Get("key_money_count", Root.Instance.PiggyBankInfo.AllBonus);

                SetFullState(Root.Instance.PiggyBankInfo.IsFull);

                BuyBtn.title = I18N.Get("key_piggy_bank_price", Root.Instance.PiggyBankInfo.Cash).ToUpper();
                var data = Root.Instance.PiggyBankInfo.ChargeInfo;
                chargeId = data.id;
                BuyBtn.SetClick(() =>
                {
                    Charge_configsItem chargeItemTest = new Charge_configsItem();
                    chargeItemTest.id = data.id;
                    chargeItemTest.bonusValue = Root.Instance.PiggyBankInfo.AllBonus;
                    chargeItemTest.amount = data.amount.ToString();
                    chargeItemTest.position = "PiggyBank";
                    LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1,
                        () =>
                        {
                            if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.PiggyBank))
                                UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                        });
                });
            });
        }

        private int chargeId;
        private ActivityEnterType activityEnterTYpe;
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.PiggyBank
                , charge_id: chargeId
                , isauto: activityEnterTYpe.IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.SYNC_PIGGY_BANK, (sender, eventArgs) =>
            {
                if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank))
                {
                    Close();
                }
                else
                {
                    Refresh();
                }
            });
        }
    }
}