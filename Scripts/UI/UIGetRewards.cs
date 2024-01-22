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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.Services.UserInterfaceService.API;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UI;
using UI.Activity;
using UniRx;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

public class UIGetRewards : UIBase<UIGetRewards>
{
    #region UI Variable Statement

    [SerializeField] private Button button_Button_Ok;
    [SerializeField] private ScrollRect scrollrect_Scroll_View;

    #endregion


    private MyList itemList;

    public override UIType uiType { get; set; } = UIType.Window;

    /// <summary>
    /// 默认状态， cash 和 bonus 是分开显示的
    /// </summary>
    private bool isShowAll;

    private UnityEvent closeCallback = new();

    private List<Item> items = new List<Item>();

    public override void OnStart()
    {
        button_Button_Ok.OnClickAsObservable().Subscribe(unit => { OnOkBtnClick(); });
        InitItemList();
    }

    private void InitItems()
    {
        var table = GetArgsByIndex<GameData>(0);

        if (table?["diff"] is List<Item> inputItems)
        {
            items.AddRange(inputItems);
        }

        var uimain = UserInterfaceSystem.That.Get<UIMain>();
        if (uimain !=null && uimain.IsVisible)
        {
            var exItems = MediatorItem.Instance.GetExShowItem();

            if (exItems != null && exItems.Any())
            {
                if (items != null)
                {
                    items.AddRange(exItems);
                }
                else
                {
                    items = exItems;
                }
            }
        }
        
        if (items.Exists(item => item.name == "MuseumPoint"))
        {
            closeCallback.AddListener(UserInterfaceSystem.That.Get<UIMain>().PlayCollectMuseumPoint);
        }

        if (items.Exists(item => item.name == "MagicBallPoint"))
        {
            closeCallback.AddListener(UserInterfaceSystem.That.Get<UIMain>().PlayCollectMagicBallPoint);
        }

        if (items.Exists(item => item.name == "piggyBonus"))
        {
            closeCallback.AddListener(UserInterfaceSystem.That.Get<UIMain>().PlayPiggyBonusAnimation);
        }

        if (table != null) isShowAll = table["showAll"] is bool ? (bool)table["showAll"] : false;

        if (table?["closeCallback"] is Action action)
        {
            closeCallback.AddListener(() => action.Invoke());
        }

        if (!isShowAll)
        {
            //没有传参 或者传的false 合并显示
            var cash = items.Find(item => item.id == Const.Cash);
            var bonus = items.Find(item => item.id == Const.Bonus);
            if (cash != null && bonus != null)
            {
                cash.Count += bonus.Count;
                items.Remove(bonus);
            }

            var zeroValue = items.Find(item => item.Count <= 0);
            if (zeroValue != null)
            {
#if DEBUG
                UserInterfaceSystem.That.ShowUI<UITip>("zeroValue Find name is" + zeroValue.name);
#endif
                YZLog.LogColor("zeroValue Find name is" + zeroValue.name);
                items.Remove(zeroValue);
            }
        }

        itemList.NumItems = items.Count;
    }

    private void OnOkBtnClick()
    {
        var table = GetTable();

        if (table?["is_fake"] is true)
        {
            if (table?["closeCallBack"] is Action closeCallBack)
            {
                closeCallBack.Invoke();
            }
        }
        else
        {
            for (int i = 0; i < itemList.NumItems; i++)
            {
                var com = itemList.GetChildAt(i);
                var position = com.transform.position;
                var item = items[i];
                EventDispatcher.Root.Raise(GlobalEvent.GetItems, (position, item));
            }
        }

        OnCloseBtnClick();
        
        // 有每日任务待领取的奖励
        if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1)
        {
            EventDispatcher.Root.Raise(GlobalEvent.CLAIM_DAILYTASK);
        }
        else
        {
            // 关闭奖励后尝试弹出美金诱导弹窗
            if (Root.Instance.IsInduceReady)
            {
                MediatorRequest.Instance.TryPopInduceWindow();
                Root.Instance.IsInduceReady = false;
            }
        }
        
        // 点了获得奖励后才弹出补充充值
        if (Root.Instance.Role.AdditionalGiftNeedShow && 
            Root.Instance.Role.AdditionalGiftInfo != null)
        {
            Root.Instance.Role.AdditionalGiftNeedShow = false;
            UserInterfaceSystem.That.SingleTonQueue<UIAdditionalGift>();
        }

        // 非自然量用户 且 老玩家首胜后弹出权限框
        if (!Root.Instance.IsNaturalFlow &&
            TimeUtils.Instance.PassDay >= 1 && Root.Instance.IsNeedRequestNotification &&
            YZDataUtil.GetYZInt(YZConstUtil.YZDeniedNotifyPermission, 0) == 0)
        {
            TinyTimer.StartTimer(() =>
            {
                YZSDKsController.Shared.PromptForPush();
                Root.Instance.IsNeedRequestNotification = false;
            }, 1.5f);
        }
    }

    public override void InitBinds()
    {
    }

    public override void InitVm()
    {
        InitItems();
    }

    public override void InitEvents()
    {
    }

    void InitItemList()
    {
        itemList = new MyList
        {
            ScrollRect = scrollrect_Scroll_View,
            defaultItem = "common/rewardItem",
            itemRenderer = (index, obj) =>
            {
                var data = items[index];

                var iconImage = obj.FindChild<Image>("Icon");
                iconImage.sprite = data.GetIcon();

                if (data.name is "MagicBallPoint" or "MuseumPoint")
                {
                    iconImage.transform.localScale *= 0.8f;
                }
                
                //可以去掉小数点后无用的0
                obj.FindChild<Text>("Count").text = Math.Round(data.Count, 2).ToString();
            },
            ListLayout = ListLayOut.Horizontal
        };
    }

    public override void Close()
    {
        OnAnimationOut();
    }

    protected override void OnAnimationIn()
    {
        transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
        transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    protected override void OnAnimationOut()
    {
        transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
            .OnComplete(() => base.Close());
    }

    protected override void OnClose()
    {
        if (UIMain.IsFirstRoomGuideRunning)
        {
            EventDispatcher.Root.Raise(NewPlayerGuideStep.BEFORE_ENTER_ROOM.ToString());
        }

        closeCallback?.Invoke();
        closeCallback?.RemoveAllListeners();

        var uimain = UserInterfaceSystem.That.Get<UIMain>();
        if (uimain != null && uimain.IsVisible)
        {
            MediatorItem.Instance.ClearExShowItem();
        }
    }
}