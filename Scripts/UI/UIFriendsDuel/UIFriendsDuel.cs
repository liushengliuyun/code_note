using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using NCat;
using UI.Effect;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;

namespace UI
{
    public class UIFriendsDuel : UIBase<UIFriendsDuel>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private List<GameObject> rewardItemList;
        [SerializeField] private List<GameObject> roomList;
        [SerializeField] private MyText invitedCount;
        [SerializeField] private MyButton closeBtn;
        [SerializeField] private MyButton descBtn;
        [SerializeField] private Slider rewardSlider;
        [SerializeField] private MyButton joinBtn;
        [SerializeField] private MyButton exitBtn;
        
        private int _rewardCount = 0;

        private List<FriendsDuelConfig> _configs;
        private List<Room> _rooms;

        private float[] _sliderRuler = new []{0, 0.08f, 0.31f, 0.54f, 0.77f, 1f, 1}; // 进度条刻度尺
        private int[] _sliderConfig = new[] {0, 0, 0, 0, 0, 0, 30}; // 刻度尺对应的配置
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_Friends_Duel_Info, (sender, eventArgs) =>
            {
                RefreshData();
            });
            AddEventListener(GlobalEvent.Claim_Duel_Reward, (sender, eventArgs) =>
            {
                ShowClaimedDuelReward();
            });
            AddEventListener(GlobalEvent.GO_TO_MAIN_STORE, (sender, eventArgs) =>
            {
                // 资源不足可能会跳到主商店界面，那么需要关闭当前页面
                Close();
            });
        }

        public override void OnStart()
        {
            joinBtn.SetClick(() =>
            {
                int itemType = -1;
                foreach (var inItem in _rooms[0].in_items)
                {
                    itemType = inItem.Key;
                }
                
                //检查资源是否足够
                if (MediatorItem.Instance.CheckResEnough(itemType, 6))
                {
                    UserInterfaceSystem.That.ShowUI<UIDuelJoin>();
                }
                else
                {
                    //IsResourceEnough = false;
                    MediatorItem.Instance.ResNotEnoughGoTo(itemType, _rooms[0], true);
                }
            });
            exitBtn.SetClick(Close);
            closeBtn.SetClick(OnCloseBtnClick);
            descBtn.SetClick(() =>
            {
                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    Type = UIConfirmData.UIConfirmType.OneBtn,
                    // HideCloseBtn = true,
                    desc = I18N.Get("key_friends_duel_desc"),
                    confirmTitle = I18N.Get("key_ok"),
                    AligmentType = TextAnchor.MiddleLeft,
                    Rect2D = new Vector2(680, 1020),
                    title = "Friends Duel",
                    // Position = new Vector2(0, 15),
                });
            });
            
            _configs = Root.Instance.FriendsDualConfigs;
            if (_configs == null)
            {
                Close();
                return;
            }

            _rewardCount = _configs.Count;
            ShowClaimedDuelReward();
            
            // 展示房间列表
            _rooms = GetFriendsDuelRooms();
            for (int i = 0; i < _rooms.Count; ++i)
            {
                InitRooms(roomList[i], i);
            }

            YZFunnelUtil.SendYZEvent("dz_ui_popup");

            rewardSlider.value = PlayerPrefs.GetFloat(Root.Instance.Role.user_id + "DuelSlider", 0);
            invitedCount.text = Root.Instance.DuelInfo.match_count.ToString();
        }

        private List<Room> GetFriendsDuelRooms()
        {
            return Root.Instance.RoomList.FindAll(room => room.type == "15");
        }

        private void InitRooms(GameObject item, int index)
        {
            var room = _rooms[index];
            var roomMono = item.GetComponent<RoomItemMono>();
            roomMono.BeforeInit();
            roomMono.PlayAnimation();
            roomMono.MuseumCount = (int)room.museum_point;
            roomMono.MagicCount = room.wizard_treasure_point;
            roomMono.TitleText.text = I18N.Get(room.title);
            roomMono.NameText.text = I18N.Get(room.name);
            roomMono.TitleBG.sprite = MediatorBingo.Instance.GetSpriteByUrl(room.TitleBG);
            roomMono.SubTitleText.text = I18N.Get(room.sub_title);

            roomMono.DescText.text = room.seat == 2 ? I18N.Get("TWO_SEAT") : I18N.Get("PLAYER_NUM", room.seat);
            
            roomMono.DescText.color = room.DescTextColor;
            // roomMono.gameObject.FindChild<Text>("root/RewardDesc").color = room.RewardDescColor;
            //使用预制体上的
            roomMono.RoomIcon.sprite = MediatorBingo.Instance.GetSpriteByUrl(room.Icon);
            // roomMono.gameObject.FindChild<Image>("root/bg0").sprite = MediatorBingo.Instance.GetSpriteByUrl(room.Bg);
            roomMono.RoomIcon.SetNativeSize();
            roomMono.roomId = room.id;
            roomMono.LastContestGroup.SetActive(Root.Instance.UserInfo.last_cash_room == room.id);
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

            roomMono.RewardText.text = "$" + GameUtils.TocommaStyle(room.PrizePool);

            //代码耦合太多, 不提出来了
            if (room.IsFriendsDuelRoom)
            {
                roomMono.IsNormal = true;
                int needCount = 0;
                int itemType = -1;

                roomMono.IsFree = false;

                //设置入场费
                foreach (var inItem in room.in_items)
                {
                    needCount = (int)inItem.Value;
                    itemType = inItem.Key;
                    roomMono.CostIcon.sprite = MediatorItem.Instance.GetItemSprite(itemType);
                    roomMono.CostText.text = "$" + GameUtils.TocommaStyle(inItem.Value);
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
                            YZDebug.Log("账号冻结");
                            return;
                        }

                        bool checkLocation = false;
                        // 检查定位是否开启
                        LocationManager.Shared.IsLocationValid(YZSafeType.Money, null, -1, () =>
                        {
                            // 关闭等待界面
                            UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                            YZNativeUtil.GetYZLocation(true, (temp) => { YZDebug.LogConcat("真机定位: ", temp); });
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
                            if (MediatorItem.Instance.CheckResEnough(itemType, needCount))
                            {
                                //IsResourceEnough = true;
                                YZLog.LogColor($"进入房间 开始匹配 room_id= {room.id}");
                                YZDebug.Log("进入房间 开始匹配");
                                
                                //MediatorRequest.Instance.MatchBegin(room);
                                MediatorRequest.Instance.CreateFriendsDuelRoom(room.id);
                            }
                            else
                            {
                                //IsResourceEnough = false;
                                // UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("RES_NOT_ENOUGH"));
                                MediatorItem.Instance.ResNotEnoughGoTo(itemType, room, true);
                            }
                        });
                    }
                    else
                    {
                        //检查资源是否足够 -1?
                        if (itemType == -1 || MediatorItem.Instance.CheckResEnough(itemType, needCount))
                        {
                            //IsResourceEnough = true;
                            YZLog.LogColor($"进入房间 开始匹配 room_id= {room.id}");
                            YZDebug.Log("进入房间 开始匹配");
                            
                            //MediatorRequest.Instance.MatchBegin(room);
                            MediatorRequest.Instance.CreateFriendsDuelRoom(room.id);
                        }
                        else
                        {
                            //IsResourceEnough = false;
                            UserInterfaceSystem.That.ShowUI<UITip>(itemType is Const.Chips
                                ? I18N.Get("key_http_code_1103")
                                : I18N.Get("RES_NOT_ENOUGH"));
                        }
                    }
                }

                Action action = OnNormalRoomPlayBtnClick;
                roomMono.RootBtn.SetClick(() => { UserInterfaceSystem.That.ShowUI<UIRoomEntry>(room, action); });

                roomMono.Button.SetClick(OnNormalRoomPlayBtnClick);
            }
            else
            {
                roomMono.RootBtn.SetClick(() => UserInterfaceSystem.That.ShowUI<UIRoomEntry>(room));
                roomMono.InitSpecialRoom(room);
            }
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        protected override void OnAnimationOut()
        {
            var table = GetTable();
            var panel = transform.GetChild(1);
            if (table?["isTriggerPop"] is true)
            {
                DOTween.Sequence().SetDelay(0.1f).SetId(UIName);
                
                var mask = transform.GetChild(0);
                mask.SetActive(false);
        
                var uimain = UserInterfaceSystem.That.Get<UIMain>();
                UIEffectUtils.Instance.CaptureAndShrink(panel, UICanvas, uimain.DuelEntryBtn.transform, false);
            }
            else
            {
                panel.DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic).SetId(UIName);
            }
        }

        private void RefreshData()
        {
            invitedCount.text = Root.Instance.DuelInfo.match_count.ToString();
            // 更新进度条
            int currentCount = Root.Instance.DuelInfo.match_count;

            if (currentCount > _sliderConfig[^1])
            {
                rewardSlider.value = rewardSlider.maxValue;
            }
            else
            {
                int stage = 0;
                for (int i = 0; i < 6; ++i)
                {
                    if (currentCount > _sliderConfig[i])
                    {
                        stage = i;
                    }
                }
            

                float valueDelta = _sliderRuler[stage + 1] - _sliderRuler[stage];
                int countDelta = _sliderConfig[stage + 1] - _sliderConfig[stage];
                float toValue = _sliderRuler[stage] + 
                                (valueDelta / countDelta) * (currentCount - _sliderConfig[stage]);
            
                float fromValue = PlayerPrefs.GetFloat(Root.Instance.Role.user_id + "DuelSlider", 0);
                DOTween.To(() => fromValue, x => rewardSlider.value = x, toValue, 0.5f).OnComplete(()=>
                {
                    if (RedPointNotify.GetCount(ERedPointItem.FriendsDuel) > 0)
                        MediatorRequest.Instance.ClaimFriendsDuel();
                });
            
                // 保存当前进度条的值
                PlayerPrefs.SetFloat(Root.Instance.Role.user_id + "DuelSlider", toValue);
            }
 
            ShowClaimedDuelReward();
        }

        private void ShowClaimedDuelReward()
        {
            // 将领取过的奖励置灰
            for (int i = 0; i < Root.Instance.DuelInfo.last_claimed; ++i)
            {
                rewardItemList[i].transform.Find("Icon").SetGray(true);
                rewardItemList[i].transform.Find("RoundBg").SetGray(true);
                rewardItemList[i].transform.Find("CountText").SetGray(true);
            }
            
            // 展示奖励
            for (int i = 0; i < _rewardCount; ++i)
            {
                _sliderConfig[i + 1] = (int)_configs[i].amount;
                SetItem(rewardItemList[i], _configs[i]);
            }
        }

        private void SetItem(GameObject itemObj, FriendsDuelConfig awardItem)
        {
            itemObj.transform.Find("Icon").Find("1").SetActive(false);
            itemObj.transform.Find("Icon").Find("2").SetActive(false);
            itemObj.transform.Find("Icon").Find("3").SetActive(false);
            itemObj.transform.Find("Icon").Find("4").SetActive(false);

            itemObj.transform.Find("CountText").Find("1").SetActive(false);
            itemObj.transform.Find("CountText").Find("2").SetActive(false);
            itemObj.transform.Find("CountText").Find("3").SetActive(false);
            itemObj.transform.Find("CountText").Find("4").SetActive(false);

            itemObj.transform.Find("RoundBg/Text").GetComponent<MyText>().text = awardItem.amount.ToString();

            switch (awardItem.type)
            {
                case 1:
                    itemObj.transform.Find("Icon").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").GetComponent<MyText>().text = "$" +
                        YZNumberUtil.FormatYZMoney(awardItem.weight.ToString());

                    break;

                case 2:
                    itemObj.transform.Find("Icon").Find("2").SetActive(true);
                    itemObj.transform.Find("CountText").Find("2").SetActive(true);

                    itemObj.transform.Find("CountText").Find("2").GetComponent<MyText>().text =
                        YZNumberUtil.FormatYZMoney(awardItem.weight.ToString());

                    break;

                case 3:
                    itemObj.transform.Find("Icon").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").GetComponent<MyText>().text = "$" +
                        YZNumberUtil.FormatYZMoney(awardItem.weight.ToString());


                    break;

                case 4:
                    itemObj.transform.Find("Icon").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").GetComponent<MyText>().text =
                        YZNumberUtil.FormatYZMoney(awardItem.weight.ToString());

                    break;
            }
        }
    }
}