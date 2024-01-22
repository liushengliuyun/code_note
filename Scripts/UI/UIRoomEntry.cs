using System;
using Core.Controllers;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIRoomEntry : UIBase<UIRoomEntry>
    {
        public GameObject RewardIs2EnterFee;
        public Transform FiveRewardPanel;
        public Transform TwoRewardPanel;
        public Transform OneRewardPanel;
        public Transform Rank1;
        public Transform Rank2;
        public Transform TopReward;
        public Transform NormalEntryFee;
        public MyButton DepositeBtn;
        public MyButton ruleBtn;
        public MyButton CloseBtn;
        public Text bottomTitle;
        private Room room;

        private IDisposable timer;
        public override UIType uiType { get; set; } = UIType.Window;

        private Action onNormalRoomClick;

        public override void OnStart()
        {
            Init();
        }

        private void Init()
        {
            room = GetArgsByIndex<Room>(0);
            onNormalRoomClick = GetArgsByIndex<Action>(1);
            DepositeBtn.Gray = false;
            RegisterEvent();

            OneRewardPanel.SetActive(false);
            TwoRewardPanel.SetActive(false);
            FiveRewardPanel.SetActive(false);

            var in_item = room.GetInItem();

            switch (room.out_items.Count)
            {
                case 1:
                    Init1RewardPanel(in_item);
                    OneRewardPanel.SetActive(true);
                    break;
                case 2:
                    Init2RewardPanel(in_item);
                    TwoRewardPanel.SetActive(true);
                    break;
                case 3:
                case 4:
                case 5:
                    if (room.out_items.Count == 3)
                    {
                        gameObject.FindChild<Transform>("Panel/five rewards/TopRanks").localPosition =
                            new Vector3(6, 100, 0);
                    }
                    Init5RewardPanel(in_item);
                    FiveRewardPanel.SetActive(true);
                    break;
            }

            timer?.Dispose();
            
            DepositeBtn.title = I18N.Get("key_play");
            bottomTitle.text = room.RoomEntryDesc;

            if (room.IsFreeBonusRoom)
            {
                if (Root.Instance.FreeBonusInfo.Lock)
                {
                    DepositeBtn.Gray = true;

                    void SetLessTime()
                    {
                        var timeSpan = TimeUtils.Instance.EndDayTimeStamp - TimeUtils.Instance.UtcTimeNow;
                        DepositeBtn.title = TimeUtils.Instance.ToHourMinuteSecond(timeSpan);
                    }

                    SetLessTime();
                    timer = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(l => { SetLessTime(); })
                        .AddTo(this);
                }
                else
                {
                    DepositeBtn.title = Root.Instance.FreeBonusInfo.CanPlay
                        ? I18N.Get("key_play")
                        : I18N.Get("key_deposit");
                    DepositeBtn.SetClick(GoFreeBonusRoom);
                }
            }
        }

        private void Init1RewardPanel(Item in_item)
        {
            var reward1 = room.GetRankReward(1);
            if (reward1 is { Count: > 0 })
            {
                var item = reward1[0];
                InitItemCount(TopReward.gameObject, item);

                TopReward.gameObject.FindChild<Image>("itemicon").sprite = reward1[0].GetIcon();
            }

            if (room.IsADRoom)
            {
                NormalEntryFee.gameObject.FindChild("itemCount").SetActive(false);
                NormalEntryFee.gameObject.FindChild("itemicon").SetActive(false);
                NormalEntryFee.gameObject.FindChild("free").SetActive(false);
                NormalEntryFee.gameObject.FindChild("watchAd").SetActive(true);
            }
            else //普通房间
            {
                if (in_item != null)
                {
                    if (!room.IsLuckyRoom)
                    {
                        InitItemCount(NormalEntryFee.gameObject, in_item);
                        NormalEntryFee.Find("free").SetActive(false);
                    }
                    else
                    {
                        NormalEntryFee.Find("free").SetActive(true);
                        SetItemTextActive(NormalEntryFee.gameObject, false);
                    }

                    NormalEntryFee.gameObject.FindChild<Image>("itemicon").sprite = in_item.GetIcon();
                }
            }
        }

        private void SetAdBtn()
        {
            if (Root.Instance.RoomAdInfo.IsLock)
            {
                DepositeBtn.Gray = true;

                void SetAdTime()
                {
                    var timeSpan = Root.Instance.RoomAdInfo.LessTime;
                    DepositeBtn.title = TimeUtils.Instance.ToHourMinuteSecond(timeSpan);
                }

                SetAdTime();
                timer = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(l => { SetAdTime(); })
                    .AddTo(this);
            }
            else if (Root.Instance.RoomAdInfo.RoomCanPlay(room.id))
            {
                DepositeBtn.SetClick(EntryMatch);
            }
            else
            {
                DepositeBtn.title = I18N.Get("key_free");
                DepositeBtn.SetClick(() =>
                {
                    ADSManager.Shared.YZShowReward("ADRoom", status =>
                    {
                        if (status == AdsStatus.REWARD)
                        {
                            MediatorRequest.Instance.WatchADRoomAD(room.id, Close);
                        }
                    });
                });
            }
        }

        private void Init2RewardPanel(Item in_item)
        {
            var reward1 = room.GetRankReward(1);
            if (reward1 is { Count: > 0 })
            {
                Rank1.gameObject.FindChild<Text>("itemCount").text =
                    reward1[0].Count.ToString();
                Rank1.gameObject.FindChild<Image>("itemicon").sprite = reward1[0].GetIcon();
            }

            var reward2 = room.GetRankReward(2);
            if (reward2 is { Count: > 0 })
            {
                var textCom = Rank2.gameObject.FindChild<Text>("itemCount");
                textCom.text = reward2[0].Count.ToString();
                MediatorItem.Instance.SetItemText(reward2[0].id, textCom);
                Rank2.gameObject.FindChild<Image>("itemicon").sprite = reward2[0].GetIcon();
            }

            var inItemIsNull = in_item == null;

            var itemCountObj = RewardIs2EnterFee.FindChild("itemCount");
            itemCountObj.SetActive(!inItemIsNull);
            var itemIconObj = RewardIs2EnterFee.FindChild("itemicon");
            itemIconObj.SetActive(!inItemIsNull);
            RewardIs2EnterFee.FindChild("free").SetActive(inItemIsNull);

            if (!inItemIsNull)
            {
                var textCom = itemCountObj.GetComponent<Text>();
                MediatorItem.Instance.SetItemText(in_item.id, textCom);
                textCom.text = in_item.Count.ToString();

                itemIconObj.GetComponent<Image>().sprite = in_item.GetIcon();
            }
        }


        private void Init5RewardPanel(Item in_item)
        {
            for (int i = 0; i < 5; i++)
            {
                var rank = i + 1;
                var reward = room.GetRankReward(rank);
                GameObject obj;
                if (i < 3)
                {
                    obj = FiveRewardPanel.gameObject.FindChild($"TopRanks/Rank{rank}");
                }
                else
                {
                    obj = FiveRewardPanel.gameObject.FindChild($"Rank{rank}");
                }
             
                if (reward == null)
                {
                    obj.SetActive(false);
                    continue;
                }
                
                var textCom = obj.FindChild<Text>("itemCount");
                textCom.text = reward[0].Count.ToString();
                MediatorItem.Instance.SetItemText(reward[0].id, textCom);
                obj.FindChild<Image>("itemicon").sprite = reward[0].GetIcon();
            }

            var inItemIsNull = in_item == null;

            var enterFeeObj = FiveRewardPanel.Find("entryfee").gameObject;
            var itemCountObj = enterFeeObj.FindChild("itemCount");
            itemCountObj.SetActive(!inItemIsNull);
            var itemIconObj = enterFeeObj.FindChild("itemicon");
            itemIconObj.SetActive(!inItemIsNull);
            enterFeeObj.FindChild("free").SetActive(inItemIsNull);

            if (!inItemIsNull)
            {
                var textCom = itemCountObj.GetComponent<Text>();
                MediatorItem.Instance.SetItemText(in_item.id, textCom);
                textCom.text = in_item.Count.ToString();

                itemIconObj.GetComponent<Image>().sprite = in_item.GetIcon();
            }
        }

        private void RegisterEvent()
        {
            if (room.IsADRoom)
            {
                SetAdBtn();
            }
            else
            {
                DepositeBtn.SetClick(() =>
                {
                    onNormalRoomClick?.Invoke();
                    Close();
                });
            }

            CloseBtn.SetClick(OnCloseBtnClick);
            ruleBtn.SetClick(() =>
            {
                string desc = "";
                if (room.IsFreeBonusRoom)
                {
                    desc = I18N.Get("key_freebonus_room_rule");
                }
                else
                {
                    desc = I18N.Get("key_normal_room_rule");
                }

                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    title = I18N.Get("key_rule"),
                    desc = desc,
                    Type = UIConfirmData.UIConfirmType.OneBtn,
                    confirmTitle = I18N.Get("key_ok"),
                    Rect2D = new Vector2(650, 904),
                    Position = new Vector2(0, 15),
                    AligmentType = TextAnchor.MiddleLeft
                });
            });
        }

        void InitItemCount(GameObject gameObject, Item item)
        {
            var bonusText = gameObject.FindChild<Text>("itemCount");
            bonusText.SetActive(true);
            if (item.id is Const.Bonus or Const.Cash)
            {
                bonusText.text = item.Count.ToString();
            }
            else if (item.id is Const.Chips)
            {
                bonusText.SetActive(false);
                var diamondCountText = gameObject.FindChild<Text>("diamondCount");
                diamondCountText.SetActive(true);
                diamondCountText.text = item.Count.ToString();
            }
            else
            {
                bonusText.SetActive(false);
                var coinCount = gameObject.FindChild<Text>("coinCount");
                coinCount.SetActive(true);
                coinCount.text = item.Count.ToString();
            }
        }

        void SetItemTextActive(GameObject gameObject, bool isActive)
        {
            var textCom1 = gameObject.FindChild<Text>("itemCount");
            var textCom2 = gameObject.FindChild<Text>("diamondCount");
            var textCom3 = gameObject.FindChild<Text>("coinCount");
            var imageCom = gameObject.FindChild<Image>("itemicon");
            if (textCom1 != null)
            {
                textCom1.SetActive(isActive);
            }

            if (textCom2 != null)
            {
                textCom2.SetActive(isActive);
            }

            if (textCom3 != null)
            {
                textCom3.SetActive(isActive);
            }
            if (imageCom != null)
            {
                imageCom.SetActive(isActive);
            }
        }

        void GoFreeBonusRoom()
        {
            if (Root.Instance.FreeBonusInfo.CanPlay)
            {
                EntryMatch();
            }
            else
            {
                Close();
                MediatorRequest.Instance.Charge(Root.Instance.FreeBonusInfo.ChargeInfo, ActivityType.FreeBonusRoom);
            }
        }

        void EntryMatch()
        {
            Close();
            MediatorRequest.Instance.MatchBegin(room);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Pass_Day, (sender, eventArgs) => Init());
        }
    }
}