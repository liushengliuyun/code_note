using UnityEngine;

namespace DataAccess.Utils
{
    public class AppInfoUtils : global::Utils.Runtime.Singleton<AppInfoUtils>
    {
        public int GetCommunicationVersion()
        {
            //todo 从硬盘上读取
            return 1;
        }
    }
}