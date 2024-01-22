//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.AudioService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using Reactive.Bindings;
using Spine.Unity;
using UI.Animation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;
using Text = UnityEngine.UI.Text;

namespace UI
{
    public class UIWheel : UIBase<UIWheel>
    {
        #region UI Variable Statement

        [SerializeField] private Text DescText;


        [SerializeField] private HorizontalScrollSnap horizontalScrollSnap;

        [SerializeField] private Transform Content;

        [SerializeField] private Transform Effect;

        [SerializeField] private Button button_CloseBtn;

        [SerializeField] private Toggle toggleFree;

        [SerializeField] private Toggle toggleBigPay;

        [SerializeField] private Toggle toggleSmallPay;

        // [SerializeField] private Button DiamondBtn;

        /// <summary>
        /// 免费场的
        /// </summary>
        [SerializeField] private Text FreeTicketText;

        // [SerializeField] private Transform Rewards;

        [SerializeField] private Transform[] Lights;

        #endregion

        private Dictionary<WheelType, GameObject> Type2Gameobject;
        public override UIType uiType { get; set; } = UIType.Window;

        /// <summary>
        /// 灯效
        /// </summary>
        void InitLights()
        {
            foreach (var lightParent in Lights)
            {
                int timeCounter = 0;

                void Normal()
                {
                    for (int i = 0; i < lightParent.childCount; i++)
                    {
                        lightParent.GetChild(i).GetChild(1).SetActive(i % 2 == timeCounter % 2);
                    }
                }

                Normal();

                RegisterInterval(0.8f, () =>
                {
                    if (IsWheelAnima)
                    {
                        return;
                    }

                    timeCounter++;

                    Normal();
                });

                //动起来的
                int moveTimeCounter = 0;
                int moveTimeCounter1 = 12;

                RegisterInterval(0.05f, () =>
                {
                    if (!IsWheelAnima)
                    {
                        return;
                    }

                    moveTimeCounter++;
                    moveTimeCounter1++;
                    var childCount = lightParent.childCount;
                    for (int i = 0; i < lightParent.childCount; i++)
                    {
                        var lightParentChildCount = moveTimeCounter % childCount;
                        var lightParentChildCount1 = (moveTimeCounter + 1) % childCount;
                        var lightParentChildCount4 = (moveTimeCounter + 2) % childCount;

                        var lightParentChildCount2 = (moveTimeCounter1) % childCount;
                        var lightParentChildCount3 = (moveTimeCounter1 + 1) % childCount;
                        var lightParentChildCount5 = (moveTimeCounter1 + 2) % childCount;

                        var visible = i == lightParentChildCount || i == lightParentChildCount1 ||
                                      i == lightParentChildCount2
                                      || i == lightParentChildCount3
                                      || i == lightParentChildCount4
                                      || i == lightParentChildCount5;
                        if (childCount > 20)
                        {
                            lightParent.GetChild(i).Find("on").SetActive(visible);
                        }
                        else
                        {
                            lightParent.GetChild(i).Find("on").SetActive(lightParentChildCount == i);
                        }
                    }
                });
            }
        }

        private void StartWheel(object sender)
        {
            var (order, whType) = sender is (int, WheelType) ? ((int, WheelType))sender : (0, WheelType.Free);

            if (whType == MediatorActivity.Instance.GetTargetWheelType())
            {
                SavePopedPayWheelTime();
            }

            var reward = GetReward(order);

            if (reward == null)
            {
                IsWheelAnima = false;
                return;
            }

            //可能断网重连 , 防御
            IsWheelAnima = true;
            var rewardsCom = GetCurrentRewardCom();
            var oldOrder = (int)rewardsCom.transform.localEulerAngles.z / 45 + 1;
            var diff = order - oldOrder % 8;
            var angle = (oldOrder + diff - 1) * 45 - 360 * 5 + 22.5f;

            AudioSystem.That.PlaySound(SoundPack.Wheel);
            rewardsCom.transform.DORotate(new Vector3(0, 0, angle),
                    3, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    //刷新转盘 按钮
                    // RefreshSpinBtn();

                    StartCoroutine(WheelEnd(reward, whType));
                });
        }

        IEnumerator WheelEnd(YZReward reward, WheelType whType)
        {
            //播放扫光特效 
            var com = GetCurrentWheelCom();
            var WheelSelect = com.gameObject.FindChild<SkeletonGraphic>("WheelSelect");
            WheelSelect.SetActive(true);
            WheelSelect.AnimationState.SetAnimation(0, "animation", false);

            yield return new WaitForSeconds(1.6f);

            WheelSelect.SetActive(false);
            //再来一次
            if (reward.type >= Const.OneMoreTime)
            {
                StartCoroutine(OneMoreTime());
            }
            else
            {
                //如果是bonus , 播放
                if (reward.type is Const.Bonus or Const.Cash)
                {
                    PlayGetBonusEffect();
                }

                IsWheelAnima = false;

                var diff = new List<Item>
                {
                    new(reward.type, reward.amount)
                };

                UserInterfaceSystem.That.ShowUI<UIGetRewards>(new GameData()
                {
                    ["diff"] = diff
                });

                yield return new WaitUntil(() => UserInterfaceSystem.That.Get<UIGetRewards>() != null);

                yield return new WaitUntil(() => UserInterfaceSystem.That.Get<UIGetRewards>() == null);

                //玩家每日首次转动免费转盘
                if (whType == WheelType.Free && ShouldPopPayWheel())
                {
                    SavePopedPayWheelTime();

                    vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value =
                        MediatorActivity.Instance.GetTargetWheelType();

                    /*UserInterfaceSystem.That.ShowQueue<UIConfirm>(
                    new UIConfirmData
                    {
                        confirmCall = () =>
                        {
                            vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value =
                                MediatorActivity.Instance.GetTargetWheelType();
                            // SendWheelNoCondition();
                        },
                        desc = I18N.Get("pay_wheel_tips"),
                    }
                );*/
                }
            }
        }

        /// <summary>
        /// 播放获得Bonus的特效
        /// </summary>
        public void PlayGetBonusEffect()
        {
            Effect.SetActive(true);
            Effect.GetComponent<ParticleImage>().Play();
        }

        /// <summary>
        /// 存放时间
        /// </summary>
        private void SavePopedPayWheelTime()
        {
            YZLog.LogColor("保存转盘转动时间 " + TimeUtils.Instance.Today);
            PersistSystem.That.SaveValue(GlobalEnum.POPED_PAY_WHEEL_TIME, TimeUtils.Instance.Today, true);
        }

        Transform GetWheelCom(WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Free:
                    return Content.GetChild(0);
                    break;
                case WheelType.PaySmall:
                    return Content.GetChild(1);
                    break;
                case WheelType.PayBig:
                    return Content.GetChild(2);
                    break;
            }

            return null;
        }

        /// <summary>
        /// 是否应该弹出付费
        /// </summary>
        /// <returns></returns>
        private bool ShouldPopPayWheel()
        {
            var day = (int)PersistSystem.That.GetValue<int>(GlobalEnum.POPED_PAY_WHEEL_TIME, true);

            YZLog.LogColor("读取转盘转动时间 " + day + "  对比的时间是 " + TimeUtils.Instance.Today);

            //判断和time now是否在一天内

            return day != TimeUtils.Instance.Today;
        }

        IEnumerator OneMoreTime()
        {
            yield return new WaitForSeconds(1);
            SendWheelNoCondition();
        }

        protected override void OnClose()
        {
            base.OnClose();

            YZFunnelUtil.YZFunnelActivityPop(ActivityType.FortuneWheel
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
        
        public override void OnStart()
        {
#if DAI_TEST
            // Root.Instance.WheelFreeTicket = 100;
#endif
            var int_Type = GetArgsByIndex<int>(0);

            if (Enum.IsDefined(typeof(WheelType), int_Type))
            {
                ro_type = (WheelType)int_Type;
            }
            else
            {
                ro_type = ChooseShowWheelType();
            }

            switch (ro_type)
            {
                case WheelType.Free:
                    toggleFree.isOn = true;
                    break;
                case WheelType.PaySmall:
                    toggleSmallPay.isOn = true;
                    break;
                case WheelType.PayBig:
                    toggleBigPay.isOn = true;
                    break;
            }

            InitLights();
            horizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(pageIndex =>
            {
                var type = pageIndex + 1;
                vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value = (WheelType)type;
            });

            toggleFree.onValueChanged.AddListener(isOn =>
            {
                if (IsWheelAnima)
                {
                    return;
                }

                if (isOn)
                {
                    vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value = WheelType.Free;
                }
            });

            toggleSmallPay.onValueChanged.AddListener(ison =>
            {
                if (IsWheelAnima)
                {
                    return;
                }

                if (ison)
                {
                    vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value = WheelType.PaySmall;
                }
            });


            toggleBigPay.onValueChanged.AddListener(ison =>
            {
                if (IsWheelAnima)
                {
                    return;
                }

                if (ison)
                {
                    vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value = WheelType.PayBig;
                }
            });

            button_CloseBtn.OnClickAsObservable().Subscribe(unit =>
            {
                if (IsWheelAnima)
                {
                    return;
                }

                Close();
            });


            InitWheelList();
        }

        private void InitWheelList()
        {
            // if (WheelList != null)
            // {
            //     foreach (var transform in WheelList)
            //     {
            //         transform.SetGray(true);
            //     }
            // }

            // Type2Gameobject = new()
            // {
            //     [WheelType.Free] = toggleFree.transform.parent.gameObject,
            //     [WheelType.PaySmall] = toggleSmallPay.gameObject,
            //     [WheelType.PayBig] = toggleBigPay.gameObject
            // };

            //开放的转盘
            foreach (var wheelType in new[] { WheelType.Free, WheelType.PaySmall, WheelType.PayBig })
            {
                // var rewards = GetRewards(wheelType);
                // Type2Gameobject[wheelType].transform.SetGray(rewards == null);

                var com = GetWheelCom(wheelType);
                com.SetActive(true);
                var spinBtn = com.gameObject.FindChild<MyButton>("SpinBtn");
                spinBtn.SetClick(() =>
                {
                    if (spinBtn.Gray)
                    {
                        return;
                    }

                    OnSpinBtnClick();
                });

                var rewardsCom = com.gameObject.FindChild<Transform>("Circle");

                var listData = GetRewards(wheelType);

                //奖励数量是8
                for (int i = 0; i < 8; i++)
                {
                    var rewardItem = rewardsCom.GetChild(i);
                    var data = listData[i];

                    if (data.type >= Const.OneMoreTime)
                    {
                        rewardItem.GetComponentInChildren<Image>().sprite =
                            MediatorBingo.Instance.GetSpriteByUrl($"{ClassType.Name}/one_more");
                        rewardItem.GetComponentInChildren<Text>().text = I18N.Get("key_wheel_roll_again");
                    }
                    else
                    {
                        rewardItem.GetComponentInChildren<Image>().sprite =
                            MediatorItem.Instance.GetItemSprite(data.type);
                        rewardItem.GetComponentInChildren<Text>().text = data.amount.ToString();
                    }
                }

                SetSpinBtnDisplay(wheelType);
            }
        }

        /// <summary>
        /// 设置spin
        /// </summary>
        /// <param name="wheelType"></param>
        private void SetSpinBtnDisplay(WheelType wheelType)
        {
            Transform com = GetWheelCom(wheelType);
            var sizeUpDown = com.gameObject.FindChild<SizeUpDown>("SpinBtn/SpinText");
            if (wheelType is WheelType.Free)
            {
                // var sizeUpDown = com.gameObject.FindChild<SizeUpDown>("SpinBtn/Spin_Image");
                sizeUpDown.enabled = !isWheelAnima && IsFreeWheelCanSpin();
            }
            else
            {
                // var spinImage = com.gameObject.FindChild<Transform>("SpinBtn/Spin_Image");
                // var spinImageReady = com.gameObject.FindChild<Transform>("SpinBtn/Spin_ready");

                Text costText = null;
                bool visible = false;
                if (wheelType is WheelType.PaySmall)
                {
                    costText = toggleSmallPay.gameObject.FindChild<Text>("costText");
                    visible = !isWheelAnima && Root.Instance.FortuneWheelInfo.PaySmallCount > 0;
                }

                if (wheelType is WheelType.PayBig)
                {
                    costText = toggleBigPay.gameObject.FindChild<Text>("costText");
                    visible = !isWheelAnima && Root.Instance.FortuneWheelInfo.PayBigCount > 0;
                }

                int key = FortuneWheelInfo.GetKey(wheelType);
                //如果玩家没有 转盘机会 
                Root.Instance.WheelChargeInfos.TryGetValue(key, out var data);

                if (costText != null)
                {
                    costText.text = I18N.Get("key_money_count", data?.amount);
                }
                
                sizeUpDown.enabled = visible;
                // spinImage.SetActive(!visible);
                // spinImageReady.SetActive(visible);
            }
 
        }

        private bool isWheelAnima;

        private bool IsWheelAnima
        {
            set
            {
                var spinBtn = GetCurrentSpinBtn();

                isWheelAnima = value;

                if (value)
                {
                    ForbidClick();
                    spinBtn.Gray = true;
                }
                else
                {
                    ResumeClick();
                    var freeTicket = vm[vname.FreeTicket.ToString()].ToIObservable<int>().Value;
                    var type = vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value;
                    if (type == WheelType.Free)
                    {
                        spinBtn.Gray = freeTicket < Const.FREE_WHEEL_COST;
                    }
                    else
                    {
                        spinBtn.Gray = false;
                    }
                }

                RefreshSpinBtn();
                //转动的时候禁止 切换
                horizontalScrollSnap.GetComponent<ScrollRect>().horizontal = !value;
            }
            get => isWheelAnima;
        }

        /// <summary>
        /// 获取当前页面的spinBtn
        /// </summary>
        /// <returns></returns>
        MyButton GetCurrentSpinBtn()
        {
            var wheelType = vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value;
            var wheelCom = GetWheelCom(wheelType);
            return wheelCom.gameObject.FindChild<MyButton>("SpinBtn");
        }

        MyButton GetFreeSpinBtn()
        {
            var wheelCom = GetWheelCom(WheelType.Free);
            return wheelCom.gameObject.FindChild<MyButton>("SpinBtn");
        }

        /// <summary>
        /// 当前的 要转动的 对象
        /// </summary>
        /// <returns></returns>
        Transform GetCurrentRewardCom()
        {
            var wheelCom = GetCurrentWheelCom();
            return wheelCom.gameObject.FindChild<Transform>("Circle");
        }

        Transform GetCurrentWheelCom()
        {
            var wheelType = vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value;
            return GetWheelCom(wheelType);
        }

        /// <summary>
        /// 优先级  免费
        /// </summary>
        /// <returns></returns>
        public WheelType ChooseShowWheelType()
        {
            //选择能转的转盘 
            if (!IsPayWheelNeedCharge(WheelType.PayBig))
            {
                return WheelType.PayBig;
            }

            if (!IsPayWheelNeedCharge(WheelType.PaySmall))
            {
                return WheelType.PaySmall;
            }

            if (IsFreeWheelCanSpin())
            {
                return WheelType.Free;
            }

            //跳转上一次能转过的转盘
            var lastRecord = GetRecordWheelType();
            if (lastRecord > 0)
            {
                return (WheelType)lastRecord;
            }

            //没有能转的, 跳转免费转盘
            return WheelType.Free;
        }

        private void OnSpinBtnClick()
        {
            //转盘
            var roType = vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value;

            switch (roType)
            {
                case WheelType.Free:
                    SendWheel();
                    break;
                case WheelType.PaySmall:
                case WheelType.PayBig:
                    //是否需要充值 
                    if (IsPayWheelNeedCharge(roType))
                    {
                        int key = FortuneWheelInfo.GetKey(roType);
                        //如果玩家没有 转盘机会 
                        Root.Instance.WheelChargeInfos.TryGetValue(key, out var data);
                        if (data != null)
                        {
                            ChargeForWheel(data);
                        }
                    }
                    else
                    {
                        SendWheel();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendWheel()
        {
            if (IsWheelAnima)
            {
                return;
            }

            SendWheelNoCondition();
        }

        /// <summary>
        /// 付费转盘是否需要充值
        /// </summary>
        /// <param name="wheelType"></param>
        /// <returns></returns>
        private bool IsPayWheelNeedCharge(WheelType wheelType)
        {
            if (wheelType is WheelType.PaySmall)
            {
                return Root.Instance.FortuneWheelInfo.PaySmallCount <= 0;
            }

            if (wheelType is WheelType.PayBig)
            {
                return Root.Instance.FortuneWheelInfo.PayBigCount <= 0;
            }

            return false;
        }

        private void SendWheelNoCondition()
        {
            var type = vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value;

            switch (type)
            {
                case WheelType.Free:
                    if (!IsFreeWheelCanSpin())
                    {
                        return;
                    }

                    MediatorRequest.Instance.FreeWheel();
                    break;
                case WheelType.PaySmall:
                case WheelType.PayBig:
                    MediatorRequest.Instance.PayWheel(FortuneWheelInfo.GetPayChargeId(type), type);
                    break;
            }

            RecordWheelType(type);
            IsWheelAnima = true;
        }


        /// <summary>
        /// 记录上一次转动的转盘
        /// </summary>
        /// <param name="wheelType"></param>
        void RecordWheelType(WheelType wheelType)
        {
            PersistSystem.That.SaveValue(GlobalEnum.LAST_WHEEL_TYPE, (int)wheelType, true);
        }

        private int GetRecordWheelType()
        {
            return (int)PersistSystem.That.GetValue<int>(GlobalEnum.LAST_WHEEL_TYPE, true);
        }

        /// <summary>
        /// 免费转盘是否能转
        /// </summary>
        /// <returns></returns>
        private bool IsFreeWheelCanSpin()
        {
            return IsWheelAnima || Root.Instance.WheelFreeTicket >= Const.FREE_WHEEL_COST;
        }

        private void ChargeForWheel(WheelChargeInfo data)
        {
            Charge_configsItem chargeItemTest = new Charge_configsItem();
            chargeItemTest.id = data.id;
            chargeItemTest.bonusValue = data.show_bonus;
            chargeItemTest.amount = data.amount.ToString();
            chargeItemTest.position = "Wheel";
            LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1,
                () =>
                {
                    if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.FortuneWheel))
                        UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                });
        }

        private WheelType ro_type;

        /// <summary>
        /// 名字不要随便动了, 和资源路径绑定
        /// </summary>

        enum vname
        {
            /// <summary>
            /// 转盘类型
            /// </summary>
            rotaryType,


            FreeTicket
        }

        public override void InitVm()
        {
            vm[vname.rotaryType.ToString()] = new ReactivePropertySlim<WheelType>(ro_type);

            vm[vname.FreeTicket.ToString()] = new ReactivePropertySlim<int>(Root.Instance.WheelFreeTicket);
        }

        public override void InitBinds()
        {
            vm[vname.FreeTicket.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                FreeTicketText.text = $"{value}/{Const.FREE_WHEEL_COST}";

                //不满足票数
                var spinBtnGray = value < Const.FREE_WHEEL_COST;
                if (!IsWheelAnima)
                {
                    var spinBtn = GetFreeSpinBtn();
                    spinBtn.Gray = spinBtnGray;
                }
            });

            vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Subscribe(value =>
            {
                var page = value.ToInt32() - 1;
                if (!IsInitEnd)
                {
                    horizontalScrollSnap.StartingScreen = page;
                }

                horizontalScrollSnap.ChangePage(page);

                SetSpinBtnDisplay(value);

                int key = FortuneWheelInfo.GetKey(value);
                //如果玩家没有 转盘机会 
                Root.Instance.WheelChargeInfos.TryGetValue(key, out var data);

                switch (value)
                {
                    case WheelType.Free:
                        DescText.text = I18N.Get("key_wheel_free_desc");
                        break;
                    case WheelType.PaySmall:
                    case WheelType.PayBig:
                        DescText.text = I18N.Get("key_wheel_pay_desc", data?.amount);
                        break;
                }

                // var spinBtn = GetCurrentSpinBtn();
                //
                // if (!IsWheelAnima)
                // {
                //     var freeTicket = vm[vname.FreeTicket.ToString()].ToIObservable<int>().Value;
                //     if (value == WheelType.Free)
                //     {
                //         spinBtn.Gray = freeTicket < FreeWheelCost;
                //     }
                //     else
                //     {
                //         spinBtn.Gray = false;
                //     }
                // }
            });
        }

        /// <summary>
        /// 获得bonus播放彩带
        /// </summary>
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Start_Wheel, (sender, eventArgs) => { StartWheel(sender); });

            AddEventListener(Proto.FREE_WHEEL, (sender, eventArgs) =>
            {
                if (eventArgs is ProtoEventArgs { Result: ProtoResult.Fail })
                {
                    IsWheelAnima = false;
                }
            });

            AddEventListener(Proto.PAY_WHEEL, (sender, eventArgs) =>
            {
                if (eventArgs is ProtoEventArgs { Result: ProtoResult.Fail })
                {
                    IsWheelAnima = false;
                }
            });


            AddEventListener(GlobalEvent.Sync_WheelFreeTicket, (sender, eventArgs) =>
                vm[vname.FreeTicket.ToString()].ToIObservable<int>().Value = Root.Instance.WheelFreeTicket
            );

            AddEventListener(GlobalEvent.Change_Wheel_Free_Ticket, (sender, eventArgs) =>
                {
                    if (sender is int fakeValue)
                    {
                        vm[vname.FreeTicket.ToString()].ToIObservable<int>().Value = fakeValue;
                    }
                }
            );
            
            AddEventListener(GlobalEvent.SYNC_FORTUNE_WHEEL_INFO, (sender, eventArgs) => { RefreshSpinBtn(); });
        }

        private void RefreshSpinBtn()
        {
            SetSpinBtnDisplay(vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value);
        }

        private List<YZReward> GetRewards(WheelType value)
        {
            List<YZReward> listData = null;
            switch (value)
            {
                case WheelType.Free:
                    listData = Root.Instance.FreeWheelList;
                    break;
                case WheelType.PaySmall:
                    listData = Root.Instance.SmallPayWheelList;
                    break;
                case WheelType.PayBig:
                    listData = Root.Instance.BigPayWheelList;
                    break;
            }

            return listData;
        }

        /// <summary>
        /// 获得当前
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private YZReward GetReward(int order)
        {
            List<YZReward> listData = GetRewards(vm[vname.rotaryType.ToString()].ToIObservable<WheelType>().Value);
            if (order == 0)
            {
                return null;
            }

            return listData[order - 1];
        }

        public override void Close()
        {
            OnAnimationOut();

            for (int i = 0; i < Content.childCount; i++)
            {
                if (i == horizontalScrollSnap.CurrentPage)
                {
                    continue;
                }

                Content.GetChild(i).SetActive(false);
            }
        }

        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => base.Close());
        }
    }
}