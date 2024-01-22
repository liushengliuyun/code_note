using System;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.AudioService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace UI
{
    public class UIMatch : UIBase<UIMatch>
    {
        [SerializeField] private Transform _5SeatGroup;

        [SerializeField] private Transform _1v1Group;

        [SerializeField] private GameObject[] PlayerIconObjs;
        
        [SerializeField] private Image PlayerIcon;

        [SerializeField] private Text MyNameText;

        [SerializeField] private Transform GuideMask;
        public MyButton StartBtn;

        public Text CountDownText;

        public Text RewardDesc;
        public Text RewardNumText;

        public Text CostText;

        public Image RewardIcon;

        public Text PlayerNumberText;

        public Transform PlayerGroup2;

        public Image CostIcon;

        //匹配时间
        private const int totalMatchTime = 5;

        private string matchId;

        private Room room;

        enum vname
        {
            /// <summary>
            /// 新匹配到的
            /// </summary>
            newMatchInfo,
            roomInfo,
            matchEndSuccess
        }

        public override void OnStart()
        {
            if (UIMain.IsFirstRoomGuideRunning)
            {
                EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
            }

            _5SeatGroup.SetActive(false);
            _1v1Group.SetActive(false);

            var role = Root.Instance.Role;

            role.LoadIcon(PlayerIcon);

            MyNameText.text = role.nickname;

            matchId = GetArgsByIndex<string>(0);


            var argRoom = GetArgsByIndex<Room>(1);
            if (argRoom != null)
            {
                room = argRoom;
            }
            else
            {
                var roomId = GetArgsByIndex<int>(1);
                room = Root.Instance.RoomList.Find(room1 => room1.id == roomId);

                //防御
                if (room == null)
                {
                    room = new Room();
                    room.id = roomId;
                    //充值免费场
                    room.type = "0";
                }
            }

            if (room == null)
            {
                Close();
            }

            if (room.seat <= 2)
            {
                _1v1Group.SetActive(true);
                
                if (Root.Instance.DuelStatusInfo != null )
                    LoadPlayerGroup(null, _1v1Group.Find("PlayerIcon"));
            }
            else
            {
                _5SeatGroup.SetActive(true);
            }

            AudioSystem.That.PlaySound(SoundPack.Match_Count_Down);

            Observable.Interval(TimeSpan.FromSeconds(1f))
                .TakeWhile(l => l <= totalMatchTime)
                .Subscribe(
                    l => { Interval(l + 1); }).AddTo(this);

            //开始游戏
            StartBtn.OnClickAsObservable().Subscribe(unit =>
            {
                if (!vm[vname.matchEndSuccess.ToString()].ToIObservable<bool>().Value)
                {
                    return;
                }

                Begin();
            });

            var isRunGuide = GetArgsByIndex<bool>(2);
            GuideMask.SetActive(isRunGuide);
        }

        public override void InitVm()
        {
            vm[vname.newMatchInfo.ToString()] = new ReactivePropertySlim<Match>();
            vm[vname.roomInfo.ToString()] = new ReactivePropertySlim<Room>(room);
            vm[vname.matchEndSuccess.ToString()] = new ReactivePropertySlim<bool>();
        }

        public override void InitBinds()
        {
            vm[vname.matchEndSuccess.ToString()].ToIObservable<bool>().Subscribe(value => { StartBtn.Gray = !value; });

            vm[vname.roomInfo.ToString()].ToIObservable<Room>().Subscribe(value =>
            {
                if (value == null)
                {
                    return;
                }

                if (room.seat == 2)
                {
                    PlayerNumberText.text = I18N.Get("TWO_SEAT");
                }
                else
                {
                    PlayerNumberText.text = I18N.Get("SEAT_NUM", room.seat);
                }

                //1 是第一名的奖励
                var topReward = value.GetRankReward(1);
                if (topReward != null)
                {
                    RewardDesc.text = I18N.Get("top_rank_reward", Item.GetItemsName(topReward));
                    if (topReward.Any())
                    {
                        RewardIcon.sprite = topReward[0].GetIcon();
                        RewardNumText.text = topReward[0].Count.ToString();
                    }
                }

                var inItem = value.GetInItem();

                if (inItem == null || value.IsLuckyRoom)
                {
                    CostIcon.SetActive(false);
                    CostText.text = I18N.Get("key_free");
                }
                else
                {
                    var costIconSprite = inItem.GetIcon();
                    CostIcon.SetActive(true);
                    CostIcon.sprite = costIconSprite;
                    CostText.text = inItem.Count.ToString();
                }
            });

            vm[vname.newMatchInfo.ToString()].ToIObservable<Match>().Subscribe(value =>
            {
                if (value == null)
                {
                    return;
                }
                AudioSystem.That.PlaySound(SoundPack.Match_Success);
                //房间是否已经满了
                bool isRoomFull = true;
                if (_1v1Group.IsActive())
                {
                    LoadPlayerGroup(value, _1v1Group.Find("PlayerIcon"));
                    FindChild<Transform>("Searching", _1v1Group).SetActive(false);
                }
                else
                {
                    foreach (var obj in PlayerIconObjs)
                    {
                        var nameText = obj.FindChild("NameText");
                        if (nameText.transform.IsActive())
                        {
                            continue;
                        }
                        LoadPlayerGroup(value, obj.transform);
                        break;
                    }
                    
                    foreach (var obj in PlayerIconObjs)
                    {
                        var nameText = obj.FindChild("NameText");
                        if (!nameText.transform.IsActive())
                        {
                            isRoomFull = false;
                        }
                    }
                }

                if (isRoomFull)
                {
                    this.AttachTimer(1f, Begin);
                }
               
            });
        }

        void LoadPlayerGroup(Match value, Transform parent)
        {
            var enemyIcon = parent.gameObject.FindChild<Image>("mask/EnemyIcon");
            var nameText =  parent.gameObject.FindChild<Text>("NameText");
            
            // 邀请对战立即显示对手
            if (Root.Instance.DuelStatusInfo != null && 
                (Root.Instance.DuelStatusInfo.status == 1 || Root.Instance.DuelStatusInfo.status == 2))
            {
                Match mTemp = new Match();
                mTemp.head_url = Root.Instance.DuelStatusInfo.competitor.head_url;
                mTemp.head_index = Root.Instance.DuelStatusInfo.competitor.head_index;
                mTemp.nickname = Root.Instance.DuelStatusInfo.competitor.nickname;
                
                LoadIcon(mTemp, enemyIcon);
                SetPlayerName(mTemp, nameText);
                nameText.SetActive(true);
                
                FindChild<Transform>("Searching", _1v1Group).SetActive(false);
                
                return;
            }
            

            LoadIcon(value, enemyIcon);

            SetPlayerName(value, nameText);
            
            nameText.SetActive(true);
        }
        
        private void SetPlayerName(Match value, Text nameText)
        {
            nameText.text = value.nickname;
            nameText.transform.SetActive(true);
        }

        private void LoadIcon(Match value, Image enemyIcon)
        {
            if (value == null)
            {
                return;
            }
            if (!value.head_url.IsNullOrEmpty())
            {
                enemyIcon.ServerUrl(value.head_url);
            }
            else
            {
                var sprite = Root.Instance.LoadPlayerIconByIndex(value.head_index);
                if (sprite != null)
                {
                    enemyIcon.sprite = sprite;
                }
            }
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_New_Match,
                (sender, args) => { vm[vname.newMatchInfo.ToString()].ToIObservable<Match>().Value = sender as Match; });

            AddEventListener(Proto.GAME_BEGIN,
                (sender, args) =>
                {
                    //成功或服务器返回失败 都close
                    if (args is ProtoEventArgs { Result: ProtoResult.Success or ProtoResult.Fail })
                    {
                        Close();
                    }
                });

            AddEventListener(Proto.MATCH_END,
                (sender, args) =>
                {
                    if (args is ProtoEventArgs { Result: ProtoResult.Success })
                    {
                        vm[vname.matchEndSuccess.ToString()].ToIObservable<bool>().Value = true;
                    }
                });
        }

        private bool isBegin = false;

        private void Begin()
        {
            if (isBegin)
            {
                return;
            }

            isBegin = true;

            MediatorRequest.Instance.GameBegin(matchId, room);
        }

        private bool isMatchEndSuccess;

        private void Interval(long takeTime)
        {
            var countDown = totalMatchTime - takeTime;

            if (countDown > 0 && !isBegin)
            {
                MediatorRequest.Instance.MatchEnd(matchId, room);
            }

            if (countDown <= 0)
            {
                Begin();
                return;
            }

            CountDownText.text = countDown.ToString();
        }

        private void OnDisable()
        {
            AudioSystem.That.StopSound(SoundPack.Match_Count_Down);
        }
    }
}