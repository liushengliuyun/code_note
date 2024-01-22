using System;
using BrunoMikoski.AnimationsSequencer;
using Core.Controllers;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace UI
{
    public class RoomItemMono : MonoBehaviour
    {
        
        [NonSerialized] public int roomId;
        
        [SerializeField] public Transform LastContestGroup;
        
        [SerializeField] private AnimationSequence animationSequence;
        [SerializeField] public MyButton RootBtn;
        [SerializeField] private MyButton FreeEnterBtn;
        [SerializeField] private MyButton CostBtn;
        [SerializeField] private MyButton FreeBousBtn;
        
        [SerializeField] private Transform NormalPanel;
        [SerializeField] private Transform ADPanel;
        [SerializeField] private Transform FreeBonusPanel;
        [SerializeField] private Transform FreeBonusLock;
        [SerializeField] private Transform ADLock;
        [SerializeField] private MyButton AdBtn;
        [SerializeField] private Transform MuseumStarGroup;
        [SerializeField] private Transform MagicBallGroup;
        [SerializeField] private GameObject BgMulti;
        [SerializeField] public AnimationSequence ShowAnimation;
        
        public Text EntryText;
        public Image TitleBG;

        public MyText RewardText;

        public Text TitleText;

        public Text NameText;

        public Text DescText;

        public Text SubTitleText;

        public Image RewardIcon;

        public Image RoomIcon;

        public Image CostIcon;

        public MyText CostText;
        
        private bool isFree;

        public int MuseumCount
        {
            set
            {
                MuseumStarGroup.SetActive( Root.Instance.MuseumInfo is not {IsReachMax : true} && value > 0);
                MuseumStarGroup.gameObject.FindChild<Text>("MuseumText").text = value.ToString();
            }
        }

        public float MagicCount
        {
            set
            {
                if (!MediatorActivity.Instance.IsActivityBegin(ActivityType.MagicBall))
                {
                    return;
                }
                
                if (!MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.MagicBall))
                {
                    return;
                }
                
                MagicBallGroup.SetActive( !Root.Instance.MagicBallInfo.IsReachMax && value > 0);
                MagicBallGroup.gameObject.FindChild<Text>("CountText").text = value.ToString();
            }
        }

        public bool IsMulti
        {
            set
            {
                // BgMulti.SetActive(value);
            }
        }

        public bool IsFree
        {
            set
            {
                FreeEnterBtn.SetActive(value);
                CostBtn.SetActive(!value);
                isFree = value;
            }
        }
        
        private bool isNormal;

        public bool IsNormal
        {
            set
            {
                NormalPanel.SetActive(value);
                isNormal = value;
            }
        }
        
        
        private bool isFreeBonusRoom;
        
        public bool IsFreeBonusRoom
        {
            set
            {
                isFreeBonusRoom = value;
                InitFreeBonusStyle(value);
            }

            get => isFreeBonusRoom;
        }

        private bool isADRoom;

        public bool IsADRoom
        {
            get => isADRoom;

            set
            {
                isADRoom = value;
                ADPanel.SetActive(value);
            }
        }
        
        private void InitFreeBonusStyle(bool value)
        {
            FreeBonusPanel.SetActive(value);
            if (value)
            {
                var amount = Root.Instance.FreeBonusInfo.Amount;
                SubTitleText.text = I18N.Get("key_charge_room_entry_desc", amount);
                EntryText.text = I18N.Get("key_charge_room_entry_title", amount);
            }
            else
            {
                EntryText.text = I18N.Get("key_entry");
            }
        }

        public MyButton Button => isFree ? FreeEnterBtn : CostBtn;

        private IDisposable timer;

        public void BeforeInit()
        {
            NormalPanel.SetActive(false);
            FreeBonusPanel.SetActive(false);
            ADPanel.SetActive(false);
            EntryText.text = I18N.Get("key_entry");
        }
        
        public void PlayAnimation()
        {
            var random = Random.Range(0, 6);
            animationSequence.Kill();
            RewardIcon.transform.localScale = Vector3.one;
            Timer.Register(random, () => animationSequence.Play(), autoDestroyOwner: this);
        }
        
        public void InitSpecialRoom(Room room)
        {
            IsFreeBonusRoom = room.IsFreeBonusRoom;
            IsADRoom = room.IsADRoom;
            
            timer?.Dispose();
            if (isFreeBonusRoom)
            {
                var @lock = Root.Instance.FreeBonusInfo.Lock;
                if (Root.Instance.FreeBonusInfo.CanPlay)
                {
                    FreeBousBtn.title = I18N.Get("key_play");
                }
                else if (@lock)
                {
                    timer = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(l =>
                    {
                        var timeSpan = TimeUtils.Instance.EndDayTimeStamp - TimeUtils.Instance.UtcTimeNow;
                        FreeBousBtn.title =  TimeUtils.Instance.ToHourMinuteSecond(timeSpan);
                    }).AddTo(this);
                }
                else
                {
                    FreeBousBtn.title = I18N.Get("key_free");
                }
                
                FreeBousBtn.Gray = @lock;
                FreeBonusLock.SetActive(@lock);
                FreeBousBtn.SetClick(() =>
                {
                    if (Root.Instance.FreeBonusInfo.CanPlay)
                    {
                        MediatorRequest.Instance.MatchBegin(roomId);
                    }
                    else if (@lock)
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_charge_room_entry_tip"));
                    }
                    else
                    {
                        Charge();
                    }
                });
            }
            else if(IsADRoom)
            {
                var Lock = Root.Instance.RoomAdInfo.IsLock;
                AdBtn.Gray = Lock;
                ADLock.SetActive(Lock);
                if (Lock)
                {
                    timer = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(l =>
                    {
                        var timeSpan = Root.Instance.RoomAdInfo.LessTime;
                        AdBtn.title =  TimeUtils.Instance.ToHourMinuteSecond(timeSpan);
                    }).AddTo(this);
                    AdBtn.SetClick(() => UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_charge_room_entry_tip")));
                }
                else if (Root.Instance.RoomAdInfo.RoomCanPlay(roomId))
                {
                    AdBtn.title = I18N.Get("key_play");
                    AdBtn.SetClick(() =>
                    {
                        MediatorRequest.Instance.MatchBegin(roomId);
                    });
                }
                else
                {
                    AdBtn.title = I18N.Get("key_free");
                    AdBtn.SetClick(() =>
                    {
                        ADSManager.Shared.YZShowReward("ADRoom", status =>
                        {
                            if (status == AdsStatus.REWARD)
                                MediatorRequest.Instance.WatchADRoomAD(roomId);
                        });
                    });
                }
            }
        }

        private void Charge()
        {
            MediatorRequest.Instance.Charge(Root.Instance.FreeBonusInfo.ChargeInfo, ActivityType.FreeBonusRoom);
        }
    }
}