using UnityEngine;
using Utils;

namespace Core.Controllers
{
    public class YZBaseController<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly T YZInstance;

        public static GameObject YZGlobal { get; private set; }

        public static T Shared
        {
            get
            {
                return YZInstance;
            }
        }

        static YZBaseController()
        {
            GameObject Global = GameObject.Find("YZController");
            if (Global == null)
            {
                Global = new GameObject("YZController");
                DontDestroyOnLoad(Global);
            }
            if (YZInstance == null)
            {
                YZGlobal = Global;
                YZInstance = Global.AddComponent<T>();
                YZDebug.LogConcat("Instance: ", typeof(T), " Inited");
                (YZInstance as YZBaseController<T>).InitController();
            }
        }

        public virtual void InitController() { }

    }
}