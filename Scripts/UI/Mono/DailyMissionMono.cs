using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Util;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Mono
{
    public class DailyMissionMono : MonoBehaviour
    {

        [SerializeField] private MyText titleText;
        [SerializeField] private MyText timeText;
        [SerializeField] private MyText descText;

        [SerializeField] private MyText pointsText;
        
        [SerializeField] private GameObject countObj;
        [SerializeField] private MyText countText; // 右上角任务序号
        
        [SerializeField] private MyButton refreshBtn;
        [SerializeField] private MyText refreshCountText;
        
        [SerializeField] private GameObject sliderPanel;
        [SerializeField] private Slider slier;
        [SerializeField] private Text slierText;
        [SerializeField] private MyButton rewardBtn;
        
        [SerializeField] private GameObject rewardsPanel;
        [SerializeField] private GameObject rewardPrefab;

        [SerializeField] private GameObject notCompletePanel;
        [SerializeField] private GameObject allCompletePanel;
        [SerializeField] private GameObject refreshTimeText;

        [SerializeField] private MyButton showRewardBtn;
        [SerializeField] private MyButton hideRewardBtn;
        
        // 刷新超级礼包用到
        [SerializeField] private GameObject diamondObj;

        private bool _isSuperMission;

        private Mission _mission;

        private List<GameObject> rewardList = null;

        private float _refreshTimer = 4;
        private const float _refreshTime = 3f;

        private bool _todayAutoRefreshed = false;

        private bool _isFlying = false;
        
        
        public void Init(bool isSuperMission, Mission missionData, int todayMissionIndex)
        {
            _isSuperMission = isSuperMission;
            if (_isSuperMission)
            {
                countObj.SetActive(false);
                refreshBtn.SetActive(true);

                titleText.text = "Super Mission";
            }
            else
            {
                countObj.SetActive(true);
                refreshBtn.SetActive(false);
                
                titleText.text = "Mission " + todayMissionIndex;

                countText.text = $"{todayMissionIndex}/4";
                
                if (todayMissionIndex > 4)
                {
                    // 所有都已完成
                    notCompletePanel.SetActive(false);
                    allCompletePanel.SetActive(true);

                    return;
                }
                else if (!isSuperMission)
                {
                
                    notCompletePanel.SetActive(true);
                    allCompletePanel.SetActive(false);
                }
            }

            _mission = missionData;

            pointsText.text = ((int)missionData.point).ToString();

            var localParams = missionData.condition.Split(",");
            for (int i = 0; i < localParams.Length; ++i)
            {
                if (localParams[i].Contains(".0000"))
                {
                    localParams[i] = localParams[i].Replace(".0000", "");
                    continue;
                }
                if (localParams[i].Contains(".00"))
                {
                    localParams[i] = localParams[i].Replace(".00", "");
                    continue;
                }
                if (localParams[i].Contains(".0"))
                {
                    localParams[i] = localParams[i].Replace(".0", "");
                }
            }
            var localization = $"key_mission_{missionData.type}";
            descText.text = I18N.Get(localization, localParams);

            float missionProgress = isSuperMission ? Root.Instance.dailyTaskInfo.supper_progress : Root.Instance.dailyTaskInfo.progress;
            // 显示 progress
            float progress = 0f;
            if (_isSuperMission)
            {
                progress =  (float) missionProgress / (float) Root.Instance.dailyTaskInfo.supper_target;
            }
            else
            {
                progress =  (float) missionProgress / (float) Root.Instance.dailyTaskInfo.task_target;
            }

            string missionType = _isSuperMission ? "super" : "ordinary";
            float sliderFromValue = PlayerPrefs.GetFloat(Root.Instance.Role.user_id + missionType + "MissionSlider", 0);
            
            
            if (_isSuperMission)
            {
                if (Root.Instance.dailyTaskInfo.superMissionCompleted == 0)
                {
                    //slier.value = progress;
                    DOTween.To(() => sliderFromValue, x => slier.value = x, progress, 0.5f);
                    PlayerPrefs.SetFloat(Root.Instance.Role.user_id + missionType + "MissionSlider", progress);
                    
                    //slierText.text = $"{progress * 100:F}%";
                }
                else
                {
                    //slier.value = 1;
                    DOTween.To(() => sliderFromValue, x => slier.value = x, 1, 0.5f);
                    PlayerPrefs.SetFloat(Root.Instance.Role.user_id + missionType + "MissionSlider", 1);
                    
                    //slierText.text = "100.00%";
                    
                }
            }
            else
            {
                if (Root.Instance.dailyTaskInfo.missonCompleted == 0)
                {
                    DOTween.To(() => sliderFromValue, x => slier.value = x, progress, 0.5f);
                    PlayerPrefs.SetFloat(Root.Instance.Role.user_id + missionType + "MissionSlider", progress);
                    
                    //slierText.text = $"{progress * 100:F}%";
                }
                else
                {
                    //slier.value = 1;
                    DOTween.To(() => sliderFromValue, x => slier.value = x, 1, 0.5f);
                    PlayerPrefs.SetFloat(Root.Instance.Role.user_id + missionType + "MissionSlider", 1);
                    
                    //slierText.text = "100.00%";
                    
                }
            }

            // 显示奖励道具
            rewardPrefab.SetActive(true);
            rewardList ??= new List<GameObject>();
            if (rewardList.Count > 0)
            {
                // 销毁原先的奖励
                for (int i = rewardList.Count - 1; i >= 0; --i)
                    Destroy(rewardList[i]);
                
                rewardList.Clear();
            }

            if (missionData.reward.bonus > 0)
            {
                var itemNewBonus = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                    rewardPrefab.transform.parent);

                SetItem(itemNewBonus, 1, missionData.reward.bonus);
                itemNewBonus.transform.localPosition.SetZ(1);
                rewardList.Add(itemNewBonus);
            }
            
            if (missionData.reward.coin > 0)
            {
                var itemNewCoin = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                    rewardPrefab.transform.parent);

                SetItem(itemNewCoin, 4, missionData.reward.coin);
                itemNewCoin.transform.localPosition.SetZ(1);
                rewardList.Add(itemNewCoin);
            }

            if (missionData.reward.chips > 0)
            {
                var itemNewChips = Instantiate(rewardPrefab, Vector3.zero, Quaternion.identity, 
                    rewardPrefab.transform.parent);

                SetItem(itemNewChips, 2, (int)missionData.reward.chips);
                itemNewChips.transform.localPosition.SetZ(1);
                rewardList.Add(itemNewChips);
            }
            
            rewardPrefab.SetActive(false);
            
            
            rewardBtn.SetClick(() =>
            {
                rewardsPanel.SetActive(true);
                sliderPanel.SetActive(false);
            });
            
            showRewardBtn.SetClick(() =>
            {
                rewardsPanel.SetActive(true);
                sliderPanel.SetActive(false);
            });
            
            hideRewardBtn.SetClick(() =>
            {
                rewardsPanel.SetActive(false);
                sliderPanel.SetActive(true);
            });

            // 默认显示slider
            rewardsPanel.SetActive(false);
            sliderPanel.SetActive(true);

            // 超级任务刷新次数
            if (_isSuperMission)
            {
                if (Root.Instance.dailyTaskInfo.supper_flush > 0)
                {
                    diamondObj.SetActive(false);
                    refreshCountText.gameObject.SetActive(true);
                }
                else
                {
                    diamondObj.SetActive(true);
                    refreshCountText.gameObject.SetActive(false);
                }
                
                refreshBtn.SetClick(() =>
                {
                    // 开始计时
                    _refreshTimer = 0;
                    if (Root.Instance.dailyTaskInfo.supper_flush > 0)
                    {
                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                        {
                            Type = UIConfirmData.UIConfirmType.TwoBtn,
                            // HideCloseBtn = true,
                            desc = I18N.Get("key_daily_mission_refresh"),
                            confirmTitle = I18N.Get("key_ok"),
                            AligmentType = TextAnchor.MiddleCenter,
                            Rect2D = new Vector2(650, 650),
                            confirmCall = () => { MediatorRequest.Instance.RefreshSuperDailyTask(2); },
                            // Position = new Vector2(0, 15),
                        });
                    }
                    else
                    {
                        if (Root.Instance.Role.GetItemCount(2) >= 50)
                        {
                            UserInterfaceSystem.That.ShowUI<UIDailyMissionDiamond>();
                        }
                        else
                        {
                            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                            {
                                Type = UIConfirmData.UIConfirmType.OneBtn,
                                desc = I18N.Get("RES_NOT_ENOUGH"),
                                confirmTitle = I18N.Get("key_ok"),
                                AligmentType = TextAnchor.MiddleCenter,
                            });
                        }
                    }
                });
            }

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

        public void FlyReward(string balance)
        {
            var uiDailyMission = UserInterfaceSystem.That.Get<UIDailyMission>();
            if (uiDailyMission == null)
            {
                return;
            }

            if (_isFlying)
                return;

            _isFlying = true;
            
            var ui = uiDailyMission.gameObject;
            ui.transform.Find("RewardMask").SetActive(true);
            var rewardCopy = Instantiate(rewardBtn, Vector3.zero, Quaternion.identity, ui.transform);
            rewardCopy.transform.position = rewardBtn.transform.position + new Vector3(0,0,5f);

            Tween t1 = rewardCopy.transform.DOMove(new Vector3(0, 0, 5f), 0.5f).SetEase(Ease.InOutExpo);
            Tween t2 = rewardCopy.transform.DOScale(Vector3.one * 2.5f, 0.5f).SetEase(Ease.InOutExpo);
            Tween t3 = rewardCopy.transform.DOMoveZ(5, 0.4f);
            var seq = DOTween.Sequence();
            seq.Append(t1);
            seq.Join(t2);
            seq.Append(t3);
            seq.Play().onComplete = () =>
            {
                MediatorRequest.Instance.SyncItem(balance);
                ui.transform.Find("RewardMask").SetActive(false);
                rewardCopy.SetActive(false);

                _isFlying = false;
            };

            if (_isSuperMission)
            {
                // 清空超级任务进度
                Root.Instance.dailyTaskInfo.superMissionCompleted = 0;
            }
            else
            {
                Root.Instance.dailyTaskInfo.missonCompleted = 0;
            }

        }

        private void Update()
        {
            if (Root.Instance.dailyTaskInfo == null)
            {
                CarbonLogger.LogError("Root.Instance.dailyTaskInfo 为空");
            }
            
            if (!_isSuperMission)
            {
                timeText.text = TimeUtils.Instance.FormatTimeToTomorrow();
                if (refreshTimeText != null)
                    refreshTimeText.GetComponent<MyText>().text = TimeUtils.Instance.FormatTimeToTomorrow();
            }
            else if (Root.Instance.dailyTaskInfo != null)
            {
                int beginTime = Root.Instance.dailyTaskInfo.daily_task_supper_begin_time;
                int hours = 48;
                if (_mission == null)
                {
                    timeText.text = TimeUtils.Instance.ToHourMinuteSecond(
                        beginTime + hours * 3600 - TimeUtils.Instance.UtcTimeNow);
                }
                else
                {
                    var seconds = beginTime + (int)_mission.time * 3600 - TimeUtils.Instance.UtcTimeNow;
                    if (seconds < 0)
                    {
                        if (!_todayAutoRefreshed)
                        {
                            MediatorRequest.Instance.RefreshSuperDailyTask(1);
                            _todayAutoRefreshed = true;
                        }
                        return;
                    }

                    timeText.text = TimeUtils.Instance.ToHourMinuteSecond(
                        beginTime + (int)_mission.time * 3600 - TimeUtils.Instance.UtcTimeNow);
                }

            }
            
            
            if (_isSuperMission)
            {
                if (_refreshTimer > _refreshTime)
                {
                    refreshBtn.interactable = true;
                }
                else
                {
                    refreshBtn.interactable = false;
                }

                _refreshTimer += Time.deltaTime;
            }

            slierText.text = $"{slier.value * 100:F}%";
        }
    }
}