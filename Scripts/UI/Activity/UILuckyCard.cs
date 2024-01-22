using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Internal;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using Reactive.Bindings;
using UnityEngine;
using Utils;
using Text = UnityEngine.UI.Text;

namespace UI.Activity
{
    public class UILuckyCard : UIBase<UILuckyCard>
    {

        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private MyButton closeBtn;
        [SerializeField] private MyButton helpBtn;
        [SerializeField] private Text countdownText;
        [SerializeField] private Text remainText;
        [SerializeField] private List<CardMono> cardList; 
        
        private float _countdownTimer;
        private List<LuckyCardConfig> _configList;

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_LuckyCard,
                (sender, args) =>
                {
                    vm[vname.luckyCardInfo.ToString()].ToIObservable<LuckyCardInfo>().Value = 
                        Root.Instance.Role.luckyCardInfo;
                });
            
            AddEventListener(GlobalEvent.CHARGE_SUCCESS,
                (sender, args) =>
                {
                    // 判断一下 id 是不是属于幸运卡的
                    int id = int.Parse((string)sender);
                    var matchedConfig = _configList.Find(match: config => config.id == id);
                    if (matchedConfig != null)
                    {
                        chargeId = id;
                        Close();
                    }
                });
        }

        enum vname
        {
            luckyCardInfo,
            chooseList
        }

        private int chargeId;
        private ActivityEnterType activityEnterTYpe;
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.LuckyCard
                , charge_id: chargeId
                , isauto: activityEnterTYpe.IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        public override void OnStart()
        {
            var entryType = GetArgsByIndex<ActivityEnterType>(0);
            activityEnterTYpe = entryType;
            MediatorActivity.Instance.AddPopCount(ActivityType.LuckyCard, entryType);
            // 更新一下信息
            MediatorRequest.Instance.UpdateLuckyCardInfo();

            var level = Root.Instance.Role.luckyCardInfo.lucky_card_level;
            _configList = Root.Instance.LuckyCardConfigs[level];
            
            RefreshRemain().Invoke();
            closeBtn.SetClick(OnCloseBtnClick);
        }

        public override void InitVm()
        {
            vm[vname.luckyCardInfo.ToString()] = new ReactivePropertySlim<LuckyCardInfo>
                (Root.Instance.Role.luckyCardInfo);
            vm[vname.chooseList.ToString()] =
                new ReactivePropertySlim<int[]>(Root.Instance.Role.luckyCardInfo.lucky_card_choose_list);
        }

        public override void InitBinds()
        {
            vm[vname.chooseList.ToString()].ToIObservable<int[]>().Subscribe(value =>
            {
                // 已翻开的翻牌状态
                for (int i = 0; i < 4; ++i)
                {
                    bool opened = value[i] > 0;
                    if (opened)
                    {
                        var i1 = i;
                        var config = _configList.Find(match: cardConfig => cardConfig.id == value[i1]);

                        //YZDataUtil.GetYZInt(YZConstUtil.YZLuckyCardOpenState + i, 0) == 1;
                        cardList[i].InitCard(true, config.amount.ToFloat(), config.amount.ToFloat()
                            , config.show_bonus.ToFloat(), i, RefreshRemain(), value[i]);
                    }
                }

                // 盖住的牌
                for (int i = 0; i < 4; ++i)
                {
                    if (value[i] == 0)
                    {
                        cardList[i].InitCard(false, 0, 0
                            , 0, i, RefreshRemain(), 0);
                    }
                }
            });
            vm[vname.luckyCardInfo.ToString()].ToIObservable<LuckyCardInfo>().Subscribe(value =>
            {
                var chooseList = value.lucky_card_choose_list;
                vm[vname.chooseList.ToString()].ToIObservable<int[]>().Value = chooseList;
            });
        }
        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => {  });
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                .SetId(UIName);
        }

        Action RefreshRemain()
        {
            return () =>
            {
                //int remainCount = YZDataUtil.GetYZInt(YZConstUtil.YZLuckyCardRemainCount, 3);
                int remainCount = Root.Instance.Role.luckyCardInfo.lucky_card_choose_list.
                    FindAll(id => id == 0).Length - 1;
                remainText.text = YZString.Format(I18N.Get("key_lucky_card_remain"), remainCount.ToString());
            };
        }

        private void Update()
        {
            //ActivityManager.Shared.GetYZActivityTime(Root.Instance.Role.luckyCardInfo)
            
            var lessTime = Root.Instance.Role.luckyCardInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
            countdownText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);
            
            if (lessTime < 0)
            {
                Close();
            }
        }
    }
}