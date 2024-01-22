using System;
using System.Collections.Generic;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Services.NetService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Newtonsoft.Json.Linq;
using UI;
using Utils;

namespace DataAccess.Controller
{
    public class MediatorTask : global::Utils.Runtime.Singleton<MediatorTask>, IRequestSender
    {
        private SortedDictionary<string, object> paramHandler;
        public SortedDictionary<string, object> ParamHandler => paramHandler ??= new SortedDictionary<string, object>();


        public void GetTaskInfo()
        {
            ParamHandler.Clear();

            NetSystem.That.SendGameRequest(Proto.GET_TASK_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.CurTaskInfo =
                        DeserializeJObject<TaskInfo>("data.castleTaskInfo", jObject);
                },
                MediatorRequest.Instance.GetBaseJson(ParamHandler)
            );
        }

        public void Choose(int level, Action callback = null)
        {
            ParamHandler.Clear();
            ParamHandler["level"] = level;
            NetSystem.That.SendGameRequest(Proto.TASK_CHOOSE_LEVEL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.CurTaskInfo =
                        DeserializeJObject<TaskInfo>("data.castleTaskInfo", jObject);
                    
                    callback?.Invoke();
                },
                MediatorRequest.Instance.GetBaseJson(ParamHandler)
            );
        }


        public void ClaimReward()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_TASK_REWARD,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.CurTaskInfo =
                        DeserializeJObject<TaskInfo>("data.castleTaskInfo", jObject);
                    MediatorRequest.Instance.SyncItem(jObject.SelectToken("data.balance").ToString());
                },
                MediatorRequest.Instance.GetBaseJson(ParamHandler));
        }


        private T DeserializeJObject<T>(string path, JObject jObject)
        {
            return YZJsonUtil.DeserializeJObject<T>(path, jObject);
        }

        public float GetBonusCount(int level)
        {
            Root.Instance.TaskConfigs.TryGetValue(level, out var rows);
            if (rows == null) return 0;

            float sum = rows.Where(tc => tc.award_type is Const.Cash or Const.Bonus)
                .Sum(tc => tc.award);
            return sum;
        }

        /// <summary>
        /// 0：玩家主动点击弹出；1：登录时弹出；2：对局结束后弹出, 3 点击进度按钮弹出
        /// </summary>
        /// <param name="popType"></param>
        public void PopTaskSystem(int popType)
        {
            //新号创建的五天内开启, 活动开始后 , 持续72小时, 
            if (IsTaskActivityOpen())
            {
                var args = new GameData()
                {
                    ["pop_type"] = popType
                };
                
                MediatorUnlock.Instance.RecordShowUI(typeof(UITask));
                
                //没有选择任务 或者任务已经完成
                if (Root.Instance.CurTaskInfo.level == 0)
                {
                    if (popType == 0 || popType == 3)
                    {
                        UserInterfaceSystem.That.ShowUI<UITaskPop>(args);
                    }
                    else
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UITaskPop>(args);
                    }
                }
                else
                {
                    if (popType == 0 || popType == 3)
                    {
                        UserInterfaceSystem.That.ShowUI<UITask>(args);
                    }
                    else
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UITask>(args);
                    }
                }
            }
        }

        public bool IsChooseTask()
        {
            return IsTaskActivityOpen() && Root.Instance.CurTaskInfo.level > 0;
        }

        /// <summary>
        ///  0玩家主动点击弹出；1：登录时弹出；2：对局结束后弹出
        /// </summary>
        /// <param name="type"></param>
        public void MarkOpenTask(int type)
        {
            YZFunnelUtil.SendYZEvent("quest_mainui_popup",
                new Dictionary<string, object>()
                {
                    { "pop_type", type }
                });
        }

        public bool IsTaskActivityOpen()
        {
            //还没有选择任务 , 5天内
            var taskNotOpen = !Root.Instance.IsTaskBegin && !TimeUtils.Instance.IsDayPassRegisterTime(5);

            if (taskNotOpen)
            {
                return true;
            }

            //选择了任务还没超时
            var taskOpen = Root.Instance.IsTaskBegin && Root.Instance.CurTaskInfo.IsInOpenTime;

            return taskOpen;
        }
    }
}