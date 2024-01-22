using System.Collections.Generic;
using BrunoMikoski.AnimationsSequencer;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using DG.Tweening;
using Reactive.Bindings;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Root = DataAccess.Model.Root;

namespace UI
{
    public class UISign : UIBase<UISign>
    {
        #region UI Variable Statement

        [SerializeField] MyButton MaskBtn;
        [SerializeField] private List<Transform> AwardTrans;
        [SerializeField] private List<Text> HeapTexts;
        [SerializeField] private Text DescText;
        [SerializeField] private Slider HeapSlider;
        [SerializeField] private Image PlayerIcon;
        [SerializeField] private AnimationSequence showSequence;

        #endregion

        private const int AwardCount = 7;
        private const int HeapCount = 30;
        private bool IsFirstTimeOpen = true;

        private int currentDaySigned = -1;
        private int tempToday = -1;

        private int maxActivedHeapRewardInex = -1;

        public override UIType uiType { get; set; } = UIType.Window;

        private static UISign inst;
        public static UISign Instance => inst;

        private static Vector3 PosAward;

        /// <summary>
        /// 是否只是展示
        /// </summary>
        private bool isjustShow;

        private void Awake()
        {
            base.Awake();
            showSequence.AutoKill = false;
        }
        
        protected override void OnClose()
        {
            base.OnClose();

            YZFunnelUtil.YZFunnelActivityPop(ActivityType.DailySign
                // , charge_id: chargeId
                , isauto: !isjustShow
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        public override void OnStart()
        {
            var heapList = Root.Instance.SignHeapAwardsList;
            if (heapList == null)
            {
                Close();
            }

            Root.Instance.Role.LoadIcon(PlayerIcon);
            for (int i = 0; i < heapList.Count; ++i)
            {
                HeapTexts[i].text = heapList[i].order.ToString();
                var itemTrans = HeapTexts[i].transform.parent.Find("Item");
                itemTrans.GetComponent<Image>().sprite = MediatorItem.Instance.GetItemSprite(heapList[i].type);
                var itemCount = HeapTexts[i].transform.parent.Find("CountText");
                itemCount.GetComponent<Text>().text = heapList[i].amount.ToString();
            }

            inst = this;
        }


        enum vname
        {
            signInfo,
            signChance,
            signCount,
            signHeapReward,
        }

        public override void InitVm()
        {
            //刷新 isjustShow 状态
            var table = GetArgsByIndex<GameData>(0);
            if (table != null)
            {
                isjustShow = (bool)table["isjustShow"];
            }

            DescText.text = isjustShow ? I18N.Get("key_click_blank_close") : I18N.Get("key_collect_reward");
            if (isjustShow)
            {
                MaskBtn.SetClick(OnCloseBtnClick);
            }

            vm[vname.signInfo.ToString()] = new ReactivePropertySlim<SignInfo>
                (Root.Instance.SignInfo);
            vm[vname.signChance.ToString()] = new ReactivePropertySlim<int>
                (Root.Instance.SignInfo.sign_chance);
            vm[vname.signCount.ToString()] = new ReactivePropertySlim<int>
                (Root.Instance.SignInfo.sign_count);
            vm[vname.signHeapReward.ToString()] = new ReactivePropertySlim<int>
                (Root.Instance.SignInfo.sign_heap_reward);
        }

        public override void InitBinds()
        {
            vm[vname.signChance.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                // todayIndex 改变，隐藏原先的，点亮现在的奖励
                for (int i = 0; i < AwardCount; i++)
                {
                    SetAward(AwardTrans[i], i);
                }
            });
            vm[vname.signCount.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                int lastOrderIndex = -1;
                int deltaOrder = 0;
                var heapList = Root.Instance.SignHeapAwardsList;
                int orderCount = heapList[0].order;

                var heapCount = Root.Instance.SignInfo.sign_heap_reward;
                // 如果没有超过任何奖励 给一个初始值
                deltaOrder = heapCount;

                for (int i = 0; i < heapList.Count; ++i)
                {
                    bool glodVisible = heapList[i].order <= heapCount;
                    HeapTexts[i].transform.parent.Find("GoldBg").SetActive(glodVisible);

                    if (glodVisible)
                        maxActivedHeapRewardInex = i;

                    if (heapCount >= heapList[i].order)
                    {
                        // 当前累积的刚好超过了某个奖励
                        lastOrderIndex = i;
                        deltaOrder = heapCount - heapList[i].order;
                        if (i < heapList.Count - 1)
                            orderCount = heapList[i + 1].order - heapList[i].order;
                        else
                            orderCount = heapList[i].order - heapList[i - 1].order;
                    }
                }

                // 进度条发生变化
                if (!IsFirstTimeOpen)
                {
                    float fromValue = HeapSlider.value;
                    float toValue = (lastOrderIndex + 1) * 25.0f +
                                    ((float)deltaOrder) / ((float)orderCount) * 25;
                    if (fromValue < toValue)
                        DOTween.To(() => fromValue, x => HeapSlider.value = x, toValue, 0.5f);
                }
                else
                {
                    HeapSlider.value = (lastOrderIndex + 1) * 25.0f +
                                       ((float)deltaOrder) / ((float)orderCount) * 25;
                }
            });
            vm[vname.signInfo.ToString()].ToIObservable<SignInfo>().Subscribe(value =>
            {
                if (value == null)
                {
                    return;
                }

                var signChance = value.sign_chance;
                var signCount = value.sign_count;
                var signHeapReward = value.sign_heap_reward;
                // 数据改变，把改变传给  todayIndex 和 signCount
                vm[vname.signChance.ToString()].ToIObservable<int>().Value = signChance;
                vm[vname.signCount.ToString()].ToIObservable<int>().Value = signCount;
                vm[vname.signHeapReward.ToString()].ToIObservable<int>().Value = signHeapReward;

                for (int i = 0; i < AwardCount; i++)
                {
                    SetAward(AwardTrans[i], i);
                }
            });
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_SignInfo,
                (sender, args) =>
                {
                    vm[vname.signInfo.ToString()].ToIObservable<SignInfo>().Value = Root.Instance.SignInfo;
                });
        }

        private void SetAward(Transform awardTrans, int day)
        {
            // state
            var signCount = vm[vname.signCount.ToString()].ToIObservable<int>().Value;
            var signChance = vm[vname.signChance.ToString()].ToIObservable<int>().Value;

            var showIndex = 0;
            if (signCount < AwardCount)
                showIndex = day;
            else
                showIndex = day + AwardCount;

            // 修复7日签到，最后一天领取3刀后奖励被刷新成了循环礼物
            if (!IsFirstTimeOpen && signCount == AwardCount && !isjustShow)
            {
                return;
            }

            var awardMono = awardTrans.GetComponent<AwardItemMono>();
            var data = Root.Instance.SignAwardsList[showIndex];
            awardMono.Icon.sprite = MediatorItem.Instance.GetItemSprite(data.type);
            awardMono.ItemCountText.text = data.amount.ToString();

            var rewardIndex = signCount % AwardCount;

            if (currentDaySigned == -1)
            {
                if (day == rewardIndex)
                {
                    if (signChance == 1 || isjustShow)
                    {
                        if (isjustShow)
                        {
                            awardMono.ClamState = AwardItemMono.ClaimState.CanNotClaim;
                        }
                        else
                            awardMono.ClamState = AwardItemMono.ClaimState.CanClaim;

                        awardMono.ItemBtn.SetClick(TrySign);
                        tempToday = day;
                    }
                    else
                        awardMono.ClamState = AwardItemMono.ClaimState.Claimed;
                }
                else
                {
                    if (day > rewardIndex)
                    {
                        awardMono.ClamState = AwardItemMono.ClaimState.CanNotClaim;
                    }
                    else if (day < rewardIndex)
                    {
                        awardMono.ClamState = AwardItemMono.ClaimState.Claimed;
                    }
                }
            }
            else
            {
                if (day == currentDaySigned)
                    awardMono.ClamState = AwardItemMono.ClaimState.Claimed;
            }
        }

        private void TrySign()
        {
            if (isjustShow)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_charge_room_entry_tip"));
                return;
            }

            MediatorRequest.Instance.Sign();
            TinyTimer.StartTimer(() => { Close(); }, 2.0f);
            currentDaySigned = tempToday;
        }

        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    IsFirstTimeOpen = false;
                    if (AwardTrans != null && AwardTrans.Count > 0 && AwardTrans[tempToday] != null)
                        PosAward = AwardTrans[tempToday].position;
                });
        }

        protected override void OnAnimationOut()
        {
            var seq = DOTween.Sequence().AppendInterval(3f).SetId(UIName);
            transform.HideUIByEffect(showSequence, 3f, () => { seq.Kill(true); });
        }

        public void FlyTodayReward()
        {
            var showIndex = 0;
            var signCount = vm[vname.signCount.ToString()].ToIObservable<int>().Value;
            if (signCount < AwardCount)
                showIndex = tempToday;
            else
                showIndex = tempToday + AwardCount;
            var itemOnline = Root.Instance.SignAwardsList[showIndex];
            Item item = new Item(itemOnline.type, itemOnline.amount);
            EventDispatcher.Root.Raise(GlobalEvent.GetItems, (PosAward, item));
        }

        public void FlyHeapReward()
        {
            if (maxActivedHeapRewardInex == -1)
                return;

            var itemOnile = Root.Instance.SignHeapAwardsList[maxActivedHeapRewardInex];
            Item item = new Item(itemOnile.type, itemOnile.amount);
            var obj = HeapTexts[maxActivedHeapRewardInex].transform.parent.Find("GoldBg");
            EventDispatcher.Root.Raise(GlobalEvent.GetItems, (obj.position, item));
        }
    }
}