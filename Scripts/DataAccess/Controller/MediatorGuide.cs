using System.Linq;
using Core.Extensions;
using DataAccess.Model;
using DataAccess.Utils.Static;

namespace DataAccess.Controller
{
    public class MediatorGuide : global::Utils.Runtime.Singleton<MediatorGuide>
    {
        public bool IsTriggerGuidePass(TriggerGuideStep guideStep)
        {
            if (Root.Instance.GameGuideList.IsNullOrEmpty())
            {
                return false;
            }

#if DAI_TEST
            // if (guideStep == TriggerGuideStep.Lucky_Guy_Played_Effect)
            // {
            //     return false;
            // }
#endif
            
            var passed = Root.Instance.GameGuideList.Split(',');

            var guideId = ((int)guideStep).ToString();

            return passed.Contains(guideId);
        }
    }
}