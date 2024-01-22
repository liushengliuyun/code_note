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
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Activity
{
    public class UIStartPack : UIBase<UIStartPack>
    {
        public Text TimeText;
        public Transform[] packs;
        public Button CloseBtn;

        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {
            CloseBtn.onClick.AddListener(OnCloseBtnClick);
            
            SetTimeLess();
            //注册倒计时
            RegisterInterval(1f, () => { SetTimeLess(); });

            var entryType = GetOpenType();
            
            MediatorActivity.Instance.AddPopCount(ActivityType.StartPacker, entryType);

            MediatorUnlock.Instance.RecordShowUI(ClassType);
        }

        private ActivityEnterType GetOpenType()
        {
            return GetArgsByIndex<ActivityEnterType>(0);
        }

        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.StartPacker 
                ,isauto: GetOpenType().IsAutoPop()
                ,duration: (int)duration
                , outtype: closeByBtn ? 1: 2
                , switch_click : SwitchClick
            );
        }

        private void SetTimeLess()
        {
            var activityLessTime = MediatorActivity.Instance.GetActivityLessTime(ActivityType.StartPacker);
            TimeText.text = TimeUtils.Instance.ToHourMinuteSecond(activityLessTime);

            if (activityLessTime <= 0)
            {
                Close();
            }
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
            vm[vname.gear.ToString()] = new ReactivePropertySlim<int>(Root.Instance.StarterPackInfo.starter_pack_level);
        }

        public override void InitBinds()
        {
            vm[vname.gear.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                Root.Instance.StarterPackConfig.TryGetValue(value, out var config);
                if (config == null)
                {
                    Close();
                }


                for (int i = 0; i < packs.Length; i++)
                {
                    //礼包价格
                    var obj = packs[i].gameObject;

                    var buyButton = obj.FindChild<MyButton>("buyButton");

                    var data = config[i];
                    buyButton.title = I18N.Get("key_starterpacker_price", data.amount);

                    buyButton.SetClick(() =>
                    {
                        MediatorRequest.Instance.Charge(data, ActivityType.StartPacker);
                    });

                    //顶部礼包
                    if (i == 2)
                    {
                        string moreSpriteName = null;
                        if (config[i].MoreValue < 130)
                        {
                            moreSpriteName = "120";
                        }
                        else if (config[i].MoreValue > 150)
                        {
                            moreSpriteName = "200";
                        }
                        else
                        {
                            moreSpriteName = "130";
                        }

                        obj.FindChild<Image>("word").sprite =
                            MediatorBingo.Instance.GetSpriteByUrl($"{UIName}/{moreSpriteName}");
                    }
                    else
                    {
                        obj.FindChild<Text>("mark group/Text mark").text = data.MoreValue + "%";
                    }

                    obj.FindChild<Text>("data/cash group/cash text").text =
                        I18N.Get("key_money_count", data.amount);
                    obj.FindChild<Text>("data/bonus group/bonusText").text =
                        I18N.Get("key_money_count", data.ShowBonus);
                    obj.FindChild<Text>("data/coin group/coinText").text = data.ShowGems.ToString();
                }
            });
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.SYNC_STARTERPACKINFO, (sender, eventArgs) =>
            {
                if (Root.Instance.StarterPackInfo is not { CanBuy: true } )
                {
                    Close();
                }
            });
        }
    }
}