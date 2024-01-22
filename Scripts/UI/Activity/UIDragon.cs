using System;
using System.Collections.Generic;
using System.Text;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
namespace UI.Activity
{
    public class UIDragon: UIBase<UIDragon>
    {
        public override UIType uiType { get; set; } = UIType.Window;

        [SerializeField] private Text countdownText;
        [SerializeField] private MyButton closeBtn;
        [SerializeField] private DragonItemMono dragonItemTemplate;
        
        private int _currentDragonCount;
        private int _dragonCountMax;
        private List<DragonItemMono> _dragonItemsList;

        private int _currentBuyId = 0;
        private int _start = 0;

        // private int chargeId;
        private ActivityEnterType activityEnterTYpe;
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.Dragon
                // , charge_id: chargeId
                , isauto: activityEnterTYpe.IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        public override void OnStart()
        {
            // 更新信息
            MediatorRequest.Instance.UpdateDragonInfo();

            var entryType = GetArgsByIndex<ActivityEnterType>(0);
            activityEnterTYpe = entryType;
            MediatorActivity.Instance.AddPopCount(ActivityType.Dragon, entryType);

            _dragonItemsList = new List<DragonItemMono>();

            var level = Root.Instance.Role.dragonInfo.one_stop_level;
            Root.Instance.DragonConfig.one_stop_level_list.TryGetValue(level.ToString(), out var itemsDic);

            if (itemsDic == null || itemsDic.Count == 0)
            {
                YZDebug.LogError("DragonConfig items Error!");
                Close();
                return;
            }
            
            _dragonCountMax = itemsDic.Count;

            _start = Root.Instance.Role.dragonInfo.one_stop_claimed;

            _currentDragonCount = _dragonCountMax - _start;

            var pos = dragonItemTemplate.GetComponent<RectTransform>().localPosition;
            for (int i = _start; i < _dragonCountMax; ++i)
            {
                var subId = YZNumberUtil.FormatYZMoney(itemsDic[(i + 1).ToString()][0].weight);
                var chargeInfo = Root.Instance.DragonConfig.one_stop_charge_list.Find
                    (match: charge => charge.sub_id.Equals(subId));

                var dragonItemNew = Instantiate(dragonItemTemplate, Vector3.zero,
                    Quaternion.identity, dragonItemTemplate.transform.parent);

                dragonItemNew.GetComponent<RectTransform>().localPosition =
                    pos + (i - _start) * new Vector3(0, -300f, 0);

                bool isFree = chargeInfo == null;

                dragonItemNew.GetComponent<DragonItemMono>().InitDragon(itemsDic[(i + 1).ToString()], isFree,
                    isFree ? "0" : chargeInfo.amount,
                    isFree ? null : chargeInfo,
                    i + 1);

                _dragonItemsList.Add(dragonItemNew);
            }

            dragonItemTemplate.SetActive(false);

            if (_dragonItemsList.Count > 0)
            {
                _dragonItemsList[0].Unlock(false);
                _currentBuyId = _dragonItemsList[0].CurrentBuyId;

            }

            closeBtn.SetClick(() =>
            {
                MediatorRequest.Instance.UpdateDragonInfo();
                Close();
            });

            MediatorUnlock.Instance.RecordShowUI(ClassType);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        public override void InitEvents()
        {
            // 充值购买成功
            AddEventListener(GlobalEvent.CHARGE_SUCCESS,
                (sender, args) =>
                {
                    // 判断一下 id 是不是一条龙的
                    int chargeId = int.Parse((string)sender);
                    if (_currentBuyId == chargeId)
                    {
                        // 发送领取
                        _dragonItemsList[0].GetRewards();
                    }
                });
            
            AddEventListener(GlobalEvent.DragonGetSuccess, (sender, eventArgs) =>
            {
                // 领取成功，播放消失动画
                if (_dragonItemsList.Count > 1)
                    _dragonItemsList[0].Fade();
                else
                {
                    TinyTimer.StartTimer(Close, 0.5f);
                }
            });
            
            AddEventListener(GlobalEvent.Dragon_Fade_Completed, (sender, eventArgs) =>
            {
                // 删除第一个 DragonItem
                Destroy(_dragonItemsList[0]);
                _dragonItemsList.RemoveAt(0);
                _currentDragonCount--;

                if (_currentDragonCount >= 1)
                {
                    // 其他的 Item向上挪
                    for (int i = 0; i < _currentDragonCount; ++i)
                    {
                        var y = _dragonItemsList[i].GetComponent<RectTransform>().localPosition.y;
                        _dragonItemsList[i].GetComponent<RectTransform>().DOLocalMoveY(y + 300f, 0.8f);
                    }
                }

                if (_currentDragonCount > 0)
                {
                    // 解锁第一个
                    _dragonItemsList[0].Unlock(true);
                    _currentBuyId = _dragonItemsList[0].CurrentBuyId;
                }
            });
        }
        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic).SetId(UIName);
        }

        private void Update()
        {
            var lessTime = Root.Instance.Role.dragonInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
            countdownText.text = TimeUtils.Instance.ToHourMinuteSecond(lessTime);

            if (lessTime < 0)
            {
                Close();
            }
        }
    }
}