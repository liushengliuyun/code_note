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
using UI.Effect;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class UIDuelReady : UIBase<UIDuelReady>
    {
        public override UIType uiType { get; set; } = UIType.Window;

        [SerializeField] private MyText roomTitle;
        [SerializeField] private MyText roomIdText;
        [SerializeField] private MyText entryText;
        [SerializeField] private MyText poolText;
        [SerializeField] private MyText timeText;

        [SerializeField] private MyButton closeBtn;

        [SerializeField] private GameObject yellowBg;
        [SerializeField] private GameObject purpleBg;

        [SerializeField] private Image myIcon;
        [SerializeField] private Image myFriendIcon;

        [SerializeField] private MyText roomIdTittle;
        [SerializeField] private MyText entryTittle;
        [SerializeField] private MyText poolTittle;

        private const int WaitTime = 180;
        
        private const float RequestIntervalTime = 1f;
        private float requestTimer = 0;
        
        private int create_time;
        private int end_time;
/// <summary>
/// room ID的颜色
/// </summary>
        private Color yellow;
        private Color purple;
        private Color yellowDark;
        private Color purpleDark;
        
        private bool isYellow;

        private bool friendsIsIn;
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_Friends_Duel_Room, (sender, eventArgs) =>
            {
                RefreshData();
            });
        }

        public override void OnStart()
        {
            ColorUtility.TryParseHtmlString("#4a4ed3", out var yellowColor);
            yellow = yellowColor;
            purple = new Color(126f / 255f, 62f / 255f, 171f / 255f);
            ColorUtility.TryParseHtmlString("#292476", out var darkBlueColor);
            yellowDark = darkBlueColor;
            purpleDark = new Color(101f / 255f, 43f / 255f, 144f / 255f);
            
            closeBtn.SetClick(() =>
            {
                // 取消邀请房间
                MediatorRequest.Instance.CloseFriendsDuelRoom(Root.Instance.DuelData.id, 3);
                
                Close();
            });
            
            roomIdText.text = Root.Instance.DuelData.room_no;
            var room = Root.Instance.RoomList.Find(room => room.id == Root.Instance.DuelData.room_id);

            roomTitle.text = room.name;
            
            //设置入场费
            foreach (var inItem in room.in_items)
            {
                entryText.text = "$" + GameUtils.TocommaStyle(inItem.Value);
            }

            poolText.text = "$" + GameUtils.TocommaStyle(room.PrizePool);

            create_time = Root.Instance.DuelData.create_time;
            end_time = create_time + WaitTime;

            isYellow = room.name.Contains("Thorn");
            
            // 根据房间显示颜色
            yellowBg.SetActive(isYellow);
            purpleBg.SetActive(!isYellow);

            roomTitle.GetComponent<Outline8>().effectColor = isYellow? yellow : purple;
            roomTitle.GetComponent<Shadow>().effectColor = isYellow? yellow : purple;
            
            roomIdTittle.GetComponent<Outline8>().effectColor = isYellow? yellow : purple;
            roomIdTittle.GetComponent<Shadow>().effectColor = isYellow? yellow : purple;

            entryTittle.color = isYellow ? yellowDark : purpleDark;
            poolTittle.color = isYellow ? yellowDark : purpleDark;
            
            // 显示头像
            var role = Root.Instance.Role;
            role.LoadIcon(myIcon);
            
            
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        private void RefreshData()
        {
            if (Root.Instance.DuelStatusInfo != null && Root.Instance.DuelStatusInfo.competitor != null
                && (Root.Instance.DuelStatusInfo.status == 1 || Root.Instance.DuelStatusInfo.status == 2))
            {
                // 匹配成功 显示朋友头像
                closeBtn.interactable = false; // 匹配成功后不允许关闭

                if (!friendsIsIn)
                {
                    // 保证只执行一次，防止多次进入
                    friendsIsIn = true;

                    if (!Root.Instance.DuelStatusInfo.competitor.head_url.IsNullOrEmpty())
                        myFriendIcon.ServerUrl(Root.Instance.DuelStatusInfo.competitor.head_url);
                    else
                    {
                        var sprite =
                            Root.Instance.LoadPlayerIconByIndex(Root.Instance.DuelStatusInfo.competitor.head_index);
                        if (sprite != null)
                        {
                            myFriendIcon.sprite = sprite;
                        }
                    }
                    
                    TinyTimer.StartTimer(() =>
                    {
                        MediatorRequest.Instance.DuelMatchBegin(Root.Instance.DuelStatusInfo.room_id, 
                            Root.Instance.DuelStatusInfo.room_no);
                        
                        Close();
                        
                        UserInterfaceSystem.That.RemoveUIByName("UIFriendsDuel");

                        // 更新一下红点
                        MediatorRequest.Instance.GetFriendsDuelInfo();
                    }, 0.5f);
                }
            }
        }

        private void FixedUpdate()
        {
            int timeSpan = end_time - TimeUtils.Instance.UtcTimeNow;
            timeText.text = TimeUtils.Instance.ToHourMinuteSecond(timeSpan);

            if (timeSpan < 0)
            {
                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    Type = UIConfirmData.UIConfirmType.OneBtn,
                    HideCloseBtn = false,
                    title = "Notice",
                    desc = I18N.Get("key_friends_duel_wait_end"),
                    confirmTitle = "OK",
                    WaitCloseCallback = true,
                    confirmCall = () =>
                    {
                        UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm));
                    },
                    cancleCall = () =>
                    {
                        UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm));
                    }
                });
                MediatorRequest.Instance.CloseFriendsDuelRoom(Root.Instance.DuelData.id, 4);
                Close();
            }

            if (requestTimer < RequestIntervalTime)
            {
                requestTimer += Time.deltaTime;
            }
            else
            {
                // 1秒请求一次状态
                requestTimer = 0;
                MediatorRequest.Instance.GetFriendsDuelRoomStatus(Root.Instance.DuelData.id);
            }
        }
    }
}