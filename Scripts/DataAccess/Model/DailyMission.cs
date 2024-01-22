using System.Collections.Generic;

namespace DataAccess.Model
{
    public class DailyMission
    {
        public Dictionary<string, List<Mission>> daily_task_list;
        public List<Mission> super_task_list;
        public Dictionary<string, List<gift>> total_reward;
    }

    public class gift
    {
        public float point;
        public reward reward;
    }

    public class Mission
    {
        public string condition;
        public reward reward;
        public int type;
        public float point;
        public float time;
    }

    public class reward
    {
        public int coin;
        public float chips;
        public float bonus;
    }
    
    public class DailyTaskInfo
    {
        public List<int> list;
        public int supper;
        public float progress;
        public int point;
        public int total_claim;
        public int supper_flush;
        public float supper_progress;
        public int begin_time;
        public int daily_task_supper_begin_time;
        public int complete;
        public int total_order;

        public int task_target;
        public int supper_target;

        public int missonCompleted;
        public int superMissionCompleted;

        public int today_week;
    }

}