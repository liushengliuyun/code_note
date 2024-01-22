using System;
using Core.Services.PersistService.API.Facade;
using DataAccess.Model;
using UI;
using UI.Activity;

namespace DataAccess.Controller
{
    public class MediatorUnlock : global::Utils.Runtime.Singleton<MediatorUnlock>
    {
        public bool IsActivityBtnUnlock(ActivityType activityType)
        {
            var match_count = Root.Instance.FinishHistoryCount;
            
            switch (activityType)
            {
                case ActivityType.StartPacker:
                    return match_count >= 2 || HaveShowUI(typeof(UIStartPack));
                case ActivityType.TaskSystem:
                    return match_count >= 1 || HaveShowUI(typeof(UITask));
                    break;
                case ActivityType.PiggyBank:
                    //第二局结束就有 piggybank的奖励
                    return match_count >= 1 || HaveShowUI(typeof(UIPiggyBank));
                    break;
                case ActivityType.Dragon:
                    return match_count >= 4 || HaveShowUI(typeof(UIDragon));
                    break;
                case ActivityType.FortuneWheel:
                    return match_count >= 3 || HaveShowUI(typeof(UIWheel));
                    break;
                case ActivityType.MonthCard:
                    return match_count >= 5 || HaveShowUI(typeof(UIMonthCardNew));
                    break;
                case ActivityType.MagicBall:
                    return match_count >= 3 || HaveShowUI(typeof(UIMagicBall));
                    break;
                case ActivityType.OnlineReward:
                    return match_count >= 5 || HaveShowUI(typeof(UIOnlineReward));
            }

            return true;
        }


        public void RecordShowUI(Type uiType)
        {
            if (uiType == null)
            {
                return;
            }
            PersistSystem.That.SaveValue(uiType.Name, true, true);
        }
        
        bool HaveShowUI(Type uiType)
        {
            if (uiType == null)
            {
                return false;
            }
           return (bool)PersistSystem.That.GetValue<bool>(uiType.Name, true);
        }
    }
}