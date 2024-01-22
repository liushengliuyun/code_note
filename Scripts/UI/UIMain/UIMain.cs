using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AndroidCShape;
using BrunoMikoski.AnimationsSequencer;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Server;
using Core.Services.AudioService.API.Facade;
using Core.Services.NetService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Services.UserInterfaceService.UIExtensions.Scripts.Utilities;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Reactive.Bindings;
using Spine.Unity;
using ThinkingAnalytics;
using UI.Activity;
using UI.Effect;
using UI.Mono;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using iOSCShape;
using Jing.TurbochargedScrollList;
using Scheduler = DataAccess.Controller.Scheduler;
using Text = UnityEngine.UI.Text;
using Timer = UnityTimer.Timer;

namespace UI
{
    // [BindPrefab("BundlesRes/UI/UIMain")]
    public class UIMain : UIBase<UIMain>
    {
        #region UI Variable Statement

        [SerializeField] private MyButton TaskProgressBtn;
        [SerializeField] private Transform Pop_Res_Tip;

        [SerializeField] private Transform Normal_Res_Group;

        // [SerializeField] private MyButton HighScoreTeach;
        [SerializeField] private MyButton MuseumAboutBtn;
        [SerializeField] private Transform MuseumFirstItem;

        [SerializeField] private MuseumMono MuseumMono;

        [SerializeField] private MyButton collect_recordBtn;
        [SerializeField] private MyButton collectBGBtn;
        [SerializeField] private Transform secondMiddle;
        [SerializeField] private Transform secondFill;

        [SerializeField] private Text GuideText;

        [SerializeField] private ScrollRect ActivityScrollRect;

        /// <summary>
        /// 活动 parent
        /// </summary>
        private Transform ActivityContent => ActivityScrollRect.content;

        [SerializeField] private Transform FreeGiftsContent;

        /// <summary>
        /// 礼包Banner
        /// </summary>
        [SerializeField] private Transform StarterPackerTrans;

        // 特殊礼包Banner
        [SerializeField] private Transform SpecialGiftATrans;
        [SerializeField] private Transform SpecialGiftBTrans;
        [SerializeField] private Transform SpecialGiftCTrans;

        // ComingSoon Banner
        [SerializeField] private Transform ComingSoonTrans;

        [SerializeField] private SkeletonGraphic WheelSpine;

        [SerializeField] private Transform HoldImage;

        [SerializeField] private Text ListEmptyText;
        [SerializeField] private MyButton CollectAllBtn;
        [SerializeField] private ShopMono shopMono;
        [SerializeField] private Transform[] Pages;

        /// <summary>
        /// 玩家信息按钮
        /// </summary>
        [SerializeField] private MyButton PlayerBtn;

        [SerializeField] private Button MaskCloseBtn;
        [SerializeField] private Transform SettingPanel;
        [SerializeField] private Image PlayerImage;

        [SerializeField] private Transform GuideMask;

        [SerializeField] private Transform EffectMask;

        /// <summary>
        /// 任务入口
        /// </summary>
        [SerializeField] private MyButton TaskBtn;

        // [SerializeField] private MyButton DailyTaskBtn;

        [SerializeField] private MyButton LuckyCardBtn;
        [SerializeField] private MyButton BestOfferBtn;
        [SerializeField] private MyButton JustForYouBtn;

        [SerializeField] private MyButton DragonBtn;

        [SerializeField] private MyButton PiggyBankBtn;

        [SerializeField] private MyButton StarterPackBtn;

        [SerializeField] private MyButton SpecialGiftBtn;
        [SerializeField] private MyButton SpecialOfferBtn;
        [SerializeField] private GameObject ActivityGroup;

        private Transform recordRewardGroup => collect_recordBtn.transform.parent;

        public MyButton OnlineRewardBtn;

        public Toggle HistoryToggle;

        /// <summary>
        /// 博物馆入口
        /// </summary>
        public Toggle CollectionToggle;

        public Toggle roomToggle;
        public Toggle StoreToggle;

        [SerializeField] private Transform select;

        /// <summary>
        /// 邀请对战入口
        /// </summary>
        public DuelEntry DuelEntryBtn;

        public Button DiamondBtn;

        /// <summary>
        /// 顶部资源栏按钮 美金
        /// </summary>
        public Button BonusBtn;

        public Button CoinBtn;

        public ScrollRect RoomListRect;
        public ScrollRect MatchHistoryRect;

        public Button WheelBtn;
        public Button SettingBtn;

        private MyButton MagicBallBtn;
        private Text MagicBallCountText;

        private MyButton LuckyGuyBtn;

        private MyList RoomList;

        private MyList MatchHistoryList;

        private GridLayoutGroup activityLayoutGroup => ActivityContent.transform.GetComponent<GridLayoutGroup>();

        #endregion

        private Vector2 HistoryRectOffsetMin;
        private Vector2 RoomRectOffsetMax;

        /// <summary>
        /// 一行最多显示的活动
        /// </summary>
        private const int OneActivityRowCount = 5;

        // 特殊礼包请求次数（特殊情况）
        int reqSpecialTime = 0;

        // 是否显示默认ComingSoon Banner
        private bool isShowComingSoon = true;

        /// <summary>
        /// 开机的活动数量
        /// </summary>
        private int EnabledActivityCount
        {
            get
            {
                //暂时 自然量是没有活动的
                if (Root.Instance.IsNaturalFlow)
                {
                    return 0;
                }

                int result = 0;

                for (int i = 0; i < ActivityContent.childCount; i++)
                {
                    var child = ActivityContent.GetChild(i);
                    if (child.gameObject.activeSelf)
                    {
                        result++;
                    }
                }

                // YZLog.LogColor("EnabledActivityCount = " + result);
                return result;
            }
        }

        private static UIMain _inst;
        public static UIMain Shared() => _inst;

        void FindCom()
        {
            MagicBallBtn = ActivityContent.gameObject.FindChild<MyButton>("MagicBall");
            LuckyGuyBtn = ActivityContent.gameObject.FindChild<MyButton>("LuckyGuy");
            MagicBallCountText = ActivityContent.gameObject.FindChild<Text>("MagicBall/CountText");
        }

        public override void OnStart()
        {
            FindCom();

            #region -------------- 主界面的各种弹窗逻辑 , 新手引导---------------------

            GuideMask.SetActive(false);

            EffectMask.SetActive(false);

            Scheduler.Instance.PostTask(MediatorBingo.Instance.RestoreGame);

// #if !DAI_TEST
            var table = GetTable();
            if (table?["dontPopWindows"] is not true)
            {
                Scheduler.Instance.PostTask(VarietyPop);
            }
// #endif

            #endregion ------------ 主界面的各种弹窗逻辑-----------------------

            HistoryRectOffsetMin = MatchHistoryRect.transform.rectTransform().offsetMin;
            RoomRectOffsetMax = RoomListRect.transform.rectTransform().offsetMax;

            HoldImage.SetActive(Root.Instance.IsNaturalFlow);

            RegisterInterval(1, () =>
            {
                //调用跨天接口
                TimeUtils.Instance.TimeToTomorrow();

                RefreshActivity();

                RefreshRedPoint();

                //检查更新充值挡位
                MediatorActivity.Instance.CheckStartPacker();
            }, true);


            SetWheelBtnSpin();

            //需要在活动更新后
            RandomPlaySpineAnimation(ActivityIndex, ActivityContent);

            RandomPlaySpineAnimation(FreeGiftsIndex, FreeGiftsContent);


            EventDispatcher.Root.Raise(GlobalEvent.Click_How_To_Play);

            if (!Root.Instance.IsNaturalFlow)
            {
                Timer.Register(30f, () =>
                {
                    RegisterInterval(60f, () =>
                    {
                        if (vm == null || !vm.Any())
                        {
                            return;
                        }

                        if (!IsUIMainTop())
                        {
                            return;
                        }

                        if (vm[vname.Page.ToString()].ToIObservable<int>().Value != 0)
                        {
                            if (ExistNotClaimRecord())
                            {
                                recordRewardGroup.SetActive(true);
                                recordRewardGroup.GetComponent<AnimationSequence>().Play();
                            }
                        }
                    }, true);
                });

                if (Root.Instance.MagicBallInfo is not { LessTime: > 0 })
                {
                    MediatorRequest.Instance.RefreshMagicBall();
                }
                else
                {
                    InitRefreshMagicBallTimer();
                }

                if (Root.Instance.MuseumInfo is not { LessTime: > 0 })
                {
                    MediatorRequest.Instance.RefreshMuseum();
                }
                else
                {
                    InitRefreshMuseumTimer();
                }
            }

            CollectionToggle.SetActive(!Root.Instance.IsNaturalFlow && Root.Instance.MuseumInfo != null);

            StoreToggle.SetActive(!Root.Instance.IsNaturalFlow);

            BonusBtn.SetActive(!Root.Instance.IsNaturalFlow);

            //mark 目前自然量没有活动

            ActivityScrollRect.SetActive(!Root.Instance.IsNaturalFlow);
            //如果是非自然量
            if (!Root.Instance.IsNaturalFlow)
            {
                BonusBtn.onClick.AddListener(ToStorePanel);
                DiamondBtn.onClick.AddListener(ToStorePanel);
                CoinBtn.onClick.AddListener(ToStorePanel);

                BestOfferBtn.SetClick(() =>
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.BestOffer, BestOfferBtn.name, nameof(UIBestOffer));
                    UserInterfaceSystem.That.ShowUI<UIBestOffer>(new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.Click
                    });
                });

                JustForYouBtn.SetClick(() =>
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.JustForYou, JustForYouBtn.name, nameof(UIJustForYou));
                    UserInterfaceSystem.That.ShowUI<UIJustForYou>(new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.Click
                    });
                });

                MagicBallBtn.SetClick(() =>
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.MagicBall, MagicBallBtn.name, nameof(UIMagicBall));
                    UserInterfaceSystem.That.ShowUI<UIMagicBall>( new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.Click
                    });
                });
                MagicBallCountText.text = Root.Instance.MagicBallInfo?.CurrentPoint.ToString();

                if (LuckyGuyBtn != null && Root.Instance.LuckyGuyInfo != null)
                {
                    LuckyGuyBtn.SetClick(() =>
                    {
                        YZFunnelUtil.YZFunnelClickActivity(ActivityType.LuckyGuy, LuckyGuyBtn.name, nameof(UILuckyGuy));
                        UserInterfaceSystem.That.ShowUI<UILuckyGuy>(new GameData()
                        {
                            ["enterType"] = ActivityEnterType.Click
                        });
                    });
                }

                MuseumAboutBtn.SetClick(() =>
                {
                    UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                    {
                        Type = UIConfirmData.UIConfirmType.OneBtn,
                        // HideCloseBtn = true,
                        desc = I18N.Get("key_museum_how_to_play"),
                        confirmTitle = I18N.Get("key_ok"),
                        AligmentType = TextAnchor.MiddleLeft,
                        Rect2D = new Vector2(650, 650),
                        // Position = new Vector2(0, 15),
                    });
                });
            }

            SettingPanel.SetActive(false);

            RefreshSettingPanel();

            RegisterClickEvent();

            InitRoomList();
            InitMatchHistoryList();

            // 查询一次充值渠道
            MediatorRequest.Instance.GetChargeMethods(silence: true);

            // 查询一次玩家流水记录
            if (Root.Instance.CashFlow == null || Root.Instance.CashFlow.Count == 0)
                MediatorRequest.Instance.GetUserCashFlow();

            // 一条龙更新红点
            MediatorRequest.Instance.UpdateDragonInfo();

//             // 启动SDK
// #if (UNITY_ANDROID || UNITY_IOS) && !NO_SDK
//             YZSDKsController.Shared.YZInitSDKAndConfig();
// #endif

            ThinkingAnalyticsAPI.Track("loaded");

            // 保留这句
            YZServerApiOrganic.Shared.SetOrganic(YZOrganic.YZNONORGANIC);

            DuelEntryBtn.gameObject.SetActive(!Root.Instance.IsNaturalFlow);
        }

        protected override void AfterInitEvents()
        {
            CheckLuckyYou();
        }

        private void CheckLuckyYou()
        {
            var value = Root.Instance.MatchHistory;
            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.Lucky_Guy_Played_Effect) &&
                value is { Count: > 0 })
            {
                var uimain = UserInterfaceSystem.That.Get<UIMain>();

                if (!(uimain == null || !uimain.IsInitEnd))
                {
                    var history = value.Find(history => history.IsLuckyRoom && history.CanClaim);
                    if (history != null)
                    {
                        EventDispatcher.Root.Raise(GlobalEvent.LUCKY_GUY_FAKE_NEWS, history.match_id);
                    }
                }
            }
        }

        private void InitRefreshMagicBallTimer()
        {
            if (Root.Instance.MagicBallInfo is { LessTime: > 0 })
            {
                refreshMagicBallTimer?.Cancel();
                refreshMagicBallTimer = this.AttachTimer(Root.Instance.MagicBallInfo.LessTime,
                    () => MediatorRequest.Instance.RefreshMagicBall());
            }
        }

        private void InitRefreshMuseumTimer()
        {
            if (Root.Instance.MuseumInfo is { LessTime: > 0 })
            {
                refreshMuseumTimer?.Cancel();
                refreshMuseumTimer = this.AttachTimer(Root.Instance.MuseumInfo.LessTime,
                    () => MediatorRequest.Instance.RefreshMuseum());
            }
        }

        /*/// <summary>
        /// 倒计时刷新 LuckyGuy
        /// </summary>
        private void InitLuckyGuyTimer()
        {
            if (Root.Instance.LuckyGuyInfo is { IsOpen: false, UntilOpenTime : > 0 })
            {
                luckyGuyTimer?.Cancel();
                luckyGuyTimer = this.AttachTimer(Root.Instance.LuckyGuyInfo.UntilOpenTime,
                    () => MediatorRequest.Instance.RefreshLuckyGuy());
            }
        }*/

        private void RegisterClickEvent()
        {
            collect_recordBtn.SetClick(() => ToRecordPanel());
            collectBGBtn.SetClick(() =>
            {
                recordRewardGroup.GetComponent<AnimationSequence>().Kill();
                recordRewardGroup.GetComponent<RectTransform>().DOAnchorPosY(135, 0.3f);
            });

            SettingBtn.onClick.AddListener(ShowSettings);
            MaskCloseBtn.onClick.AddListener(CloseSettingPanel);

            TaskBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.TaskSystem, TaskBtn.name, nameof(UITask));
                MediatorTask.Instance.PopTaskSystem(0);
            });

            TaskProgressBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.TaskSystem, TaskProgressBtn.name, nameof(UITask));
                MediatorTask.Instance.PopTaskSystem(3);
            });

            /*DailyTaskBtn.SetClick(() =>
            {
                if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 0)
                {
                    // 没奖励，直接请求刷新, 否则暂时不刷新，领完再刷
                    MediatorRequest.Instance.GetDailyTaskInfo(true);
                    UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
                    MediatorRequest.Instance.ClaimDailyTask(true);
                }
            });*/

            LuckyCardBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.LuckyCard, LuckyCardBtn.name, nameof(UILuckyCard));
                UserInterfaceSystem.That.ShowUI<UILuckyCard>(ActivityEnterType.Click);
            });
            DragonBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.Dragon, DragonBtn.name, nameof(UIDragon));
                UserInterfaceSystem.That.ShowUI<UIDragon>(ActivityEnterType.Click);
            });
            SpecialGiftBtn.SetClick(() =>
            {
            
                int week = Root.Instance.Role.specialGiftInfo.today_week.ToInt();
                if (week == 1 || week == 2)
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.SpecialGift, SpecialGiftBtn.name, nameof(UISpecialGiftA));
                    UserInterfaceSystem.That.ShowUI<UISpecialGiftA>();
                }
                else if (week == 3 || week == 4 || week == 5)
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.SpecialGift, SpecialGiftBtn.name, nameof(UISpecialGiftB));
                    UserInterfaceSystem.That.ShowUI<UISpecialGiftB>();
                }
                else if (week == 6 || week == 0)
                {
                    YZFunnelUtil.YZFunnelClickActivity(ActivityType.SpecialGift, SpecialGiftBtn.name, nameof(UISpecialGiftC));
                    UserInterfaceSystem.That.ShowUI<UISpecialGiftC>();
                }
            });
            SpecialOfferBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.SpecialOffer, SpecialOfferBtn.name, nameof(UISpecialOffer));
                UserInterfaceSystem.That.ShowUI<UISpecialOffer>(new GameData()
                {
                    ["ActivityEnterType"] = ActivityEnterType.Click
                });
            });
            PiggyBankBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.PiggyBank, PiggyBankBtn.name, nameof(UIPiggyBank));
                HidePiggyFullTip();
                UserInterfaceSystem.That.ShowUI<UIPiggyBank>(ActivityEnterType.Click);
            });
            RefreshPiggyBankFullState(false);
            StarterPackBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.StartPacker, StarterPackBtn.name, nameof(UIStartPack));
                UserInterfaceSystem.That.ShowUI<UIStartPack>(ActivityEnterType.Click);
            });

            PlayerBtn.SetClick(() =>
            {
                if (Root.Instance.Role.NotBindLoginEmail)
                {
                    UserInterfaceSystem.That.ShowUI<UIPlayerInfo>();
                }
                else
                {
                    MediatorRequest.Instance.WithdrawHistory(false, () =>
                    {
                        UserInterfaceSystem.That.ShowUI<UIPlayerInfo>();

                    });
                }
            });

            CollectAllBtn.SetClick(() => { OnCollectAllBtnClick(); });

            HistoryToggle.OnValueChangedAsObservable().Subscribe(value =>
            {
                if (!vm.Any())
                {
                    return;
                }

                if (value)
                {
                    MediatorRequest.Instance.GetCompleteHistory(true);
                    MediatorRequest.Instance.GetInCompleteHistory(true);
                    vm[vname.Page.ToString()].ToIObservable<int>().Value = 0;
                }

                HistoryToggle.transform.Find("Text").SetActive(value);
            });

            roomToggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    vm[vname.Page.ToString()].ToIObservable<int>().Value = 1;
                    DOTween.Kill(ActivityGroup.GetComponent<RectTransform>(), false);
                    ActivityGroup.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(ActivityGroup.GetComponent<RectTransform>().anchoredPosition.x, -160);
                }
            });

            StoreToggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    ToStorePanel();
                }

                StoreToggle.transform.Find("Text").SetActive(value);
            });

            CollectionToggle.onValueChanged.AddListener(value =>
            {
                if (IsGuideRunning())
                {
                    return;
                }

                if (value)
                {
                    vm[vname.Page.ToString()].ToIObservable<int>().Value = 3;
                }

                CollectionToggle.transform.Find("Text").SetActive(value);
            });

            OnlineRewardBtn.OnClickAsObservable().Subscribe(unit =>
            {
                if (Root.Instance.OnlineRewardInfo != null)
                {
                    UserInterfaceSystem.That.ShowUI<UIOnlineReward>(new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.Click
                    });
                }
            });

            WheelBtn.OnClickAsObservable().Subscribe(unit =>
            {
                UserInterfaceSystem.That.ShowUI<UIWheel>(new GameData()
                {
                    ["ActivityEnterType"] = ActivityEnterType.Click
                });
            });

            DuelEntryBtn.GetComponent<MyButton>().SetClick(() =>
            {
                MediatorRequest.Instance.GetFriendsDuelInfo();
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.FriendDuel, DuelEntryBtn.name, nameof(UIFriendsDuel));
                UserInterfaceSystem.That.ShowUI<UIFriendsDuel>();
            });

            if (_inst == null)
                _inst = this;
        }

        private void OnCollectAllBtnClick()
        {
            var incompleteData = Root.Instance.MatchHistory.Where(history => history.CanClaimWhenWithDraw).ToList();

            void send()
            {
                var history = Root.Instance.MatchHistory.Find(history => history.IsLuckyRoom && history.CanClaim);
                if (history != null)
                {
                    EventDispatcher.Root.Raise(GlobalEvent.LUCKY_GUY_FAKE_NEWS, history.match_id);
                }
            }

            if (incompleteData is { Count: > 0 })
            {
                //如果失败， 则请求最新的历史记录，以刷新列表
                NetSystem.That.SetFailCallBack(content =>
                {
                    NetSystem.That.ShowNetWaitMask();
                    MediatorRequest.Instance.GetInCompleteHistory(true);
                    MediatorRequest.Instance.GetCompleteHistory(true);
                });


                NetSystem.That.ShowNetWaitMask();
                MediatorRequest.Instance.MatchClaim(incompleteData, forceSend: true, callback: send);
            }
            else
            {
                send();
            }
        }

        private void RefreshPiggyBankFullState(bool isCollectEffect)
        {
            var isFull = Root.Instance.PiggyBankInfo.IsFull;
            // PiggyBankBtn.gameObject.FindChild("Full").SetActive(isFull);

            var animationName = isFull ? "idea2" : "idea";

            PiggyBankBtn.gameObject.FindChild<SkeletonGraphic>("PiggyBankEntry").AnimationState
                .SetAnimation(0, animationName, true);

            if (isFull && isCollectEffect)
            {
                var fullTip = PiggyBankBtn.gameObject.FindChild("Full/Full Tip");

                fullTip.SetActive(true);

                // TinyTimer.StartTimer(() => { HidePiggyFullTip(); }, 5f);
            }

            piggyCollecting = false;
        }

        private void HidePiggyFullTip()
        {
            PiggyBankBtn.gameObject.TryGetComponent<AddHeightCanvas>(out var addHeightCanvas);
            var fullTip = PiggyBankBtn.gameObject.FindChild("Full/Full Tip");
            if (addHeightCanvas != null)
            {
                addHeightCanvas.enabled = false;
            }

            if (fullTip != null)
            {
                fullTip.SetActive(false);
            }
        }


        private void OnDisable()
        {
            HidePiggyFullTip();
        }

        private void SetWheelBtnSpin()
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            var spinAniName = Root.Instance.FortuneWheelInfo.HaveChance ? "spin2" : "spin1";

            var trackEntry = WheelSpine.AnimationState.GetCurrent(0);
            var currentAnimation = trackEntry.Animation.Name;

            if (spinAniName == currentAnimation)
            {
                return;
            }

            WheelSpine.timeScale = 1;
            WheelSpine.AnimationState.SetAnimation(0, spinAniName, spinAniName == "spin2");
        }

        private void RefreshRedPoint()
        {
            EventDispatcher.Root.Raise(GlobalEvent.REFRESH_RED_POINT);
        }

        /// <summary>
        /// 跳转到商店panel
        /// </summary>
        private void ToStorePanel()
        {
            if (IsGuideRunning())
            {
                return;
            }

            vm[vname.Page.ToString()].ToIObservable<int>().Value = 2;
        }

        private void ToRecordPanel()
        {
            if (IsGuideRunning())
            {
                return;
            }

            vm[vname.Page.ToString()].ToIObservable<int>().Value = 0;
        }

        private void RefreshSettingPanel()
        {
            SettingPanel.GetComponent<SettingPanelMono>().EmailValidType =
                Root.Instance.IsBindMail ? EmailValidType.Binded : EmailValidType.None;
        }

        private void ShowSettings()
        {
            HidePiggyFullTip();
            SettingPanel.SetActive(true);
            SettingPanel.GetComponent<AnimationSequence>().Play();
        }

        public void CloseSettingPanel()
        {
            SettingPanel.transform.HideUIByEffect(SettingPanel.GetComponent<AnimationSequence>());
        }

        /// <summary>
        /// 新手引导到第一局房间 是否Run
        /// </summary>
        public static bool IsFirstRoomGuideRunning;

        /// <summary>
        /// 最近一次点击资源是否充足
        /// </summary>
        private bool IsResourceEnough { get; set; } = true;

        bool beforeEnterGuideFinish;

        /// <summary>
        /// 各种弹窗逻辑
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        async UniTask VarietyPop()
        {
            await UniTask.WaitUntil(() => { return IsUIMainTop(); });

            if (IsUIMainInAnimation)
            {
                await UniTask.WaitUntil(() => !IsUIMainInAnimation);
            }

            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.BEFORE_ENTER_ROOM))
            {
                IsFirstRoomGuideRunning = true;

                /*var timeoutToken = new CancellationTokenSource();
                timeoutToken.CancelAfterSlim(TimeSpan.FromSeconds(10)); // 设置10s超时*/
                try
                {
                    //避免真机上 ui加载慢
                    EffectMask.SetActive(true);
                    UserInterfaceSystem.That.ShowUI<UINewPlayerBonus>();

                    //等待玩家领取奖励完成
                    await UniTask.WaitUntil(() => IsUIMainTop());

                    EffectMask.SetActive(false);

                    //todo 如果没有奖励， 会有bug， 但是没有奖励， 本来就玩不了
                    await UniTask.WaitUntil(() => beforeEnterGuideFinish);
                }
                catch (Exception e)
                {
                    YZLog.LogColor("引导 BEFORE_ENTER_ROOM 报错" + e.ToString(), "red");
                }
            }

            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.FIRST_ROOM_GAME))
            {
                await GuideFirstGame();
            }
            else
            {
                //防御
                GuideEnd();
            }

            //防御
            IsFirstRoomGuideRunning = false;
            GuideMask.SetActive(false);
        }

        /// <summary>
        /// 登陆弹窗
        /// </summary>
        private void GuideEnd()
        {
            IsFirstRoomGuideRunning = false;

            GuideMask.SetActive(false);

            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            //特殊处理，第一局结束后不弹
            if (Root.Instance.MatchHistoryCount >= 2 ||
                !Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.SECOND_BONUS_GAME))
            {
                if (!game_end_poped)
                {
                    //本来游戏结束后就会弹窗
                    MediatorActivity.Instance.PopAllActivity();
                }

                MediatorTask.Instance.PopTaskSystem(1);

                // 当天还没签到才弹窗
                if (Root.Instance.SignInfo.sign_chance == 1)
                    UserInterfaceSystem.That.SingleTonQueue<UISign>();

                MediatorActivity.Instance.PopActivity(ActivityType.MonthCard);

                MediatorActivity.Instance.PopActivity(ActivityType.LuckyGuy);
            }

            //弹出提示 绑定vip信息
            if (Root.Instance.UserInfo.save_email_group == "B" &&
                !Root.Instance.UserInfo.IsBindedVipInfo &&
                Root.Instance.ChargeInfo.success_total >= 100 &&
                !Root.Instance.IsBindMail)
            {
                UserInterfaceSystem.That.ShowQueue<UIPlayerSubPhone>();
            }

            GuideSecondGame();
        }

        /// <summary>
        /// 5天内可以选择活动， 活动开始后 , 持续72小时
        /// </summary>
        /// <returns></returns>
        private bool IsTaskActivityOpen()
        {
            return MediatorTask.Instance.IsTaskActivityOpen();
        }

        /*/// <summary>
        /// 当前选择的活动是否处于开启状态
        /// </summary>
        /// <returns></returns>
        private bool IsTaskOpen()
        {
            return Root.Instance.CurTaskInfo.IsInOpenTime;
        }*/

        /// <summary>
        /// 第一次进入的目标房间索引
        /// </summary>
        private int targetRoomIndex = 1;

        /// <summary>
        /// 引导钻石局
        /// </summary>
        private int SecondRoomIndex
        {
            get
            {
                var showRoomList = Root.Instance.ShowRoomList;
                if (showRoomList == null)
                {
                    return -1;
                }

                var target = showRoomList.Find(room => room.IsDiamondRoom);

                if (target == null)
                {
                    return -1;
                }

                return target.id;
            }
        }

        /// <summary>
        /// 引导玩家第一次进入房间
        /// </summary>
        async UniTask GuideFirstGame()
        {
            IsFirstRoomGuideRunning = true;
            await UniTask.WaitUntil(() => RoomList.NumItems > 0);

            var data = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value;

            int index = -1;
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i] is Room roomI && roomI.id == targetRoomIndex)
                    index = i;
            }

            if (index < 0)
            {
                return;
            }

            ForbidSelfClick();

            await UniTask.Delay(TimeSpan.FromMilliseconds(250));

            ResumeSelfClick();

            await WaitRoomBtnClick(index);

            await UniTask.WaitUntil(() => !IsVisible || !IsResourceEnough);

            //等待对局结束 TODO 补充条件
            await UniTask.WaitUntil(() => IsUIMainTop() || !IsResourceEnough);

            GuideEnd();
        }

        private bool IsSecondRoomGuideRunning;

        bool IsGuideRunning()
        {
            return IsSecondRoomGuideRunning || IsFirstRoomGuideRunning;
        }

        async UniTask GuideSecondGame()
        {
            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.SECOND_BONUS_GAME))
            {
                if (Root.Instance.MatchHistoryCount >= 2)
                {
                    MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.SECOND_BONUS_GAME);
                    return;
                }

                IsSecondRoomGuideRunning = true;

                await SaveWaitTime();

                var data = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value;

                var secondGuideIndex = -1;
                //data.FindIndex(room => room.id == SecondRoomIndex);

                for (int i = 0; i < data.Count; ++i)
                {
                    if (data[i] is Room roomI && roomI.id == SecondRoomIndex)
                        secondGuideIndex = i;
                }

                if (secondGuideIndex < 0)
                {
                    IsSecondRoomGuideRunning = false;
                    return;
                }

                GuideText.text = I18N.Get("key_guide_10", 1000);
                await WaitRoomBtnClick(secondGuideIndex);
                MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.SECOND_BONUS_GAME);
                IsSecondRoomGuideRunning = false;
            }
        }

        private async UniTask SaveWaitTime()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => IsUIMainTop(), cancellationToken: token);

            await UniTask.Delay(TimeSpan.FromMilliseconds(200), cancellationToken: token);

            // await UniTask.NextFrame();

            await UniTask.WaitUntil(() => !UserInterfaceSystem.That.HaveUIInQueue(), cancellationToken: token);

            await UniTask.Delay(TimeSpan.FromMilliseconds(200), cancellationToken: token);

            await UniTask.WaitUntil(() => IsUIMainTop(), cancellationToken: token);
        }

        /// <summary>
        /// 不适合虚拟列表
        /// </summary>
        /// <param name="itemIndex"></param>
        private async UniTask WaitRoomBtnClick(int itemIndex)
        {
            GuideMask.SetActive(true);

            RoomList.ScrollToIndex(itemIndex);

            LoadFingerAtRoom(itemIndex);

            var childIndex = RoomList.ItemIndexToChildIndex(itemIndex);
            //虚拟列表刷新后的 引导对象
            var newGuideItem = RoomList.GetChildAt(childIndex);
            var heightLight = newGuideItem.AddComponent<AddHeightCanvas>();

            RoomListRect.vertical = false;
            var button = newGuideItem.GetComponent<RoomItemMono>().Button;

            await UniTask.WhenAny(button.OnClickAsync(), UniTask.WaitUntil(() => !IsUIMainTop()));

            ResumeSelfClick();

            RoomListRect.vertical = true;
            heightLight.enabled = false;

            heightLight.Destroy();

            GuideMask.SetActive(false);

            if (finger != null)
            {
                finger.SetActive(false);
            }
        }

        Transform GetGuideSpine()
        {
            return GuideMask.GetChild(0);
        }

        private async UniTask GuideClickMuseumToggle()
        {
            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.MUSEUM_GUIDE_1) &&
                Root.Instance.MuseumItems[0].State == 1)
            {
                if (IsUIMainInAnimation)
                {
                    await UniTask.WaitUntil(() => !IsUIMainInAnimation);
                }

                await SaveWaitTime();

                MediatorRequest.Instance.SendTriggerGuideStep(TriggerGuideStep.MUSEUM_GUIDE_1);
                LoadFinger(CollectionToggle.transform, new Vector3(-10, 20, 0));
                finger.transform.localScale = Vector3.one * 0.7f;
                var heightCanvas = CollectionToggle.gameObject.AddComponent<AddHeightCanvas>();

                GuideMask.SetActive(true);
                GuideText.text = I18N.Get("key_guide_museum_1");
                var maskBtn = GuideMask.GetComponent<MyButton>();

                await UniTask.WhenAny(CollectionToggle.OnValueChangedAsync(), maskBtn.OnClickAsync());
                finger.transform.localScale = Vector3.one;
                finger.SetActive(false);
                GuideMask.SetActive(false);

                heightCanvas.enabled = false;
            }
        }

        private async UniTask GuideClickMuseumReward()
        {
            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.MUSEUM_GUIDE_2) &&
                Root.Instance.MuseumItems[0].State == 1)
            {
                await UniTask.NextFrame();

                ForbidSelfClick();
                //等待动画完毕
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                ResumeSelfClick();

                MediatorRequest.Instance.SendTriggerGuideStep(TriggerGuideStep.MUSEUM_GUIDE_2);
                LoadFinger(MuseumFirstItem);

                var heightCanvas = MuseumFirstItem.gameObject.AddComponent<AddHeightCanvas>();

                GuideMask.SetActive(true);
                GuideText.text = I18N.Get("key_guide_museum_2");
                var maskBtn = GuideMask.GetComponent<MyButton>();
                var museumItemButton = MuseumFirstItem.GetComponent<MuseumItemMono>().BoxButton;

                maskBtn.SetClick(() => { museumItemButton.onClick?.Invoke(); });

                await UniTask.WhenAny(museumItemButton.OnClickAsync(), maskBtn.OnClickAsync());

                maskBtn.ClearClick();

                finger.SetActive(false);

                GuideMask.SetActive(false);

                heightCanvas.enabled = false;
            }
        }

        private bool MUSEUM_GUIDE_3_BEGIN = false;

        private async UniTask GuideAfterGetMuseumReward()
        {
            if (MUSEUM_GUIDE_3_BEGIN)
            {
                return;
            }

            MUSEUM_GUIDE_3_BEGIN = true;

            GuideText.text = I18N.Get("key_guide_museum_3");

            await UniTask.WaitUntil(() => IsUIMainTop());

            MediatorRequest.Instance.SendTriggerGuideStep(TriggerGuideStep.MUSEUM_GUIDE_3);

            GuideMask.SetActive(true);

            var maskBtn = GuideMask.GetComponent<MyButton>();

            await UniTask.WhenAny(maskBtn.OnClickAsync());

            GuideMask.SetActive(false);
        }

        private void RefreshActivity()
        {
            isShowComingSoon = true;

            RefreshTaskActivity();

            RefreshWheelEntry();

            RefreshDailyMission();

            RefreshRewardOnline();

            RefreshStarterPack();

            RefreshLuckyCard();

            RefreshDragon();

            RefreshSpecialGift();

            RefreshSpecialOffer();

            RefreshPiggyBank();

            RefreshJustForYou();

            RefreshBestOffer();

            RefreshComingSoon();

            RefreshDuel();

            RefreshMagicBallEntry();

            RefreshLuckyGuyEntry();

            RefreshActivityLayout();
        }

        private void LateUpdate()
        {
            float upPosY = 250;
            float downPosY = -160;
            var content = RoomListRect.transform.Find("ViewPort/#Content").GetComponent<RectTransform>();

            //ActivityGroup.y 和 content.y的插值
            var diff = -160;
            var animationDiff = 200;
            //YZDebug.Log("Pos Y = " + content.anchoredPosition.y + " Vy = " + RoomListRect.velocity.y);

            if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                var activityGroupRect = ActivityGroup.GetComponent<RectTransform>();

                if (content.anchoredPosition.y < animationDiff)
                {
                    DOTween.Kill(activityGroupRect, false);
                    activityGroupRect.DOAnchorPosY(downPosY, 0.5f).SetEase(Ease.OutQuart);
                    // if (activityGroupRect.anchoredPosition.y - content.anchoredPosition.y < diff)
                    // {
                    //     var y = Math.Clamp(content.anchoredPosition.y + diff, downPosY, upPosY);
                    //     activityGroupRect.anchoredPosition = new Vector2(activityGroupRect.anchoredPosition.x, y);
                    // }
                    return;
                }
                
                if (content.anchoredPosition.y > animationDiff && RoomListRect.velocity.y > 0)
                {
                    DOTween.Kill(activityGroupRect, false);
                    activityGroupRect.DOAnchorPosY(upPosY, 0.5f).SetEase(Ease.OutQuart);
                    return;
                }

                if (RoomListRect.velocity.y > 250 && RoomListRect.velocity.y < 4000)
                {
                    DOTween.Kill(activityGroupRect, false);
                    activityGroupRect.DOAnchorPosY(upPosY, 0.5f).SetEase(Ease.OutQuart);
                }
                else if (RoomListRect.velocity.y < -20)
                {
                    DOTween.Kill(activityGroupRect, false);
                    activityGroupRect.DOAnchorPosY(downPosY, 0.5f).SetEase(Ease.OutQuart);
                }
            }
        }

        private void RefreshDuel()
        {
            if (Root.Instance.MatchHistoryCount >= 7 && !Root.Instance.IsNaturalFlow)
            {
                DuelEntryBtn.SetActive(true);
                DuelEntryBtn.TryToShowDuelBubble();
            }
            else
            {
                DuelEntryBtn.SetActive(false);
            }
        }

        private void RefreshComingSoon()
        {
            ComingSoonTrans.SetActive(isShowComingSoon);
        }

        private void RefreshActivityLayout()
        {
            try
            {
                HoldImage.SetActive(EnabledActivityCount == 0);
                var towLine = EnabledActivityCount > OneActivityRowCount;
                secondMiddle.SetActive(towLine);
                secondFill.SetActive(towLine);
                //2行以上
                if (towLine)
                {
                    //活动显示2行
                    RoomListRect.transform.rectTransform().offsetMax =
                        new Vector2(RoomRectOffsetMax.x, RoomRectOffsetMax.y - 160);
                    activityLayoutGroup.padding.bottom = 54;
                }
                else
                {
                    //活动显示1行
                    RoomListRect.transform.rectTransform().offsetMax = RoomRectOffsetMax;
                    activityLayoutGroup.padding.bottom = 210;
                }

                //超过了两行
                if (EnabledActivityCount > OneActivityRowCount * 2)
                {
                    ActivityScrollRect.vertical = true;
                    ActivityScrollRect.enabled = true;
                }
                else
                {
                    ActivityScrollRect.enabled = false;
                }

                //超过了两行
                if (EnabledActivityCount > OneActivityRowCount * 2)
                {
                    ActivityScrollRect.vertical = true;
                    ActivityScrollRect.enabled = true;
                }
                else
                {
                    ActivityScrollRect.enabled = false;
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
            }
        }

        private void RefreshPiggyBank()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.PiggyBank))
                {
                    PiggyBankBtn.SetActive(false);
                    return;
                }

                //存钱罐活动
                bool isPiggyBankOpen = MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank);
                PiggyBankBtn.SetActive(isPiggyBankOpen);
                if (isPiggyBankOpen)
                {
                    var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.PiggyBank);
                    PiggyBankBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(lessTime);
                }
                else
                {
                    if (reqSpecialTime == 0)
                    {
                        // 存钱罐结束后请求一次special gift
                        MediatorRequest.Instance.UpdateSpecialGiftInfo();
                        reqSpecialTime = -1;
                    }
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                PiggyBankBtn.SetActive(false);
            }
        }

        private void RefreshSpecialGift()
        {
            try
            {
                // 特殊礼包活动
                var spLessTime = Root.Instance.Role.specialGiftInfo.special_gift_end_time -
                                 TimeUtils.Instance.UtcTimeNow;
                bool isSpecialGiftOpen = Root.Instance.Role.specialGiftInfo?.special_gift_chance > 0
                                         && Root.Instance.Role.specialGiftInfo?.special_gift_today_chance > 0
                                         && spLessTime > 0;

                SpecialGiftBtn.SetActive(isSpecialGiftOpen);
                if (isSpecialGiftOpen)
                {
                    var image = SpecialGiftBtn.gameObject.FindChild("Activity Icon");

                    var A = image.FindChild("A");
                    A.SetActive(false);
                    var B = image.FindChild("B");
                    B.SetActive(false);
                    var C = image.FindChild("C");
                    C.SetActive(false);

                    int week = Root.Instance.Role.specialGiftInfo.today_week.ToInt();
                    if (week == 1 || week == 2)
                    {
                        SpecialGiftBtn.targetGraphic = A.GetComponent<Graphic>();
                        A.SetActive(true);
                    }
                    else if (week == 3 || week == 4 || week == 5)
                    {
                        SpecialGiftBtn.targetGraphic = B.GetComponent<Graphic>();
                        B.SetActive(true);
                    }
                    else if (week == 6 || week == 0)
                    {
                        SpecialGiftBtn.targetGraphic = C.GetComponent<Graphic>();
                        C.SetActive(true);
                    }

                    SpecialGiftBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.FormatTimeToTomorrow();

                    // 特殊礼包的 Banner
                    if (week == 1 || week == 2)
                    {
                        if (!SpecialGiftATrans.IsActive())
                        {
                            SpecialGiftATrans.SetActive(true);
                            SpecialGiftATrans.GetComponent<SpecialGiftMonoA>().Init();
                        }
                    }
                    else if (week == 3 || week == 4 || week == 5)
                    {
                        if (!SpecialGiftBTrans.IsActive())
                        {
                            SpecialGiftBTrans.SetActive(true);
                            SpecialGiftBTrans.GetComponent<SpecialGiftMonoB>().Init();
                        }
                    }
                    else if (week == 6 || week == 0)
                    {
                        if (!SpecialGiftCTrans.IsActive())
                        {
                            SpecialGiftCTrans.SetActive(true);
                            SpecialGiftCTrans.GetComponent<SpecialGiftMonoC>().Init();
                        }
                    }

                    // 有banner就不显示 comingsoon banner
                    isShowComingSoon = false;
                }
                else
                {
                    SpecialGiftATrans.SetActive(false);
                    SpecialGiftBTrans.SetActive(false);
                    SpecialGiftCTrans.SetActive(false);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                SpecialGiftBtn.SetActive(false);
                SpecialGiftATrans.SetActive(false);
                SpecialGiftBTrans.SetActive(false);
                SpecialGiftCTrans.SetActive(false);
            }
        }

        private void RefreshSpecialOffer()
        {
            if (Root.Instance.Role.SpecialOfferInfo == null)
            {
                SpecialOfferBtn.SetActive(false);
                return;
            }

            var lessTime = Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow;
            if (Root.Instance.Role.SpecialOfferInfo != null &&
                Root.Instance.Role.SpecialOfferInfo.show_time > 0 &&
                lessTime > 0 &&
                Root.Instance.ChargeInfo.success_total <= 0)
            {
                SpecialOfferBtn.SetActive(true);
                SpecialOfferBtn.transform.Find("TextGroup/Text").GetComponent<Text>().text =
                    TimeUtils.Instance.ToHourMinuteSecond(lessTime);
            }
            else
            {
                SpecialOfferBtn.SetActive(false);
            }
        }

        private void RefreshDragon()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.Dragon))
                {
                    DragonBtn.SetActive(false);
                    return;
                }

                if (Root.Instance.Role.dragonInfo == null)
                    return;

                // 一条龙活动
                var dragonlessTime = Root.Instance.Role.dragonInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
                bool isDragonOpen = Root.Instance.Role.dragonInfo?.one_stop_today_chance == 1 && dragonlessTime > 0;
                DragonBtn.SetActive(isDragonOpen);
                if (isDragonOpen)
                {
                    DragonBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(dragonlessTime);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                DragonBtn.SetActive(false);
            }
        }

        private void RefreshLuckyCard()
        {
            try
            {
                // throw new Exception();
                // 幸运卡活动
                var luckylessTime = Root.Instance.Role.luckyCardInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
                bool isLuckyCardOpen = luckylessTime > 0 && Root.Instance.Role.luckyCardInfo.lucky_card_chance > 0;
                LuckyCardBtn.SetActive(isLuckyCardOpen);
                if (isLuckyCardOpen)
                {
                    LuckyCardBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(luckylessTime);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                LuckyCardBtn.SetActive(false);
            }
        }

        private void RefreshStarterPack()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.StartPacker))
                {
                    StarterPackBtn.SetActive(false);
                    return;
                }

                //首充活动 Banner
                var isStarterPackBegin = MediatorActivity.Instance.IsActivityBegin(ActivityType.StartPacker);
                StarterPackBtn.SetActive(isStarterPackBegin);
                StarterPackerTrans.SetActive(isStarterPackBegin);
                if (isStarterPackBegin)
                {
                    var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.StartPacker);

                    StarterPackerTrans.GetComponent<StarterPackMono>().TimeText.text =
                        TimeUtils.Instance.ToHourMinuteSecond(lessTime);

                    StarterPackBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(lessTime);

                    // 有banner就不显示 comingsoon banner
                    isShowComingSoon = false;
                }

                if (isStarterPackBegin && !StarterPackerTrans.IsActive())
                {
                    StarterPackerTrans.GetComponent<StarterPackMono>().Init();
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                StarterPackBtn.SetActive(false);
                StarterPackerTrans.SetActive(false);
            }
        }

        private void RefreshRewardOnline()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.OnlineReward))
                {
                    OnlineRewardBtn.SetActive(false);
                    return;
                }
                else
                {
                    if (!OnlineRewardBtn.IsActive())
                    {
                        OnlineRewardBtn.SetActive(true);
                        RefreshRedPoint();
                    }
                }

                //每日奖励 
                var timeSpan = Root.Instance.OnlineRewardInfo.LessTime;
                var onlineRewardText = OnlineRewardBtn.gameObject.FindChild<Text>("TextGroup/Text");

                if (Root.Instance.OnlineRewardInfo.GetAllReward)
                {
                    // OnlineRewardBtn.Gray = true;
                    //到当天结束的时间
                    onlineRewardText.text =
                        TimeUtils.Instance.ToHourMinuteSecond(TimeUtils.Instance.EndDayTimeStamp -
                                                              TimeUtils.Instance.UtcTimeNow);
                }
                else if (timeSpan > 0)
                {
                    // OnlineRewardBtn.Gray = true;
                    onlineRewardText.text = TimeUtils.Instance.ToHourMinuteSecond(timeSpan);
                }
                else
                {
                    // OnlineRewardBtn.Gray = false;
                    onlineRewardText.text = I18N.Get("key_go_claim");
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
            }
        }

        private void RefreshMagicBallEntry()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.MagicBall))
                {
                    MagicBallBtn.SetActive(false);
                    return;
                }


                //如果点数超过总点数， 且所有奖励已领取

                var isActivityOpen = MediatorActivity.Instance.IsActivityOpen(ActivityType.MagicBall);

                if (isActivityOpen)
                {
                    var lesstime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.MagicBall);

                    if (lesstime > 0)
                    {
                        if (!MagicBallBtn.IsActive())
                        {
                            MagicBallCountText.text = Root.Instance.MagicBallInfo?.CurrentPoint.ToString();
                        }

                        MagicBallBtn.SetActive(true);
                        MagicBallBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                            TimeUtils.Instance.ToDayHourMinuteSecond(lesstime);
                    }
                    else
                    {
                        MagicBallBtn.SetActive(false);
                    }
                }
                else
                {
                    MagicBallBtn.SetActive(false);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                MagicBallBtn.SetActive(false);
            }
        }

        private void RefreshLuckyGuyEntry()
        {
            if (LuckyGuyBtn == null)
            {
                return;
            }

            try
            {
                var isActivityOpen = MediatorActivity.Instance.IsActivityOpen(ActivityType.LuckyGuy);

                if (isActivityOpen)
                {
                    LuckyGuyBtn.SetActive(true);
                    var refreshCountTime = Root.Instance.LuckyGuyInfo.RefreshCountTime;
                    var luckyGuyText = LuckyGuyBtn.gameObject.FindChild<Text>("TextGroup/Text");
                    luckyGuyText.text = TimeUtils.Instance.ToDayHourMinuteSecond(refreshCountTime);
                }
                else
                {
                    LuckyGuyBtn.SetActive(false);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                MagicBallBtn.SetActive(false);
            }
        }

        private void RefreshTaskActivity()
        {
            if (Root.Instance.IsNaturalFlow)
            {
                TaskProgressBtn.SetActive(false);
                return;
            }

            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.TaskSystem))
                {
                    TaskBtn.SetActive(false);
                    TaskProgressBtn.SetActive(false);
                    return;
                }

                if (Root.Instance.MaybeInTaskTime)
                {
                    var isTaskActivityOpen = IsTaskActivityOpen();
                    if (isTaskActivityOpen)
                    {
                        TaskBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                            TimeUtils.Instance.ToHourMinuteSecond(Root.Instance.CurTaskInfo.LessTime);
                    }

                    TaskBtn.SetActive(isTaskActivityOpen);
                }
                else
                {
                    TaskBtn.SetActive(false);
                }

                TaskProgressBtn.SetActive(MediatorTask.Instance.IsChooseTask());

                if (Root.Instance.CurTaskInfo != null)
                {
                    var progress = Root.Instance.CurTaskInfo.FinishProgress;
                    TaskProgressBtn.gameObject.FindChild<Text>("Text").text =
                        $"{progress}/{Root.Instance.CurTaskInfo.AllTaskCount}";
                    TaskProgressBtn.gameObject.FindChild<Image>("progress").fillAmount =
                        (float)progress / Root.Instance.CurTaskInfo.AllTaskCount;
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                TaskBtn.SetActive(false);
                TaskProgressBtn.SetActive(false);
            }
        }

        private void RefreshWheelEntry()
        {
            try
            {
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.FortuneWheel))
                {
                    WheelBtn.SetActive(false);
                }
                else
                {
                    WheelBtn.SetActive(true);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                WheelBtn.SetActive(false);
            }
        }

        private void RefreshDailyMission()
        {
            /*try
            {
                if (MediatorActivity.Instance.IsActivityOpen(ActivityType.DailyMission))
                {
                    DailyTaskBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.FormatTimeToTomorrow();
                    DailyTaskBtn.SetActive(true);
                }
                else
                {
                    DailyTaskBtn.SetActive(false);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                DailyTaskBtn.SetActive(false);
            }*/
        }

        enum vname
        {
            Role,
            RoomListData,
            MatchHistoryData,
            FreeTicked,
            Page,

            /// <summary>
            /// 未完成的历史记录是否展开
            /// </summary>
            InCompleteOn,

            /// <summary>
            /// 已完成的历史记录是否展开
            /// </summary>
            CompleteOn,

            /// <summary>
            /// 1v1房间折叠列表是否展开
            /// </summary>
            Seat2On,

            /// <summary>
            /// 5人房间折叠列表是否展开
            /// </summary>
            Seat5On
        }

        private ArrayList Historydata = new ArrayList();

        public override void InitVm()
        {
            var seat2On = GetVmValue<bool>(vname.Seat2On.ToString(), true, out _);
            vm[vname.Seat2On.ToString()] = new ReactivePropertySlim<bool>(seat2On);

            var seat5On = GetVmValue<bool>(vname.Seat5On.ToString(), true, out _);
            vm[vname.Seat5On.ToString()] = new ReactivePropertySlim<bool>(seat5On);

            //拷贝
            vm[vname.Role.ToString()] = new ReactivePropertySlim<Role>(Root.Instance.Role);
            vm[vname.FreeTicked.ToString()] = new ReactivePropertySlim<int>(Root.Instance.WheelFreeTicket);
            vm[vname.RoomListData.ToString()] = new ReactivePropertySlim<ArrayList>(GetOrganizedList());

            var page = GetVmValue(vname.Page.ToString(), 1, out var _);
            vm[vname.Page.ToString()] = new ReactivePropertySlim<int>(page);

            var inCompleteOn = GetVmValue<bool>(vname.InCompleteOn.ToString(), true, out var _);
            vm[vname.InCompleteOn.ToString()] = new ReactivePropertySlim<bool>(inCompleteOn);

            var completeOn = GetVmValue<bool>(vname.CompleteOn.ToString(), true, out var _);
            vm[vname.CompleteOn.ToString()] = new ReactivePropertySlim<bool>(completeOn);

            Historydata = GetHistoryListData();
            vm[vname.MatchHistoryData.ToString()] = new ReactivePropertySlim<ArrayList>(Historydata);
        }

        public override void InitBinds()
        {
            int lastPage = -1;
            vm[vname.Page.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                foreach (var page in Pages)
                {
                    page.SetActive(false);
                }

                if (value == 0)
                {
                    recordRewardGroup.SetActive(false);
                }

                //从商店去对局界面或者大厅
                if (lastPage == 2 && (value == 0 || value == 1))
                {
                    if (IsUIMainTop())
                    {
                        MediatorActivity.Instance.PopActivityRandomAtStore();
                    }
                }

                Pages[value].SetActive(true);

                if (value == 0)
                {
                    HistoryToggle.isOn = true;
                }
                else if (value == 1)
                {
                    PlayRoomListAnimation();
                    RoomList.ScrollToTop();
                    SendRefreshLuckyGuy();

                    roomToggle.isOn = true;
                }
                else if (value == 2)
                {
                    StoreToggle.isOn = true;
                }
                else if (value == 3)
                {
                    GuideClickMuseumReward();
                    CollectionToggle.isOn = true;
                }

                if (value != 1)
                {
                    HidePiggyFullTip();
                }
                
                MovePage(value, lastPage);
                lastPage = value;
            });

            vm[vname.FreeTicked.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                WheelBtn.gameObject.FindChild<Text>("TextGroup/Text").text = $"{value}/{Const.FREE_WHEEL_COST}";
            });

            vm[vname.Role.ToString()].ToIObservable<Role>().Subscribe(_ =>
            {
                var role = Root.Instance.Role;
                if (role == null)
                {
                    return;
                }

                role.LoadIcon(PlayerImage);
                role.LoadIcon(Pop_Res_Tip.gameObject.FindChild<Image>("PlayerIcon/mask/Icon"));

                RefreshItemContent(Const.Bonus);
                RefreshItemContent(Const.Chips);
                RefreshItemContent(Const.Coin);
            });

            vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Subscribe(value =>
            {
                // RoomList.Clear();
                RoomList.NumItems = value.Count;
            });

            vm[vname.MatchHistoryData.ToString()].ToIObservable<ArrayList>().Subscribe(value =>
            {
                if (value == null || value.Count == 0)
                {
                    return;
                }

                var existNotClaimRecord = ExistNotClaimRecord();
                CollectAllBtn.SetActive(existNotClaimRecord);

                if (existNotClaimRecord)
                {
                    MatchHistoryRect.transform.rectTransform().offsetMin =
                        new Vector2(HistoryRectOffsetMin.x, HistoryRectOffsetMin.y + 200);
                }
                else
                {
                    MatchHistoryRect.transform.rectTransform().offsetMin = HistoryRectOffsetMin;
                }


                ListEmptyText.SetActive(!haveAnyRecord);

                MatchHistoryList.NumItems = value.Count;
            });
        }

        private Sequence moveSelectSeq;

        private void MovePage(int currentPage, int lastPage)
        {
            float anchorPosX;

            switch (currentPage)
            {
                case 0:
                    anchorPosX = -274;
                    break;
                case 1:
                    anchorPosX = -93.6f;
                    break;
                case 2:
                    anchorPosX = 93.1f;
                    break;
                case 3:
                    anchorPosX = 273.7f;
                    break;
                default:
                    return;
            }


            moveSelectSeq?.Kill();
            moveSelectSeq = DOTween.Sequence()
                .PrependInterval(0.15f).PrependCallback(() =>
                {
                    switch (currentPage)
                    {
                        case 0:
                            HistoryToggle.GetComponent<AnimationSequence>().Play();
                            break;
                        case 1:
                            roomToggle.GetComponent<AnimationSequence>().Play();
                            break;
                        case 2:
                            StoreToggle.GetComponent<AnimationSequence>().Play();
                            break;
                        case 3:
                            CollectionToggle.GetComponent<AnimationSequence>().Play();
                            break;
                    }
                })
                .Join(select.GetComponent<RectTransform>().DOAnchorPosX(anchorPosX, 0.2f));

            //都有被中断的可能
            switch (lastPage)
            {
                case 0:
                    ToggleBackAnimation(HistoryToggle.transform);
                    break;
                case 1:
                    //animation 被ui打断 后 ， 居然不能PlayBackwards
                    ToggleBackAnimation(roomToggle.transform);
                    break;
                case 2:
                    ToggleBackAnimation(StoreToggle.transform);
                    break;
                case 3:
                    ToggleBackAnimation(CollectionToggle.transform);
                    break;
            }
        }

        private void ToggleBackAnimation(Transform transform)
        {
            var rectTransform = transform.GetComponent<RectTransform>();
            DOTween.Sequence().Append(rectTransform.DOAnchorPosY(0, 0.2f))
                .Join(rectTransform.DOScale(1, 0.2f))
                .PrependCallback(() => transform.gameObject.FindChild("Text").SetActive(false));
        }

        private static bool ExistNotClaimRecord()
        {
            return Root.Instance.MatchHistory.Exists(history => history.CanClaim);
        }

        private void OnEnable()
        {
            if (!IsInitEnd)
            {
                return;
            }

            if (IsPanelAt(roomPageIndex))
            {
                SendRefreshLuckyGuy();
                RoomList?.ScrollToTop();
                PlayRoomListAnimation();
            }

            DOTween.Kill(ActivityGroup.GetComponent<RectTransform>(), false);
            ActivityGroup.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ActivityGroup.GetComponent<RectTransform>().anchoredPosition.x, -160f);
        }

        void SendRefreshLuckyGuy()
        {
            //需要走过引导才有该活动
            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.SECOND_BONUS_GAME))
            {
                return;
            }

            if (MediatorActivity.Instance.IsActivityOpen(ActivityType.LuckyGuy))
            {
                return;
            }

            if (Root.Instance.LuckyGuyInfo != null && Root.Instance.LuckyGuyInfo.IsFinish(false))
            {
                return;
            }

            if (MediatorActivity.Instance.IsActivityBegin(ActivityType.LuckyGuy, checkOpen: false))
            {
                MediatorRequest.Instance.RefreshLuckyGuy();
            }
        }

        private void PlayRoomListAnimation()
        {
            if (RoomList != null)
            {
                for (int i = 0; i < RoomList.NumItems; i++)
                {
                    var child = RoomList.GetChildAt(i);
                    if (child == null)
                    {
                        continue;
                    }

                    var roomMono = child.GetComponent<RoomItemMono>();
                    var roomTransform = child.GetComponent<Transform>();
                    var scrollListItem = child.GetComponent<ScrollListItem>();
                    
                    if (roomMono != null)
                    {
                        roomMono.PlayAnimation();
                        roomTransform.localScale = Vector3.zero;
                        this.AttachTimer(scrollListItem.index * 0.05f, roomMono.ShowAnimation.Play);
                    }
                }

                this.AttachTimer(0.05f, () =>
                {
                    for (int i = 0; i < RoomList.NumItems; i++)
                    {
                        var child = RoomList.GetChildAt(i);
                        if (child == null)
                        {
                            continue;
                        }

                        var roomMono = child.GetComponent<RoomItemMono>();
                        var roomTransform = child.GetComponent<Transform>();
                        var scrollListItem = child.GetComponent<ScrollListItem>();
                    
                        if (scrollListItem != null)
                        {
                            roomTransform.localScale = Vector3.zero;
                            this.AttachTimer(scrollListItem.index * 0.05f, roomMono.ShowAnimation.Play);
                        }
                    }
                });
            }
        }

        private void InitRoomList()
        {
            var verticalLayoutGroup = RoomListRect.content.GetComponent<VerticalLayoutGroup>();
            RoomList = new MyList
            {
                ScrollRect = RoomListRect,
                defaultItem = $"{ClassType.Name}/RoomItem",

                itemProvider = index =>
                {
                    var data = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is Room)
                    {
                        return $"{ClassType.Name}/RoomItem";
                    }
                    else if (data is title title2 && title2.order == 0)
                    {
                        return $"{ClassType.Name}/TitleGroupRoom2";
                    }
                    else
                    {
                        return $"{ClassType.Name}/TitleGroupRoom5";
                    }
                },

                itemRenderer = (index, item) =>
                {
                    var data = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is Room room)
                    {
                        RenderRoom(item.GetComponent<RoomItemMono>(), room);
                        item.transform.TryGetComponent<AddHeightCanvas>(out var heightCanvas);
                        if (heightCanvas != null)
                        {
                            heightCanvas.SetNewSorting();
                        }
                    }
                    else if (data is title title)
                    {
                        var titleGroupMono = item.GetComponent<TitleGroupMono>();
                        titleGroupMono.Title.text = title.textContent;

                        if (title.order == 0)
                        {
                            titleGroupMono.IsDown = vm[vname.Seat2On.ToString()].ToIObservable<bool>().Value;

                            titleGroupMono.Btn.SetClick(() =>
                            {
                                titleGroupMono.IsDown = !titleGroupMono.IsDown;
                                vm[vname.Seat2On.ToString()].ToIObservable<bool>().Value = titleGroupMono.IsDown;
                                titleGroupMono.transform.Find("ArrowText").GetComponent<MyText>().text =
                                    titleGroupMono.IsDown ? "Show Less" : "Show More";
                                RefreshRoomList();
                            });
                        }
                        else
                        {
                            titleGroupMono.IsDown = vm[vname.Seat5On.ToString()].ToIObservable<bool>().Value;

                            titleGroupMono.Btn.SetClick(() =>
                            {
                                titleGroupMono.IsDown = !titleGroupMono.IsDown;
                                vm[vname.Seat5On.ToString()].ToIObservable<bool>().Value = titleGroupMono.IsDown;
                                titleGroupMono.transform.Find("ArrowText").GetComponent<MyText>().text =
                                    titleGroupMono.IsDown ? "Show Less" : "Show More";
                                RefreshRoomList();
                            });
                        }
                    }
                },

                itemSizeProvider = index =>
                {
                    var data = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is Room)
                    {
                        return new Vector2(725, 220);
                    }
                    else
                    {
                        return new Vector2(750f, 75f);
                    }
                },
                PaddingTop = verticalLayoutGroup.padding.top,
                PaddingBottom = verticalLayoutGroup.padding.bottom,
                //和roomList 上rect mask 2D的数值一样 
                TopOffset = -400,
                lineGap = verticalLayoutGroup.spacing
            };
            RoomList.SetVirtual();
        }

        public void RenderRoom(RoomItemMono roomMono, Room room, bool not_check_res = false)
        {
            if (roomMono == null || room == null)
            {
                return;
            }

            roomMono.BeforeInit();
            roomMono.PlayAnimation();
            roomMono.MuseumCount = (int)room.museum_point;
            roomMono.MagicCount = room.wizard_treasure_point;
            roomMono.TitleText.text = I18N.Get(room.title);
            roomMono.NameText.text = I18N.Get(room.name);
            roomMono.TitleBG.sprite = MediatorBingo.Instance.GetSpriteByUrl(room.TitleBG);
            roomMono.SubTitleText.text = I18N.Get(room.sub_title);
            roomMono.gameObject.FindChild<Image>("root/bg0").sprite = MediatorBingo.Instance.GetSpriteByUrl(room.Bg);
            roomMono.DescText.text =
                room.seat == 2 ? I18N.Get("TWO_SEAT") : I18N.Get("PLAYER_NUM", room.seat);

            roomMono.DescText.color = room.DescTextColor;
            // roomMono.gameObject.FindChild<Text>("root/RewardDesc").color = room.RewardDescColor;
            roomMono.RoomIcon.sprite = MediatorBingo.Instance.GetSpriteByUrl(room.Icon);
            roomMono.RoomIcon.SetNativeSize();
            roomMono.roomId = room.id;
            roomMono.LastContestGroup.SetActive(Root.Instance.UserInfo.last_cash_room == room.id);
            roomMono.IsMulti = room.IsMulti;
            var topReward = room.GetRankReward(1);
            if (topReward.Any())
            {
                var item1 = topReward[0];
                if (item1 != null)
                {
                    roomMono.RewardIcon.sprite = item1.GetIcon();

                    MediatorItem.Instance.SetItemText(item1.id, roomMono.RewardText);
                }
            }

            roomMono.RewardText.text = GameUtils.TocommaStyle(room.PrizePool);

            //代码耦合太多, 不提出来了
            if (room.IsNormal)
            {
                roomMono.IsNormal = true;
                int needCount = 0;
                int itemType = -1;

                roomMono.IsFree = room.IsFree;

                //设置入场费
                foreach (var inItem in room.in_items)
                {
                    needCount = (int)inItem.Value;
                    itemType = inItem.Key;
                    roomMono.CostIcon.sprite = MediatorItem.Instance.GetItemSprite(itemType);
                    roomMono.CostText.text = GameUtils.TocommaStyle(inItem.Value);
                    MediatorItem.Instance.SetItemText(itemType, roomMono.CostText);
                }

                void OnNormalRoomPlayBtnClick()
                {
                    if (itemType is Const.Bonus or Const.Cash)
                    {
                        // 冻结的账号
                        if (Root.Instance.Role.IsFreeze)
                        {
                            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                            {
                                Type = UIConfirmData.UIConfirmType.OneBtn,
                                HideCloseBtn = false,
                                desc = I18N.Get("key_unusual_activity"),
                                confirmTitle = I18N.Get("key_contact_us"),
                                WaitCloseCallback = true,
                                confirmCall = () => { YZNativeUtil.ContactYZUS(EmailPos.Charge); },
                                cancleCall = () => { UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm)); }
                            });
                            // YZDebug.Log("账号冻结");
                            return;
                        }

                        if (Root.Instance.illegalityPeople)
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_illegalityPeople"));
                            return;
                        }
                        
                        bool checkLocation = false;
                        // 检查定位是否开启
                        LocationManager.Shared.IsLocationValid(YZSafeType.Money, null, -1, () =>
                        {
                            // 关闭等待界面
                            UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                            YZNativeUtil.GetYZLocation(true,
                                (temp) => { YZDebug.LogConcat("真机定位: ", temp); });
                            checkLocation = true;

                            YZDebug.Log("room 定位开启 true");

                            if (!checkLocation)
                                return;
#if RELEASE || RTEST
        // 检查具体定位
                                    string countryCode = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode).ToUpper();
                                    if (countryCode != "US" && Root.Instance.Role.white == 0)
                                    {
                                        // 定位非美国  且不在白名单内
                                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_region_game_limit"));
                                        return;
                                    }
#endif

                            //检查资源是否足够
                            if (not_check_res || MediatorItem.Instance.CheckResEnough(itemType, needCount))
                            {
                                IsResourceEnough = true;
                                YZLog.LogColor($"进入房间 开始匹配 room_id= {room.id}");
                                YZDebug.Log("进入房间 开始匹配");
                                MediatorRequest.Instance.MatchBegin(room, IsFirstRoomGuideRunning);
                            }
                            else
                            {
                                IsResourceEnough = false;
                                // UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("RES_NOT_ENOUGH"));
                                MediatorItem.Instance.ResNotEnoughGoTo(itemType, room);
                            }
                        });
                    }
                    else
                    {
                        //检查资源是否足够 -1?
                        if (not_check_res || itemType == -1 ||
                            MediatorItem.Instance.CheckResEnough(itemType, needCount))
                        {
                            IsResourceEnough = true;
                            YZLog.LogColor($"进入房间 开始匹配 room_id= {room.id}");
                            YZDebug.Log("进入房间 开始匹配");
                            MediatorRequest.Instance.MatchBegin(room, IsFirstRoomGuideRunning);
                        }
                        else
                        {
                            IsResourceEnough = false;
                            MediatorItem.Instance.ResNotEnoughGoTo(itemType, room);
                        }
                    }
                }

                Action action = OnNormalRoomPlayBtnClick;
                roomMono.RootBtn.SetClick(() =>
                {
                    if (IsGuideRunning())
                    {
                        return;
                    }

                    UserInterfaceSystem.That.ShowUI<UIRoomEntry>(room, action);
                });

                roomMono.Button.SetClick(OnNormalRoomPlayBtnClick);
            }
            else
            {
                roomMono.RootBtn.SetClick(() =>
                {
                    if (IsGuideRunning())
                    {
                        return;
                    }

                    UserInterfaceSystem.That.ShowUI<UIRoomEntry>(room);
                });
                roomMono.InitSpecialRoom(room);
            }
        }

        private void InitMatchHistoryList()
        {
            MatchHistoryList = new MyList
            {
                ScrollRect = MatchHistoryRect,
                defaultItem = $"{ClassType.Name}/MatchHistoryItem",
                lineGap = 10,
                PaddingBottom = 40,
                OnPullDownRelease = (data) =>
                {
                    MediatorRequest.Instance.GetInCompleteHistory(true,
                        () => UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_list_refresh")));
                },

                itemProvider = index =>
                {
                    var data = vm[vname.MatchHistoryData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is MatchHistory)
                    {
                        return $"{ClassType.Name}/MatchHistoryItem";
                    }

                    else
                    {
                        return $"{ClassType.Name}/TitleGroup";
                    }
                },

                itemSizeProvider = index =>
                {
                    var data = vm[vname.MatchHistoryData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is MatchHistory)
                    {
                        return new Vector2(750, 170);
                    }
                    else
                    {
                        return new Vector2(750, 100);
                    }
                },

                itemRenderer = (index, item) =>
                {
                    var data = vm[vname.MatchHistoryData.ToString()].ToIObservable<ArrayList>().Value[index];
                    if (data is MatchHistory history)
                    {
                        var matchHistoryMono = item.GetComponent<MatchHistoryItem>();
                        matchHistoryMono.Init(history);
                        if (history.PullFlag)
                        {
                            if (history.NotFinish)
                            {
                                MediatorRequest.Instance.GetInCompleteHistory();
                            }
                            else
                            {
                                MediatorRequest.Instance.GetCompleteHistory();
                            }
                        }
                    }

                    if (data is title title)
                    {
                        var titleGroupMono = item.GetComponent<TitleGroupMono>();
                        titleGroupMono.Title.text = title.textContent;

                        if (title.order == 0)
                        {
                            titleGroupMono.IsDown = vm[vname.InCompleteOn.ToString()].ToIObservable<bool>().Value;

                            titleGroupMono.Btn.SetClick(() =>
                            {
                                titleGroupMono.IsDown = !titleGroupMono.IsDown;
                                vm[vname.InCompleteOn.ToString()].ToIObservable<bool>().Value = titleGroupMono.IsDown;
                                RefreshMatchHistory();
                            });
                        }
                        else
                        {
                            titleGroupMono.IsDown = vm[vname.CompleteOn.ToString()].ToIObservable<bool>().Value;

                            titleGroupMono.Btn.SetClick(() =>
                            {
                                titleGroupMono.IsDown = !titleGroupMono.IsDown;
                                vm[vname.CompleteOn.ToString()].ToIObservable<bool>().Value = titleGroupMono.IsDown;
                                RefreshMatchHistory();
                            });
                        }
                    }
                }
            };
            MatchHistoryList.SetVirtual();
        }

        /// <summary>
        /// 货币入账震动计时器
        /// </summary>
        private IDisposable vibrateDisposable;

        private void RefreshItemContent(int constId)
        {
            var role = Root.Instance.Role;
            switch (constId)
            {
                case Const.Bonus:
                case Const.Cash:
                    bonusTweener?.Kill();
                    // var bonusContent = GameUtils.TocommaStyle(role.GetDollars());
                    var bonusContent = Math.Round(role.GetDollars(), 2).ToString();
                    var bonusText = BonusBtn.transform.Find("Title").GetComponent<Text>();
                    bonusText.text = bonusContent;
                    var pop_bonus_text = Pop_Res_Tip.gameObject.FindChild<Text>("resource bar/BonusBtn/Title");
                    pop_bonus_text.text = bonusContent;

                    if (!IsInitEnd)
                    {
                        MediatorItem.Instance.SetItemText(Const.Bonus, bonusText);
                        MediatorItem.Instance.SetItemText(Const.Bonus, pop_bonus_text);
                    }

                    break;
                case Const.Coin:
                    coinTweener?.Kill();
                    // var coinTextContent = GameUtils.TocommaStyle(role.GetItemCount(Const.Coin));
                    var coinTextContent = Math.Round(role.GetItemCount(Const.Coin), 2).ToString();
                    var pop_coin_text = Pop_Res_Tip.gameObject.FindChild<Text>("resource bar/TokenBtn/Title");
                    var coinText = CoinBtn.transform.Find("Title").GetComponent<Text>();
                    coinText.text = pop_coin_text.text = coinTextContent;
                    if (!IsInitEnd)
                    {
                        MediatorItem.Instance.SetItemText(Const.Coin, coinText);
                        MediatorItem.Instance.SetItemText(Const.Coin, pop_coin_text);
                    }

                    break;
                case Const.Chips:
                    diamondTweener?.Kill();
                    // var gemsTextContent = GameUtils.TocommaStyle(role.GetItemCount(Const.Chips));
                    var gemsTextContent = Math.Round(role.GetItemCount(Const.Chips), 2).ToString();
                    var pop_chip_text = Pop_Res_Tip.gameObject.FindChild<Text>("resource bar/DiamondBtn/Title");
                    var gemsText = DiamondBtn.transform.Find("Title").GetComponent<Text>();
                    gemsText.text = gemsTextContent;
                    pop_chip_text.text = gemsTextContent;
                    if (!IsInitEnd)
                    {
                        MediatorItem.Instance.SetItemText(Const.Chips, gemsText);
                        MediatorItem.Instance.SetItemText(Const.Chips, pop_chip_text);
                    }

                    break;
            }
        }

        /// <summary>
        /// 准备播放 fake lucky 。。 you win 
        /// </summary>
        private bool infakeluckguy;

        async UniTask FakeLuckyGuy(string matchId)
        {
            if (Root.Instance.LuckyGuyInfo == null || Root.Instance.LuckyGuyInfo.LuckyRoom == null)
            {
                return;
            }

            if (infakeluckguy)
            {
                return;
            }

            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.Lucky_Guy_Played_Effect))
                infakeluckguy = true;

            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => IsUIMainTop(), cancellationToken: token);

            void show_ui()
            {
                UserInterfaceSystem.That.TopInQueue<UILuckyGuy>(new GameData()
                {
                    ["enterType"] = ActivityEnterType.Trigger,
                    ["bravo"] = true,
                    ["matchId"] = matchId
                });
                infakeluckguy = false;
            }

            Action closeCallBack = () =>
            {
                if (IsCollecting)
                {
                    this.AttachTimer(1.3f, show_ui);
                }
                else
                {
                    show_ui();
                }
            };

            //是否已经触发过 you win 特效
            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.Lucky_Guy_Played_Effect))
            {
                var effect = ResourceSystem.That.InstantiateGameObjSync("effect/eff_ui_youwon", transform);
                //显示you win 特效
                infakeluckguy = true;
                MediatorRequest.Instance.SendTriggerGuideStep(TriggerGuideStep.Lucky_Guy_Played_Effect);

                await UniTask.Delay(1300, cancellationToken: token);

                effect.Destroy();
                UserInterfaceSystem.That.ShowQueue<UIGetRewards>(
                    new GameData()
                    {
                        ["diff"] = Root.Instance.LuckyGuyInfo.LuckyRoom.GetRankReward(1),
                        ["showAll"] = false,
                        ["is_fake"] = true,
                        ["closeCallBack"] = closeCallBack
                    });
            }
            else
            {
                show_ui();
            }
        }


        async UniTask ShowLuckyGuy()
        {
            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromMinutes(10), cancellationToken: token),
                UniTask.WaitUntil(() => Root.Instance.LuckyGuyInfo is { IsOpen: true }, cancellationToken: token));


            UserInterfaceSystem.That.SingleTonQueue<UILuckyGuy>(
                () =>
                {
                    var topUI = UserInterfaceSystem.That.GetTopNormalUI();
                    if (topUI == null)
                    {
                        return false;
                    }

                    var uimain = topUI as UIMain;

                    return uimain != null && !uimain.IsUIMainInAnimation && uimain.roomToggle.isOn;
                },
                new GameData()
                {
                    ["enterType"] = ActivityEnterType.Trigger,
                    ["queueOrder"] = -10
                });
        }

        /// <summary>
        /// 是从 对局恢复来的
        /// </summary>
        private bool isLuckyPlayResore;

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.LUCKY_GUY_FAKE_NEWS,
                (sender, eventArgs) => { FakeLuckyGuy(sender as string); });

            AddEventListener(Proto.MATCH_INFO, (sender, eventArgs) => { isLuckyPlayResore = false; });

            AddEventListener(GlobalEvent.Rigister_Lucky_guy_fail, (sender, args) =>
            {
                if (Root.Instance.LuckyGuyInfo == null || Root.Instance.LuckyGuyInfo.IsFinish())
                {
                    return;
                }

                if (sender is "lucky_room")
                {
                    isLuckyPlayResore = true;
                }

                if (Root.Instance.LuckyGuyInfo is { play_chance: not 1 })
                {
                    return;
                }

                Root.Instance.LuckyGuyInfo.FirstFailTime = TimeUtils.Instance.UtcTimeNow;

                this.AttachTimer(GlobalEnum.Lucky_guy_show_interval, () =>
                {
                    if (Root.Instance.LuckyGuyInfo != null && !Root.Instance.LuckyGuyInfo.IsFinish())
                    {
                        ShowLuckyGuy();
                    }
                });
            });

            AddEventListener(GlobalEvent.SYNC_MUSEUM_INFO, (sender, args) => { MuseumMono.Init(); });

            // AddEventListener(GlobalEvent.SHOW_UI_QUEUE_FINISH, (sender, args) => { SendRefreshLuckyGuy(); });

            AddEventListener(GlobalEvent.SYNC_LUCKY_GUY_INFO, (sender, args) => { RefreshLuckyGuyEntry(); });

            AddEventListener(GlobalEvent.Sync_Item, (sender, args) => { vm[vname.Role.ToString()].Refresh(); });

            AddEventListener(GlobalEvent.Sync_Single_Item, (sender, args) =>
            {
                var item_id = (int)sender;
                RefreshItemContent(item_id);
            });

            AddEventListener(GlobalEvent.Sync_History, (sender, args) => { RefreshMatchHistory(); });

            AddEventListener(GlobalEvent.Sync_WheelFreeTicket,
                (sender, eventArgs) =>
                {
                    vm[vname.FreeTicked.ToString()].ToIObservable<int>().Value = Root.Instance.WheelFreeTicket;
                });

            //TODO 不好的异步写法
            AddEventListener(GlobalEvent.AutoPop_OnlineReward_Close,
                (sender, eventArgs) => { MediatorBingo.Instance.RestoreGame(); });

            AddEventListener(NewPlayerGuideStep.BEFORE_ENTER_ROOM.ToString(),
                (sender, eventArgs) => { beforeEnterGuideFinish = true; });

            AddEventListener(GlobalEvent.Sync_Role_Info,
                (sender, eventArgs) =>
                {
                    RefreshSettingPanel();
                    vm[vname.Role.ToString()].Refresh();
                });

            AddEventListener(GlobalEvent.Pass_Day, (sender, eventArgs) =>
            {
                RefreshSettingPanel();
                Refresh();

                // 跨天后重新请求一次签到
                if (!Root.Instance.IsNaturalFlow)
                {
                    MediatorRequest.Instance.GetSignInfo();
                    // MediatorRequest.Instance.GetMonthCardInfo();
                    MediatorRequest.Instance.GetWeekCardInfo();
                    MediatorRequest.Instance.GetSpecialOfferInfo();
                }
            });


            AddEventListener(new[]
            {
                GlobalEvent.Refresh_Room_List,
                GlobalEvent.SYNC_ROOM_AD_INFO,
                GlobalEvent.MUSEUM_INFO_REACH_MAX,
                GlobalEvent.MAGIC_BALL_REACH_MAX,
                Proto.MUSEUM_REFRESH,
                Proto.MAGIC_BALL_REFRESH
            }, (sender, eventArgs) => { RefreshRoomList(); });


            AddEventListener(
                new[] { Proto.MAGIC_BALL_CLAIM, Proto.MAGIC_BALL_INFO },
                (sender, eventArgs) =>
                {
                    MagicBallCountText.text = Root.Instance.MagicBallInfo.CurrentPoint.ToString();
                });

            AddEventListener(GlobalEvent.SHOP_REFRESH, (sender, eventArgs) => { shopMono.Refresh(); });

            AddEventListener(GlobalEvent.DAILY_REWARD_CHANCE, (sender, eventArgs) => { shopMono.RefreshFreeGift(); });

            AddEventListener(GlobalEvent.GetItems, (sender, eventArgs) => { CollectRes(sender); });

            //博物馆点数增加
            AddEventListener(GlobalEvent.MUSEUM_INFO_ADD, (sender, eventArgs) =>
            {
                if (Root.Instance.IsNaturalFlow)
                {
                    return;
                }

                museumPointCollecting = true;
            });

            AddEventListener(TriggerGuideStep.MUSEUM_GUIDE_3.ToString(),
                (sender, eventArgs) => { GuideAfterGetMuseumReward(); });

            AddEventListener(GlobalEvent.Sync_TaskInfo, (sender, eventArgs) => { RefreshActivity(); });

            AddEventListener(GlobalEvent.Sync_LuckyCard, (sender, eventArgs) => { RefreshActivity(); });

            AddEventListener(GlobalEvent.Sync_Dragon, (sender, eventArgs) => { RefreshActivity(); });

            //充值成功后, 请求下商店信息, 服务器下发数据可能不及时
            AddEventListener(GlobalEvent.CHARGE_SUCCESS,
                (sender, eventArgs) =>
                {
                    Root.Instance.IsChargeToday = true;
                    MediatorRequest.Instance.GetShopInfo();
                    roomChargeToken?.Cancel();
                });

            AddEventListener(GlobalEvent.SYNC_FORTUNE_WHEEL_INFO,
                (sender, eventArgs) => { SetWheelBtnSpin(); });

            AddEventListener(new[] { GlobalEvent.CANCEL_GAME_END, GlobalEvent.ONE_GAME_END },
                (sender, eventArgs) => { GameEndPop(); });

            //等待出现弹窗, 显示特效到存钱罐
            AddEventListener(GlobalEvent.ADD_PIGGY_BONUS,
                (sender, eventArgs) => { AddPiggyCollect(); });

            AddEventListener(GlobalEvent.MAGIC_BALL_POINT_ADD,
                (sender, eventArgs) => { AddMagicBallCollect(); });

            AddEventListener(GlobalEvent.GO_TO_MAIN_STORE,
                (sender, eventArgs) => { ToStorePanel(); });

            AddEventListener(GlobalEvent.GO_TO_MAIN_ROOM_PAGE,
                (sender, eventArgs) => { vm[vname.Page.ToString()].ToIObservable<int>().Value = 1; });

            AddEventListener(GlobalEvent.Player_Click_Down,
                (sender, eventArgs) =>
                {
                    // if (!InputMonitor.HitTest(ActivityScrollRect.GetComponent<RectTransform>()))
                    // {
                    //     var Full_Tip = PiggyBankBtn.gameObject.FindChild("Full/Full Tip");
                    //     if (Full_Tip != null && Full_Tip.transform.IsActive())
                    //     {
                    //         HidePiggyFullTip();
                    //         EffectMask.SetActive(false);
                    //     }
                    // }
                });

            AddEventListener(GlobalEvent.SYNC_FREE_BONUS_INFO,
                (sender, eventArgs) => { MatchFreeBonusGame(); });


            AddEventListener(GlobalEvent.ROOM_CHARGE,
                (sender, eventArgs) => { OpenRoomChargeA(); });

            AddEventListener(Proto.GAME_END,
                (sender, eventArgs) =>
                {
                    if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
                    {
                        ShowExItems();
                    }
                });

            AddEventListener(GlobalEvent.CLAIM_DAILYTASK, (sender, eventArgs) => { UniClaimDailyTask(); });

            AddEventListener(Proto.MAGIC_BALL_REFRESH, (sender, eventArgs) =>
            {
                InitRefreshMagicBallTimer();
                if (Root.Instance.MagicBallInfo != null)
                {
                    MagicBallCountText.text = Root.Instance.MagicBallInfo.CurrentPoint.ToString();
                }
            });

            AddEventListener(Proto.MUSEUM_REFRESH, (sender, eventArgs) => { InitRefreshMuseumTimer(); });
        }

        private TweenerCore<Vector2, Vector2, VectorOptions> backTween;

        private TweenerCore<float, float, FloatOptions> bonusTweener;
        private TweenerCore<float, float, FloatOptions> diamondTweener;
        private TweenerCore<float, float, FloatOptions> coinTweener;

        private async UniTask CollectRes(object sender)
        {
            await UniTask.Delay(150);

            var uiOnlineReward = UserInterfaceSystem.That.Get<UIOnlineReward>();

            var isNormal = IsVisible && !IsPanelAt(3) && uiOnlineReward == null;

            var tuple = (ValueTuple<Vector3, Item>)sender;
            var position = tuple.Item1;
            var item = tuple.Item2;

            Image to = null;
            float itemCount = 0;
            Text textCom = null;
            Text popTextCom = null;
            TweenerCore<float, float, FloatOptions> useTweener = null;
            switch (item.id)
            {
                case Const.Cash:
                case Const.Bonus:
                    useTweener = bonusTweener;
                    var pop_bonus_btn = Pop_Res_Tip.gameObject.FindChild("resource bar/BonusBtn");
                    var bonusBtn = isNormal
                        ? BonusBtn.gameObject
                        : pop_bonus_btn;
                    to = bonusBtn.FindChild<Image>("Icon");
                    textCom = BonusBtn.gameObject.FindChild<Text>("Title");
                    popTextCom = pop_bonus_btn.FindChild<Text>("Title");
                    itemCount = Root.Instance.Role.GetDollars();
                    break;
                case Const.Chips:
                    useTweener = diamondTweener;
                    var pop_diamond_btn = Pop_Res_Tip.gameObject.FindChild("resource bar/DiamondBtn");
                    var diamondBtn = isNormal
                        ? DiamondBtn.gameObject
                        : pop_diamond_btn;
                    to = diamondBtn.FindChild<Image>("Icon");
                    textCom = DiamondBtn.gameObject.FindChild<Text>("Title");
                    popTextCom = pop_diamond_btn.FindChild<Text>("Title");
                    itemCount = Root.Instance.Role.GetItemCount(Const.Chips);
                    break;
                case Const.Coin:
                    useTweener = coinTweener;
                    var pop_coin_btn = Pop_Res_Tip.gameObject.FindChild("resource bar/TokenBtn");
                    var coinBtn = isNormal
                        ? CoinBtn.gameObject
                        : pop_coin_btn;
                    to = coinBtn.FindChild<Image>("Icon");
                    textCom = CoinBtn.gameObject.FindChild<Text>("Title");
                    popTextCom = pop_coin_btn.FindChild<Text>("Title");
                    itemCount = Root.Instance.Role.GetItemCount(Const.Coin);
                    break;
            }

            if (to != null)
            {
                var parent = isNormal ? Normal_Res_Group.gameObject : Pop_Res_Tip.gameObject;
                var heightCanvas = parent.TryAddComponent<AddHeightCanvas>();
                heightCanvas.SetAddSortingOrder(100);

                //显示UIMain
                IsVisible = true;

                heightCanvas.enabled = true;

                UIEffectUtils.Instance.CreatResCollect(position, to.transform, to.sprite.texture);

                //这次领取道具之前的 道具数量

                //最终的小数位
                var dicimal = Math.Min(2, itemCount.ToString().NumOfDecimal());

                var beforeGet = itemCount - item.Count;

                var delayTime = 1.5f;

                // var diff = item.Count;
                if (!isNormal)
                {
                    Pop_Res_Tip.SetActive(true);
                    var rectTransform = Pop_Res_Tip.transform.rectTransform();
                    DOTween.To(() => rectTransform.anchoredPosition, v => rectTransform.anchoredPosition = v,
                        new Vector2(3, -40), 0.3f);

                    backTween?.Kill();

                    backTween = DOTween.To(() => rectTransform.anchoredPosition,
                            v => rectTransform.anchoredPosition = v,
                            new Vector2(3, 110), 0.3f)
                        .SetDelay(1 + delayTime)
                        .OnComplete(() => { Pop_Res_Tip.SetActive(false); });
                }

                useTweener?.Kill();

                useTweener = DOTween.To(() => beforeGet, x => beforeGet = x, itemCount, 1f)
                    .OnStart(() =>
                    {
                        vibrateDisposable?.Dispose();

                        YZGameUtil.Vibrate();
                        AudioSystem.That.PlaySound(SoundPack.Res_Change_Sound);
                        vibrateDisposable = Observable.Interval(TimeSpan.FromSeconds(0.15f)).First(count =>
                        {
                            if (count == 2)
                            {
                                AudioSystem.That.PlaySound(SoundPack.Res_Change_Sound);
                            }

                            //最多到5就结束了
                            // YZLog.LogColor(count);
                            
                            YZGameUtil.Vibrate();
                            //最多震动6次
                            return count >= 6;
                        }).Subscribe();
                    })
                    .OnUpdate(() =>
                    {
                        // textCom.text = GameUtils.TocommaStyle(Math.Round(beforeGet, dicimal));
                        // textCom1.text = GameUtils.TocommaStyle(Math.Round(beforeGet, dicimal));
                        var updateCount = Math.Round(beforeGet, dicimal).ToString();
                        textCom.text = updateCount;
                        popTextCom.text = updateCount;
                    })
                    .SetDelay(1f);

                //dotween 不一定会进入OnComplete
                this.AttachTimer(2, () =>
                {
                    vibrateDisposable?.Dispose();
                    if (isNormal)
                    {
                        heightCanvas.enabled = false;
                    }

                    var finnalCount = Math.Round(itemCount, dicimal).ToString();
                    textCom.text =  finnalCount;
                    popTextCom.text = finnalCount;
                });

                switch (item.id)
                {
                    case Const.Cash:
                    case Const.Bonus:
                        bonusTweener = useTweener;
                        break;
                    case Const.Chips:
                        diamondTweener = useTweener;
                        break;
                    case Const.Coin:
                        coinTweener = useTweener;
                        break;
                }
            }
        }


        public void PlayCollectMuseumPoint()
        {
            playCollectMuseumPoint();

            GuideClickMuseumToggle();
        }

        async UniTask playCollectMuseumPoint()
        {
            var sprite = MediatorBingo.Instance.GetSpriteByUrl("museum/point");

            UIEffectUtils.Instance.CreatResCollect(Vector3.zero, CollectionToggle.transform, sprite.texture);

            EffectMask.SetActive(true);

            var heightCanvas = CollectionToggle.gameObject.TryAddComponent<AddHeightCanvas>();


            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            museumPointCollecting = false;

            heightCanvas.enabled = false;

            EffectMask.SetActive(false);
        }

        public void PlayCollectMagicBallPoint()
        {
            playCollectMagicBallPoint();
        }

        async UniTask playCollectMagicBallPoint()
        {
            var sprite = MediatorBingo.Instance.GetSpriteByUrl("common/magic_point_icon");

            EffectMask.SetActive(true);

            var heightCanvas = MagicBallBtn.gameObject.TryAddComponent<AddHeightCanvas>();


            //target 活动头像
            UIEffectUtils.Instance.CreatResCollect(Vector3.zero, MagicBallBtn.transform, sprite.texture,
                () => { MagicBallCountText.text = Root.Instance.MagicBallInfo.CurrentPoint.ToString(); });

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            magicBallCollecting = false;

            heightCanvas.enabled = false;

            EffectMask.SetActive(false);
        }

        async UniTask ShowExItems()
        {
            if (isLuckyPlayResore)
            {
                await UniTask.WhenAny(UniTask.WaitUntil(() => !isLuckyPlayResore),
                    UniTask.Delay(1000)
                );
                isLuckyPlayResore = false;
            }

            //显示UIMain
            await UniTask.WaitUntil(() => IsUIMainTop() && IsPanelAt(1));

            if (infakeluckguy)
            {
                return;
            }

            if (!MediatorItem.Instance.HaveExShowItem())
            {
                return;
            }

            UserInterfaceSystem.That.TopInQueue<UIGetRewards>(new GameData()
            {
                ["diff"] = new List<Item>(),
            });
        }

        private void RefreshRoomList()
        {
            var roomList = GetOrganizedList();

            vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value = roomList;
        }

        private void BubbleSort(ref ArrayList roomList, int start)
        {
            if (start < 0)
            {
                return;
            }

            int i, j;
            for (i = 0; i < roomList.Count - 1; i++)
            {
                for (j = start; j < roomList.Count - 1 - i; j++)
                {
                    for (int k = 1; k + j < roomList.Count; ++k)
                        if ((roomList[j] is Room roomj && roomj.IsDollarRoom &&
                             roomList[j + k] is Room roomj1 && roomj1.IsDollarRoom)
                            && roomj.seat == 5 && roomj1.seat == 2)
                        {
                            (roomList[j], roomList[j + k]) = (roomList[j + k], roomList[j]);
                        }
                }
            }

            for (i = 0; i < roomList.Count - 1; i++)
            {
                for (j = start; j < roomList.Count - 1 - i; j++)
                {
                    for (int k = 1; k < roomList.Count - j; ++k)
                        if (roomList[j] is Room roomj && roomj.IsDollarRoom &&
                            roomList[j + k] is Room roomj1 && roomj1.IsDollarRoom
                            && roomj.seat == roomj1.seat && roomj.in_items[1] > roomj1.in_items[1])
                        {
                            //YZDebug.Log("roomj in = " + roomj.in_items[1] + " roomj1 in = " + roomj1.in_items[1]);
                            (roomList[j], roomList[j + k]) = (roomList[j + k], roomList[j]);
                        }
                }
            }
        }

        private ArrayList GetOrganizedList()
        {
            // 自然量
            if (Root.Instance.IsNaturalFlow)
            {
                ArrayList roomListOrganic = new ArrayList();
                foreach (var room in Root.Instance.ShowRoomList)
                {
                    roomListOrganic.Add(room);
                }

                return roomListOrganic;
            }

            // 非自然量
            ArrayList roomList = new ArrayList();
            foreach (var room in Root.Instance.ShowRoomList)
            {
                roomList.Add(room);
            }

            int diamondIndex = -1;
            for (int i = 0; i < Root.Instance.ShowRoomList.Count; ++i)
            {
                // 找到1000钻石标志房间
                if (Root.Instance.ShowRoomList[i].IsDiamondRoom &&
                    Root.Instance.ShowRoomList[i].in_items[2] == 1000)
                {
                    diamondIndex = i;
                    break;
                }
            }

            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.FIRST_ROOM_GAME))
            {
                for (int i = 0; i < roomList.Count; ++i)
                {
                    if (roomList[i] is Room room && room.id == targetRoomIndex)
                    {
                        roomList.Remove(room);
                        roomList.Insert(0, room);
                    }
                }
            }

            //bug
            if (diamondIndex < 0 || true)
            {
                return roomList;
            }

            BubbleSort(ref roomList, diamondIndex);

            int seatTwoStart = -1;
            for (int i = diamondIndex + 1; i < Root.Instance.ShowRoomList.Count; ++i)
            {
                if (roomList[i] is Room room && room.IsDollarRoom && room.seat == 2)
                {
                    seatTwoStart = i;
                    break;
                }
            }

            //bug 找不到 seatTwoStart
            // 1v1美金场 折叠
            roomList.Insert(seatTwoStart, new title { textContent = I18N.Get("TWO_SEAT"), order = 0 });


            var seat2On = vm[vname.Seat2On.ToString()].ToIObservable<bool>().Value;
            var seat5On = vm[vname.Seat5On.ToString()].ToIObservable<bool>().Value;

            var multiRoom2 = (Room)roomList[seatTwoStart + 1];
            multiRoom2.IsMulti = !seat2On;

            for (int i = roomList.Count - 1; i > seatTwoStart + 1; --i)
            {
                if (roomList[i] is Room room && room.seat == 2 && room.IsDollarRoom && !seat2On)
                {
                    roomList.Remove(room);
                }
            }

            int fiveSeatStart = -1;

            for (int i = diamondIndex + 1; i < roomList.Count; ++i)
            {
                if (roomList[i] is Room room && room.IsDollarRoom && room.seat == 5)
                {
                    fiveSeatStart = i;
                    break;
                }
            }

            // 5人美金场 折叠
            roomList.Insert(fiveSeatStart, new title { textContent = I18N.Get("FIVE_SEAT"), order = 1 });

            var multiRoom5 = (Room)roomList[fiveSeatStart + 1];
            multiRoom5.IsMulti = !seat5On;

            for (int i = roomList.Count - 1; i > fiveSeatStart + 1; --i)
            {
                if (roomList[i] is Room room && room.seat == 5 && room.IsDollarRoom && !seat5On)
                {
                    roomList.Remove(room);
                }
            }

            // 现金场中间筛选出非现金场
            int cashRoomLastIndex = -1;
            for (int i = roomList.Count - 1; i > seatTwoStart; --i)
            {
                if (roomList[i] is Room room && room.IsDollarRoom)
                {
                    // 最后一个现金场
                    cashRoomLastIndex = i;
                    break;
                }
            }

            if (cashRoomLastIndex != -1)
            {
                List<Room> nonCashRooms = new List<Room>();
                for (int i = cashRoomLastIndex; i > seatTwoStart; --i)
                {
                    if (roomList[i] is Room room && !room.IsDollarRoom)
                    {
                        nonCashRooms.Add(room);
                        roomList.Remove(room);
                    }
                }

                for (int i = roomList.Count - 1; i > seatTwoStart; --i)
                {
                    if (roomList[i] is Room room && room.IsDollarRoom)
                    {
                        // 最后一个现金场
                        cashRoomLastIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < nonCashRooms.Count; ++i)
                {
                    roomList.Insert(cashRoomLastIndex + 1, nonCashRooms[i]);
                }
            }


            return roomList;
        }

        async UniTask MatchFreeBonusGame()
        {
            RefreshRoomList();
            //直接匹配
            if (Root.Instance.FreeBonusInfo.CanPlay)
            {
                await UniTask.WaitUntil(() => { return IsUIMainTop(); });

                var roomList = vm[vname.RoomListData.ToString()].ToIObservable<ArrayList>().Value;
                if (roomList == null)
                {
                    return;
                }

                foreach (var room in roomList)
                {
                    if (room is Room { IsFreeBonusRoom: true })
                    {
                        MediatorRequest.Instance.MatchBegin((Room)room, callback: () => { RefreshRoomList(); });
                        break;
                    }
                }
            }
        }

        private CancellationTokenSource roomChargeToken;

        async UniTask OpenRoomChargeA()
        {
            roomChargeToken = new CancellationTokenSource();
            await UniTask.WaitUntil(() =>
            {
                var topNormalUI = UserInterfaceSystem.That.GetTopNormalUI();
                return topNormalUI != null &&
                       (topNormalUI.ClassType == typeof(UIMain) ||
                        topNormalUI.ClassType == typeof(UIFriendsDuel) ||
                        topNormalUI.ClassType == typeof(UIRoomEntry) ||
                        topNormalUI.ClassType == typeof(UILuckyGuy)
                       );
            }, cancellationToken: roomChargeToken.Token);
            
            if (MediatorActivity.Instance.IsActivityOpen(ActivityType.JustForYou))
            {
                // 设置当前活动时间
                MediatorRequest.Instance.SetRoomChargeBeginTime("A",
                    () =>
                    {
                        UserInterfaceSystem.That.ShowUI<UIJustForYou>(
                            new GameData()
                            {
                                ["ActivityEnterType"] = ActivityEnterType.Trigger
                            });
                    });
            }
        }

        private bool game_end_poped;

        /// <summary>
        /// 游戏结束后的弹窗
        /// </summary>
        /// <returns></returns>
        async UniTask GameEndPop()
        {
            game_end_poped = true;

            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.WaitUntil(() => IsVisible, cancellationToken: token);

            ForbidSelfClick();

            await UniTask.WaitUntil(IsUIMainTop, cancellationToken: token);

            if (IsUIMainInAnimation)
            {
                await UniTask.WhenAny(UniTask.WaitUntil(() => !IsUIMainInAnimation, cancellationToken: token),
                    UniTask.Delay(2000, cancellationToken: token));
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromMilliseconds(100), cancellationToken: token);
            }

            //高亮的地方可以点 【额外的 canvas】
            ResumeSelfClick();

            DOTween.Kill(ActivityGroup.GetComponent<RectTransform>(), false);
            ActivityGroup.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(ActivityGroup.GetComponent<RectTransform>().anchoredPosition.x, -160f);

            MediatorActivity.Instance.PopAllActivity(true);
        }

        private async UniTask UniClaimDailyTask()
        {
            if (IsUIMainInAnimation)
            {
                await UniTask.WaitUntil(() => !IsUIMainInAnimation);
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromMilliseconds(100));
            }

            var topUi = UserInterfaceSystem.That.GetTopNormalUI();
            if (topUi.ClassType != typeof(UIDailyMission))
                MediatorRequest.Instance.ClaimDailyTask(true);
        }

        private bool IsUIMainTop()
        {
            var uiWaitNet = UserInterfaceSystem.That.Get<UIWaitNet>();

            if (uiWaitNet != null)
            {
                return false;
            }

            var topUI = UserInterfaceSystem.That.GetTopNormalUI();
            if (topUI == null)
            {
                return false;
            }

            if (this == null)
            {
                return false;
            }

            return topUI.UIName == UIName;
        }

        private bool piggyCollecting;

        private bool magicBallCollecting;

        /// <summary>
        /// 博物馆点数是否在播放动画
        /// </summary>
        private bool museumPointCollecting;

        public bool IsUIMainInAnimation => IsCollecting || infakeluckguy;

        bool IsCollecting => museumPointCollecting || piggyCollecting || magicBallCollecting;

        private const int roomPageIndex = 1;

        bool IsPanelAt(int index)
        {
            return vm[vname.Page.ToString()].ToIObservable<int>().Value == index;
        }

        private void AddMagicBallCollect()
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.MagicBall))
            {
                return;
            }

            if (magicBallCollecting)
            {
                return;
            }

            magicBallCollecting = true;
        }

        void AddPiggyCollect()
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank))
            {
                return;
            }

            if (piggyCollecting)
            {
                return;
            }

            piggyCollecting = true;

            var item = new Item("piggyBonus", Root.Instance.PiggyBankInfo.AddPiggyBonus);
            MediatorItem.Instance.AddExShowItem(item);
        }

        public void PlayPiggyBonusAnimation()
        {
            playPiggyBonusAnimation();
        }

        private async Task playPiggyBonusAnimation()
        {
            await UniTask.WaitUntil(() => { return IsPanelAt(1); });

            EffectMask.SetActive(true);

            //移动存钱罐
            // ActivityScrollRect.ScrollToObject(PiggyBankBtn.GetComponent<RectTransform>());

            //高亮存钱罐
            var heightCanvas = PiggyBankBtn.gameObject.TryAddComponent<AddHeightCanvas>();

            UIEffectUtils.Instance.CreatResCollect(new Vector3(),
                PiggyBankBtn.transform,
                new Item("piggyBonus").GetIcon().texture,
                () => { RefreshPiggyBankFullState(true); });

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            heightCanvas.enabled = false;

            EffectMask.SetActive(false);
        }

        private void RefreshMatchHistory()
        {
            //清空ItemModel
            // MatchHistoryList.Clear();
            vm[vname.MatchHistoryData.ToString()].ToIObservable<ArrayList>().Value = GetHistoryListData();
        }

        class title
        {
            public Type targetMono = typeof(TitleGroupMono);

            public int order;

            public string textContent;
        }

        private bool haveAnyRecord;

        /// <summary>
        /// 获取历史记录数据
        /// </summary>
        /// <returns></returns>
        private ArrayList GetHistoryListData()
        {
            var result = new ArrayList();

            var incomplete = Root.Instance.MatchHistory.Where(
                    history => history.NotFinish)
                .OrderBy(history => -history.begin_time).ToList();

            var complete = Root.Instance.MatchHistory.Where(
                history => !history.NotFinish).OrderBy(history => -history.begin_time).ToList();

            haveAnyRecord = incomplete.Any() || complete.Any();

            var inComplectOn = vm[vname.InCompleteOn.ToString()].ToIObservable<bool>().Value;

            result.Add(new title { textContent = I18N.Get("key_in_progress_1"), order = 0 });

            if (inComplectOn)
            {
                for (int i = 0; i < incomplete.Count; i++)
                {
                    if (!Root.Instance.InCompleteRecordsOver && i == incomplete.Count - 10)
                    {
                        incomplete[i].PullFlag = true;
                    }
                    else
                    {
                        incomplete[i].PullFlag = false;
                    }

                    result.Add(incomplete[i]);
                }
            }

            var complectOn = vm[vname.CompleteOn.ToString()].ToIObservable<bool>().Value;
            result.Add(new title { textContent = I18N.Get("key_complete"), order = 1 });

            if (complectOn)
            {
                for (int i = 0; i < complete.Count; i++)
                {
                    if (!Root.Instance.CompleteRecordsOver && i == complete.Count - 10)
                    {
                        complete[i].PullFlag = true;
                    }
                    else
                    {
                        complete[i].PullFlag = false;
                    }

                    result.Add(complete[i]);
                }
            }


            return result;
        }

        private GameObject finger;

        private void LoadFingerAtRoom(int itemIndex)
        {
            var childIndex = RoomList.ItemIndexToChildIndex(itemIndex);
            var parentObj = RoomList.GetChildAt(childIndex);
            if (parentObj == null)
            {
                return;
            }

            LoadFinger(parentObj.GetComponent<RoomItemMono>().Button.transform);
        }

        void LoadFinger(Transform parent)
        {
            finger ??= ResourceSystem.That.InstantiateGameObjSync("common/finger01");
            finger.SetActive(true);
            finger.transform.SetParent(parent);
            finger.transform.localScale = new Vector3(1, 1, 1);
            finger.transform.localPosition = new Vector3(28, -18, 0);
        }

        void LoadFinger(Transform parent, Vector3 localPosition)
        {
            finger ??= ResourceSystem.That.InstantiateGameObjSync("common/finger01");
            finger.SetActive(true);
            finger.transform.SetParent(parent);
            finger.transform.localScale = new Vector3(1, 1, 1);
            finger.transform.localPosition = localPosition;
        }

        void RefreshBestOffer()
        {
            try
            {
                var active = MediatorActivity.Instance.IsActivityBegin(ActivityType.BestOffer);
                BestOfferBtn.SetActive(active);

                if (active)
                {
                    var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.BestOffer);
                    BestOfferBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(lessTime);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                BestOfferBtn.SetActive(false);
            }
        }

        void RefreshJustForYou()
        {
            try
            {
                var active = MediatorActivity.Instance.IsActivityBegin(ActivityType.JustForYou);
                JustForYouBtn.SetActive(active);

                if (active)
                {
                    var lessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.JustForYou);
                    JustForYouBtn.gameObject.FindChild<Text>("TextGroup/Text").text =
                        TimeUtils.Instance.ToHourMinuteSecond(lessTime);
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(e.ToString(), "red");
                JustForYouBtn.SetActive(false);
            }
        }

        private List<int> ActivityIndex = new List<int>();
        private List<int> FreeGiftsIndex = new List<int>();

        private Timer refreshMagicBallTimer;
        private Timer refreshMuseumTimer;
        private Timer luckyGuyTimer;

        /// <summary>
        /// 处理动画
        /// </summary>
        void InitSpineAnimation(ref List<int> activityIndex, Transform activityContent)
        {
            activityIndex.Clear();
            for (int i = 0; i < activityContent.childCount; i++)
            {
                var activityBtn = activityContent.GetChild(i);

                var skeletonGraphic = GetSkeletonGraphic(activityBtn);

                if (skeletonGraphic != null)
                {
                    if (skeletonGraphic == WheelSpine)
                    {
                        if (WheelSpine.AnimationState.GetCurrent(0).Animation.Name == "spin2")
                        {
                            continue;
                        }
                    }

                    if (activityBtn.IsActive())
                    {
                        activityIndex.Add(i);
                    }

                    skeletonGraphic.timeScale = 0;
                    skeletonGraphic.startingLoop = true;
                }

                activityBtn.GetChild(0).TryGetComponent<SpineBreath>(out var spineBreath);
                if (spineBreath != null)
                {
                    spineBreath.enabled = false;
                }
            }
        }

        private SkeletonGraphic GetSkeletonGraphic(Transform activityBtn)
        {
            SkeletonGraphic skeletonGraphic = null;

            if (activityBtn.name == "SpecialGift")
            {
                var spineParentTransform = activityBtn.GetChild(0);
                for (int j = 0; j < spineParentTransform.childCount; j++)
                {
                    if (spineParentTransform.GetChild(j).IsActive())
                    {
                        spineParentTransform.GetChild(j).TryGetComponent<SkeletonGraphic>(out var component);
                        if (component != null)
                        {
                            skeletonGraphic = component;
                            break;
                        }
                    }
                }
            }
            else
            {
                activityBtn.GetChild(0).TryGetComponent<SkeletonGraphic>(out var component);
                skeletonGraphic = component;
            }

            return skeletonGraphic;
        }

        async UniTask RandomPlaySpineAnimation(List<int> activityIndex, Transform activityContent)
        {
            var token = this.GetCancellationTokenOnDestroy();
            InitSpineAnimation(ref activityIndex, activityContent);
            activityIndex.Shuffle();

            bool runAnimation = false;
            foreach (var index in activityIndex)
            {
                var activityBtn = activityContent.GetChild(index);
                if (!activityBtn.IsActive())
                {
                    continue;
                }

                var skeletonGraphic = GetSkeletonGraphic(activityBtn);

                if (skeletonGraphic == null)
                {
                    continue;
                }

                if (skeletonGraphic == WheelSpine)
                {
                    if (WheelSpine.AnimationState.GetCurrent(0).Animation.Name == "spin2")
                    {
                        continue;
                    }
                }

                skeletonGraphic.timeScale = 1;
                runAnimation = true;
                var trackEntry = skeletonGraphic.AnimationState.GetCurrent(0);
                trackEntry.TrackTime = 0;
                await UniTask.Delay(TimeSpan.FromSeconds(trackEntry.AnimationEnd * 2), cancellationToken: token);

                if (skeletonGraphic == WheelSpine)
                {
                    if (WheelSpine.AnimationState.GetCurrent(0).Animation.Name == "spin2")
                    {
                        continue;
                    }
                }

                trackEntry.TrackTime = 0;

                await UniTask.NextFrame(token);

                skeletonGraphic.timeScale = 0;

                await UniTask.Delay(3000, cancellationToken: token);
            }

            if (!runAnimation)
            {
                await UniTask.Delay(500, cancellationToken: token);
            }


            RandomPlaySpineAnimation(activityIndex, activityContent);
        }

        private Timer resumeClickTimer;

        private new void ForbidSelfClick()
        {
            base.ForbidSelfClick();
            resumeClickTimer?.Cancel();
            resumeClickTimer = this.AttachTimer(3f, () => { ResumeSelfClick(); });
        }
    }
}