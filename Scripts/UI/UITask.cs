using System;
using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.AnimationsSequencer;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Services.UserInterfaceService.UIExtensions.Scripts.Utilities;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using UI.Mono;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;

namespace UI
{
    public class UITask : UIBase<UITask>
    {
        #region UI Variable Statement

        [SerializeField] private ScrollRect HorizontalScrollRect;

        [SerializeField] private HorizontalScrollSnap horizontalScrollSnap;
        [SerializeField] private Transform[] pages;

        // [SerializeField] private MyButton ChooseBtn;
        [SerializeField] private MyButton[] CloseBtns;
        [SerializeField] private MyButton[] AboutBtns;

        [SerializeField] private MyButton preBtn;
        [SerializeField] private MyButton nextBtn;

        #endregion

        /// <summary>
        ///  第一次进入时, 显示的难度等级
        /// </summary>
        private int enterShowLevel;

        private int hideTipComTime = 5;

        private const int easyLevel = 1;
        private const int normalLevel = 2;
        private const int hardLevel = 3;

        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.TaskSystem
                // , charge_id: chargeId
                , isauto: GetPopType() is 1 or 2
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }

        /// <summary>
        ///  0玩家主动点击弹出；1：登录时弹出；2：对局结束后弹出
        /// </summary>
        private int GetPopType()
        {
            var table = GetArgsByIndex<GameData>(0);
            if (table?["pop_type"] != null)
            {
                var pop_type= (int)table["pop_type"];
                return pop_type;
            }

            return -1;
        }
        
        public override void OnStart()
        {
            var pop_type= GetPopType();
            if (pop_type >= 0)
            {
                MediatorTask.Instance.MarkOpenTask(pop_type);
            }

            foreach (var page in pages)
            {
                page.SetActive(true);
            }

            RefreshLessTime();
            RegisterInterval(1,
                () =>
                {
                    RefreshLessTime();

                    RefreshTipCom();
                });

            if (Root.Instance.CurTaskInfo.level > 0)
            {
                //当前选择的任务对应的page
                enterShowLevel = Root.Instance.CurTaskInfo.level;
            }
            else
            {
                if (GetTable()?["level"] != null)
                {
                    enterShowLevel = (int)GetTable()["level"];
                }
            }

            horizontalScrollSnap.StartingScreen = enterShowLevel - 1;


            for (int i = 0; i < CloseBtns.Length; i++)
            {
                CloseBtns[i].SetClick(Close);
            }

            foreach (var aboutBtn in AboutBtns)
            {
                aboutBtn.SetClick(ShowTipWindow);
            }
        }

        private void RefreshTipCom()
        {
            if (lastShowTip == null)
            {
                return;
            }

            hideTipComTime--;

            if (hideTipComTime <= 0)
            {
                lastShowTip.SetActive(false);
            }
        }

        void ShowTipWindow()
        {
            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
            {
                Type = UIConfirmData.UIConfirmType.OneBtn,
                // HideCloseBtn = true,
                desc = I18N.Get("kay_task_tip_1") + "\n\n" + I18N.Get("kay_task_tip_2") + "\n\n" +
                       I18N.Get("kay_task_tip_3"),
                confirmTitle = I18N.Get("key_ok_got_it"),
                AligmentType = TextAnchor.MiddleLeft,
                Rect2D = new Vector2(650, 650),
                // Position = new Vector2(0, 15),
            });
        }

        private void ResetTipShowTime()
        {
            hideTipComTime = 5;
        }

        private void RefreshLessTime()
        {
            for (int i = 0; i < pages.Length; i++)
            {
                var timeTextCom = GetTaskPage(i + 1).gameObject.FindChild<TimeGroupMono>("top").Text;
                var hourMinuteSecond = TimeUtils.Instance.ToHourMinuteSecond(Root.Instance.CurTaskInfo.LessTime);
                timeTextCom.text = hourMinuteSecond;
            }
        }

        private void ChooseTask()
        {
            MediatorTask.Instance.Choose(horizontalScrollSnap.CurrentPage + 1);
        }

        enum vname
        {
            taskConfigs
        }

        private Transform lastShowTip;

        public override void InitVm()
        {
            //防止刷新时引用到了之前的数据
            lastShowTip = null;
            var isChoosingLevel = Root.Instance.CurTaskInfo.level > 0;
            //禁止左右滚动
            HorizontalScrollRect.enabled = !isChoosingLevel;
            preBtn.SetActive(!isChoosingLevel);
            nextBtn.SetActive(!isChoosingLevel);

            vm[vname.taskConfigs.ToString()] =
                new ReactivePropertySlim<Dictionary<int, List<TaskConfig>>>(Root.Instance.TaskConfigs);
        }

        public override void InitBinds()
        {
            vm[vname.taskConfigs.ToString()].ToIObservable<Dictionary<int, List<TaskConfig>>>().Subscribe(taskConfigs =>
            {
                if (taskConfigs == null)
                {
                    return;
                }

                foreach (var kv in taskConfigs)
                {
                    // var page = GetTaskPage(kv.Key);
                    var level = kv.Key;
                    TaskInfo curTaskInfo = Root.Instance.CurTaskInfo;

                    //如果已经选择了任务, 但是
                    if (curTaskInfo.level > 0 && curTaskInfo.level != level)
                    {
                        continue;
                    }

                    var taskPage = GetTaskPage(level);
                    var chooseBtn = taskPage.Find("bottom").Find("Choose Btn").GetComponent<MyButton>();

                    if (Root.Instance.CurTaskInfo.level > 0)
                    {
                        chooseBtn.title = I18N.Get("key_go_claim");
                        chooseBtn.Gray = !curTaskInfo.CanGetReward;
                    }

                    chooseBtn.SetClick(() =>
                    {
                        if (Root.Instance.CurTaskInfo.level > 0)
                        {
                            if (chooseBtn.Gray)
                            {
                                return;
                            }

                            MediatorTask.Instance.ClaimReward();
                        }
                        else
                        {
                            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                            {
                                desc = I18N.Get("key_task_choose_tip"),
                                confirmCall = () => ChooseTask()
                            });
                        }
                    });


                    foreach (var row in kv.Value)
                    {
                        var rewardCom = GetRewardInPage(level, row.order);

                        if (rewardCom == null)
                        {
                            continue;
                        }
                        
                        var taskMono = rewardCom.GetComponent<TaskRewardItemMono>();

                        var item = new Item(row.award_type, row.award);

                        if (taskMono == null)
                        {
                            continue;
                        }
                        
                        var button = taskMono.Btn;

                        var tipCom = rewardCom.Find("Tip");
                        var isCurrentTask = row.order == curTaskInfo.progress;

                        taskMono.ShowProgress = isCurrentTask;
                        taskMono.Target = curTaskInfo.task.target;
                        taskMono.Progress = curTaskInfo.task.progress;

                        //移动到屏幕中间
                        if (isCurrentTask)
                        {
                            GetTaskScroll(level).GetComponent<ScrollRect>()
                                .ScrollToObject(rewardCom.GetComponent<RectTransform>());
                        }

                        //同一时间只展示一个tip , 不包括正在进行中的任务 ,5s后消失
                        button.SetClick(() =>
                        {
                            if (isCurrentTask && curTaskInfo.CanGetReward)
                            {
                                MediatorTask.Instance.ClaimReward();
                            }

                            if (!isCurrentTask)
                            {
                                if (lastShowTip != null)
                                {
                                    lastShowTip.SetActive(false);
                                }

                                ResetTipShowTime();
                                lastShowTip = tipCom;
                            }

                            tipCom.SetActive(true);
                        });

                        taskMono.Item = item;

                        if (isCurrentTask)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(tipCom.GetComponent<RectTransform>());
                        }

                        tipCom.SetActive(isCurrentTask);

                        Text textCom = tipCom.GetComponentInChildren<Text>();


                        /*switch (level)
                        {
                            case easyLevel:
                                textCom.color = new Color(178 / 255f, 90 / 255f, 37 / 255f);
                                break;
                            case normalLevel:
                                textCom.color = new Color(106 / 255f, 43 / 255f, 132 / 255f);
                                break;
                            case hardLevel:
                                textCom.color = new Color(0 / 255f, 64 / 255f, 136 / 255f);
                                break;
                        }*/

                        //文本
                        var localization = $"TASK_DESC_{row.task_type}";

                        var localParams = row.condition.Split(",");

                        // 特殊处理
                        if (row.task_type == 9)
                        {
                            localParams = localParams.Remove(localParams[1]);
                        }

                        var processedParams = new object[localParams.Length];
                        for (int j = 0; j < localParams.Length; j++)
                        {
                            var success = float.TryParse(localParams[j], out var num);
                            processedParams[j] = success ? num : I18N.Get(localParams[j]);
                        }

                        var textContent = I18N.Get(localization, processedParams);

                        textCom.text = textContent;

                        taskMono.IsGet = row.order < curTaskInfo.progress;
                        
                        taskMono.IsLock = curTaskInfo.level > 0 && curTaskInfo.progress < row.order;
                    }
                }
            });
        }

        public override void InitEvents()
        {
            AddEventListener(Proto.TASK_CHOOSE_LEVEL, (sender, eventArgs) =>
            {
                if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
                {
                    Refresh();
                }
            });

            AddEventListener(GlobalEvent.Sync_TaskInfo, (sender, eventArgs) => { Refresh(); });


            AddEventListener(Proto.GET_TASK_REWARD, (sender, eventArgs) =>
            {
                if (Root.Instance.CurTaskInfo.level == 0)
                {
                    Close();

                    UserInterfaceSystem.That.ShowQueue<UITaskPop>(() =>
                    {
                        var topUI = UserInterfaceSystem.That.GetTopNormalUI();
                        if (topUI == null)
                        {
                            return false;
                        }

                        return topUI.ClassType == typeof(UIMain);
                    });
                }
                else
                {
                    Refresh();
                }
            });
        }

        Transform GetTaskScroll(int level)
        {
            return HorizontalScrollRect.content.GetChild(level - 1).Find("scroll");
        }

        Transform GetTaskPage(int level)
        {
            if (level == 0)
            {
                int debug;
            }

            return HorizontalScrollRect.content.GetChild(level - 1);
        }

        Transform GetRewardInPage(Transform taskPage, int order)
        {
            var contentTrans = taskPage.GetComponent<ScrollRect>().content;
            var childCount = contentTrans.childCount - order;
            if (childCount < 0 || childCount >= contentTrans.childCount)
            {
                return null;
            }
            return contentTrans.GetChild(childCount);
        }

        Transform GetRewardInPage(int level, int order)
        {
            return GetRewardInPage(GetTaskScroll(level), order);
        }
    }
}