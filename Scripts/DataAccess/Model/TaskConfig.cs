using System;
using DataAccess.Utils;

namespace DataAccess.Model
{
    public class TaskConfig
    {
        public int order;

        private int type;

        public float target;

        public int award_type;

        /// <summary>
        /// 奖励额度
        /// </summary>
        public float award;

        public string condition;

        public int task_type;
    }


    public struct CastleTask
    {
        public int type;

        /// <summary>
        /// 当前任务进度
        /// </summary>
        public int progress;

        public float target;

        public bool IsDone => progress >= target;
    }

    public class TaskInfo
    {
        /// <summary>
        /// 活动开启时间， 接取活动后持续72小时
        /// </summary>
        public int begin_time;

        /// <summary>
        /// 难度等级 1, 2, 3
        /// </summary>
        public int level;

        /// <summary>
        /// 当前进行到了哪个任务
        /// </summary>
        public int progress;

        public int FinishProgress
        {
            get
            {
                if (task.IsDone)
                {
                    return progress;
                }

                return progress - 1;
            }
        }

        public int AllTaskCount
        {
            get
            {
                if (level == 0)
                {
                    return 0;
                }

                Root.Instance.TaskConfigs.TryGetValue(level, out var list);
                if (list == null)
                {
                    return 0;
                }

                return list.Count;
            }
        }


        /// <summary>
        /// 记录任务进度 , 在开始游戏时记录,  结束游戏时对比
        /// </summary>
        private int lastTaskProgress;
        

        public CastleTask task;

        /// <summary>
        /// 活动开启时间， 接取活动后持续72小时, 再开启时间内
        /// </summary>
        public bool IsInOpenTime => begin_time > 0 && TimeUtils.Instance.UtcTimeNow - begin_time <= 3 * 3600 * 24;

        /// <summary>
        /// 任务已经开始
        /// </summary>
        public bool IsBegin => begin_time > 0;

        public int LessTime
        {
            get
            {
                if (begin_time > 0)
                {
                    var lessTime = begin_time + 3 * 3600 * 24 - TimeUtils.Instance.UtcTimeNow;

                    return Math.Max(0, lessTime);
                }
                else
                {
                    return Root.Instance.RegisterTime + 5 * 3600 * 24 - TimeUtils.Instance.UtcTimeNow;
                }
            }
        }

        public bool CanGetReward => IsInOpenTime && level > 0 && task.IsDone;


        /// <summary>
        /// 记录任务进度
        /// </summary>
        public void RecordProgress()
        {
            lastTaskProgress = task.progress;
        }

        /// <summary>
        /// 任务进度是否更新
        /// </summary>
        /// <returns></returns>
        public bool IsProgressBigger()
        {
            if (!IsInOpenTime)
            {
                return false;
            }

            return task.progress > lastTaskProgress;
        }
    }
}