using System;
using System.Collections.Generic;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using MyBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UI.Mono;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace UI
{
    [Obsolete]
    public class UIDailyMission : UIBase<UIDailyMission>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private MyButton infoBtn;
        [SerializeField] private MyButton closeBtn;

        [SerializeField] private MyText pointsText;
        [SerializeField] private MyButton middleBox;
        [SerializeField] private MyButton endBox;
        [SerializeField] private Slider slider;

        [SerializeField] private MyText timeText;

        [SerializeField] private DailyMissionMono missionMono;
        [SerializeField] private DailyMissionMono superMissionMono;

        [SerializeField] private GameObject middleBubble;
        [SerializeField] private GameObject endBubble;

        private DailyMission _configs;
        private int _todayMissionIndex = 1; // 今日第几个任务
        private int _missionListIndex = 0;
        private int _superMissionIndex = 0;

        private Mission _mission;
        private Mission _superMission;

        private bool _isNewPlayer = true;

        private bool _middleBoxCanClaim = false;
        private bool _endBoxCanClaim = false;

        private bool _isGetSecondRewards = false;
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_Daily_Mission, 
                (sender, eventArgs) => { RefreshData(); });

            AddEventListener(GlobalEvent.Pass_Day, (sender, eventArgs) =>
            {
                // 跨天重新请求
                MediatorRequest.Instance.GetDailyTaskInfo();
            });
            
            // 界面打开时，才接受：  同时触发普通任务和超级任务 
            AddEventListener(GlobalEvent.GetItems, (sender, eventArgs) =>
            {
                if (_isGetSecondRewards)
                    return;
                    
                string balance = YZDataUtil.GetLocaling(YZConstUtil.YZDailyMissionBalance, "");
            
                TinyTimer.StartTimer(() =>
                {
                    // 领取奖励
                    if (balance.NotNullOrEmpty())
                    {
                        _isGetSecondRewards = true;
                        superMissionMono.FlyReward(balance);
                        // 清空奖励
                        YZDataUtil.SetYZString(YZConstUtil.YZDailyMissionBalance, "");
                        YZDataUtil.SetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0);
                    }
                }, 0.5f, this);
                
                TinyTimer.StartTimer(() =>
                {
                    
                    // 刷新页面
                    MediatorRequest.Instance.GetDailyTaskInfo();
                    _isGetSecondRewards = false;
                }, 2.0f, this);
            });
        }

        public override void OnStart()
        {
            if (Root.Instance.IsNaturalFlow)
            {
                Close();
                return ;
            }

            if (Root.Instance.DailyMissionConfigs != null)
                _configs = Root.Instance.DailyMissionConfigs;
            else
            {
                YZDebug.LogError("每日任务没有拉取到配置");
                Close();
                return;
            }
            
            string type = GetArgsByIndex<bool>(0)? "0" : "1";
            YZFunnelUtil.SendYZEvent("dm_mainui_pop", new Dictionary<string, object>()
            {
                { "pop_type",  type}
            });

            if (type.Equals("1"))
            {
                // 领奖励，防止无奖励错误弹窗
                if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 0)
                {
                    Close();
                    return;
                }
            }

            middleBox.SetClick(OnClickMiddleBox);
            endBox.SetClick(OnClickEndBox);
            
            infoBtn.SetClick(() =>
            {
                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    Type = UIConfirmData.UIConfirmType.OneBtn,
                    // HideCloseBtn = true,
                    desc = I18N.Get("key_daily_mission_info"),
                    confirmTitle = I18N.Get("key_ok"),
                    AligmentType = TextAnchor.MiddleLeft,
                    Rect2D = new Vector2(650, 650),
                    // Position = new Vector2(0, 15),
                });
            });
            
            closeBtn.SetClick(OnCloseBtnClick);

            RefreshData();

            if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1)
            {
                MediatorRequest.Instance.ClaimDailyTaskOnOpen();
            }
        }

        private void HandleBalnce()
        {
            //string _testBalance = "{\"money\": 0,\"chips\": 800,\"bonus\": 1,\"coin\": 10500}";
            // missionMono.FlyReward(_testBalance);

            string balance = YZDataUtil.GetLocaling(YZConstUtil.YZDailyMissionBalance, "");

            // 只有普通任务奖励
            if (Root.Instance.dailyTaskInfo.missonCompleted == 1 
                && Root.Instance.dailyTaskInfo.superMissionCompleted == 0)
            {
                TinyTimer.StartTimer(() =>
                {
                    // 领取奖励
                    if (balance.NotNullOrEmpty())
                    {
                        missionMono.FlyReward(balance);
                        // 清空奖励
                        YZDataUtil.SetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0);
                        YZDataUtil.SetYZString(YZConstUtil.YZDailyMissionBalance, "");
                    }
                }, 0.5f, this);
            } 
            // 只有超级任务奖励
            else if (Root.Instance.dailyTaskInfo.missonCompleted == 0
                     && Root.Instance.dailyTaskInfo.superMissionCompleted == 1)
            {
                TinyTimer.StartTimer(() =>
                {
                    // 领取奖励
                    if (balance.NotNullOrEmpty())
                    {
                        superMissionMono.FlyReward(balance);
                        // 清空奖励
                        YZDataUtil.SetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0);
                        YZDataUtil.SetYZString(YZConstUtil.YZDailyMissionBalance, "");
                    }
                }, 0.5f, this);
            }
            // 同时有两个任务奖励
            else if (Root.Instance.dailyTaskInfo.missonCompleted == 1
                     && Root.Instance.dailyTaskInfo.superMissionCompleted == 1)
            {
                TinyTimer.StartTimer(() =>
                {
                    // 领取奖励
                    if (balance.NotNullOrEmpty())
                    {
                        var dic = JsonConvert.DeserializeObject<Dictionary<string, float>>(balance);
                        // 拆分奖励，balance分为普通和超级两个部分
                        // balance 减去超级的部分，计算剩余的
                        var missionReward = _superMission.reward;
                        JObject jsonReward = new JObject
                        {
                            ["money"] = dic["money"],
                            ["bonus"] = dic["bonus"] - missionReward.bonus,
                            ["chips"] = dic["chips"] - missionReward.chips,
                            ["coin"] = dic["coin"] - missionReward.coin
                        };
                        missionMono.FlyReward(jsonReward.ToString());

                        //string str = json.ToString();
                        // YZDebug.Log("json = " + str);

                    }
                }, 0.5f, this);
            }

            if (balance.NotNullOrEmpty())
            {
                if (!(Root.Instance.dailyTaskInfo.missonCompleted == 1
                      && Root.Instance.dailyTaskInfo.superMissionCompleted == 1))
                {
                    TinyTimer.StartTimer(() =>
                    {
                        // 刷新页面
                        MediatorRequest.Instance.GetDailyTaskInfo();
                    }, 2.0f, this);
                }
            }
        }

        private void InitGifts(GameObject bubble, int giftIndex)
        {
            string playerKey = _isNewPlayer ? "new_player" : "player";
            
            
            if (_configs.total_reward != null && _configs.total_reward.Count == 2)
            { 
                var rewardData = _configs.total_reward[playerKey][giftIndex];
                // 设置礼包状态
                
                // 第一个礼包 
                if (Root.Instance.dailyTaskInfo.total_claim == 0
                    && Root.Instance.dailyTaskInfo.point >= rewardData.point
                    && giftIndex == 0)
                {
                    _middleBoxCanClaim = true;
                    middleBox.gameObject.transform.DOShakeRotation(1.0f, 20f, 40,
                        90f, true).SetLoops(-1, LoopType.Restart);
                    middleBox.gameObject.transform.DOScale(1.2f, 1.0f).SetLoops(-1, LoopType.Yoyo);
                }

                // 第二个礼包
                if (Root.Instance.dailyTaskInfo.total_claim <= 1 
                    && Root.Instance.dailyTaskInfo.point >= rewardData.point
                    && giftIndex == 1)
                {   
                    _endBoxCanClaim = true;
                    endBox.gameObject.transform.DOShakeRotation(1.0f, 20f, 40, 
                        90f, true).SetLoops(-1, LoopType.Restart);
                    endBox.gameObject.transform.DOScale(1.2f, 1.0f).SetLoops(-1, LoopType.Yoyo);
                }

                if (Root.Instance.dailyTaskInfo.total_claim == 1 || Root.Instance.dailyTaskInfo.total_claim > 2)
                {
                    middleBox.gameObject.SetActive(false);
                    middleBox.transform.parent.Find("Gray").SetActive(true);
                }

                if (Root.Instance.dailyTaskInfo.total_claim >= 2)
                {
                    endBox.gameObject.SetActive(false);
                    endBox.transform.parent.Find("Gray").SetActive(true);
                }

                // 设置气泡
               
                var rewardPrefab = bubble.transform.Find("Reward").gameObject;
                
                if (rewardData.reward.bonus > 0)
                {
                    var itemNewBonus = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                        rewardPrefab.transform.parent);

                    SetItem(itemNewBonus, 1, rewardData.reward.bonus);
                }
                
                if (rewardData.reward.coin > 0)
                {
                    var itemNewBonus = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                        rewardPrefab.transform.parent);

                    SetItem(itemNewBonus, 4, rewardData.reward.coin);
                }
                    
                if (rewardData.reward.chips > 0)
                {
                    var itemNewBonus = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                        rewardPrefab.transform.parent);

                    SetItem(itemNewBonus, 2, (int)rewardData.reward.chips);
                }
                
                rewardPrefab.SetActive(false);
            }
        }

        private void OnClickMiddleBox()
        {
            if (!_middleBoxCanClaim)
            {
                bool isActive = middleBubble.activeSelf;
                middleBubble.SetActive(!isActive);
                if (!isActive)
                    endBubble.SetActive(false);
            }
            else
            {
                MediatorRequest.Instance.ClaimDailyTaskTotal(1);
                middleBox.gameObject.SetActive(false);
                middleBox.transform.parent.Find("Gray").SetActive(true);
            }
        }

        private void OnClickEndBox()
        {
            if (!_endBoxCanClaim)
            {
                bool isActive = endBubble.activeSelf;
                if (!isActive)
                    middleBubble.SetActive(false);
                endBubble.SetActive(!isActive);
            }
            else
            {
                MediatorRequest.Instance.ClaimDailyTaskTotal(2);
                endBox.gameObject.SetActive(false);
                endBox.transform.parent.Find("Gray").SetActive(true);
            }
        }

        private void RefreshMission()
        {
            if (_configs.daily_task_list != null
                && _configs.daily_task_list.ContainsKey(_todayMissionIndex.ToString())
                && _configs.daily_task_list[_todayMissionIndex.ToString()] != null)
                _mission = _configs.daily_task_list[_todayMissionIndex.ToString()][_missionListIndex];
            
            missionMono.Init(false, _mission, _todayMissionIndex);
        }

        private void RefreshSuperMission()
        {
            if (_configs.super_task_list != null)
                _superMission = _configs.super_task_list[_superMissionIndex];

            superMissionMono.Init(true, _superMission, _superMissionIndex);
        }

        private void RefreshData()
        {
            if (Root.Instance.dailyTaskInfo != null)
            {
                _todayMissionIndex = Root.Instance.dailyTaskInfo.complete + 2; 
                // complete 取值 -1 0 1 2 3   _todayMissionIndex 对应 1 2 3 4 5

                if (_todayMissionIndex != 5)
                    _missionListIndex = Root.Instance.dailyTaskInfo.list[_todayMissionIndex - 1] - 1;
                else
                    _missionListIndex = 0;

                _superMissionIndex = Root.Instance.dailyTaskInfo.supper - 1;

                // 显示两种任务
                RefreshMission();
                RefreshSuperMission();

                _isNewPlayer = Root.Instance.dailyTaskInfo.total_order == 0;
                
                // 设置礼包状态 气泡状态
                InitGifts(middleBubble, 0);
                InitGifts(endBubble, 1);
                
                string playerKey = _isNewPlayer ? "new_player" : "player";
                // 显示 point progress
                float progress = Root.Instance.dailyTaskInfo.point /
                                 Root.Instance.DailyMissionConfigs.total_reward[playerKey][1].point;
                //slider.value = progress;
                
                float fromValue = PlayerPrefs.GetFloat(Root.Instance.Role.user_id + "DailyMissionSlider", 0);
                DOTween.To(() => fromValue, x => slider.value = x, progress, 0.5f);

                int point =
                    Root.Instance.dailyTaskInfo.point >
                    Root.Instance.DailyMissionConfigs.total_reward[playerKey][1].point
                        ? (int)Root.Instance.DailyMissionConfigs.total_reward[playerKey][1].point
                        : Root.Instance.dailyTaskInfo.point;
                pointsText.text =
                    $"{point}/" +
                    $"{(int)Root.Instance.DailyMissionConfigs.total_reward[playerKey][1].point}";
                
                PlayerPrefs.SetFloat(Root.Instance.Role.user_id + "DailyMissionSlider", progress);
                
                HandleBalnce();
            }
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        private void Update()
        {
            // 获取当前时间
            var dto = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.Instance.UtcTimeNow - 8 * 3600);
            DateTime now = dto.DateTime;
            
            // 获取当前星期几（7表示周日，1表示周一，依次类推）
            int currentDayOfWeek = Root.Instance.dailyTaskInfo.today_week;

            // 计算距离下一个周天的天数差
            int daysUntilNextSunday = currentDayOfWeek == 7 ? 1 : 8 - currentDayOfWeek;

            // 获取下一个周天凌晨的时间
            DateTime nextSundayMidnight = now.Date.AddDays(daysUntilNextSunday);

            // 计算距离下一个周天凌晨的时间差
            TimeSpan timeUntilNextSundayMidnight = nextSundayMidnight - now;
            
            int days = timeUntilNextSundayMidnight.Days;
            if (days >= 1)
                timeText.text = days + " Days Left";
            else
            {
                timeText.text =
                    TimeUtils.Instance.ToHourMinuteSecond((int) timeUntilNextSundayMidnight.TotalSeconds);
            }

            if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1)
            {
                // 有奖励不能点击关闭按钮
                closeBtn.interactable = false;
            }
            else
            {
                closeBtn.interactable = true;
            }
        }
        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => {  });
        }

        protected override void OnAnimationOut()
        {
            //transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic).SetId(UIName);
        }

        private void SetItem(GameObject itemObj, int type, float count)
        {
            itemObj.transform.Find("Icon").Find("1").SetActive(false);
            itemObj.transform.Find("Icon").Find("2").SetActive(false);
            itemObj.transform.Find("Icon").Find("3").SetActive(false);
            itemObj.transform.Find("Icon").Find("4").SetActive(false);

            itemObj.transform.Find("CountText").Find("1").SetActive(false);
            itemObj.transform.Find("CountText").Find("2").SetActive(false);
            itemObj.transform.Find("CountText").Find("3").SetActive(false);
            itemObj.transform.Find("CountText").Find("4").SetActive(false);

            switch (type)
            {
                case 1:
                    itemObj.transform.Find("Icon").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").GetComponent<MyText>().text = "$" +
                        YZNumberUtil.FormatYZMoney(count.ToString());

                    break;

                case 2:
                    itemObj.transform.Find("Icon").Find("2").SetActive(true);
                    itemObj.transform.Find("CountText").Find("2").SetActive(true);

                    itemObj.transform.Find("CountText").Find("2").GetComponent<MyText>().text =
                        YZNumberUtil.FormatYZMoney(count.ToString());

                    break;

                case 3:
                    itemObj.transform.Find("Icon").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").GetComponent<MyText>().text = "$" +
                        YZNumberUtil.FormatYZMoney(count.ToString());


                    break;

                case 4:
                    itemObj.transform.Find("Icon").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").GetComponent<MyText>().text =
                        YZNumberUtil.FormatYZMoney(count.ToString());

                    break;
            }
        }
        
    }
}