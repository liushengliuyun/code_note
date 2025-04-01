using Utils;
using Debug = UnityEngine.Debug;

namespace Functions
{
    public static class XLog
    {
        public static bool Enable = true;
        
        public static void Log(object obj)
        {
            if (Enable)
            {
                Debug.Log(obj);
            }
        }

        public static void LogConcat(params object[] args)
        {
            if (Enable)
            {
                Debug.Log(StringBuilderUtils.Join(args));
            }
        }
        
        
        public static void LogColor(object message, string color = "orange")
        {
            if (Enable)
            {
                if (string.IsNullOrEmpty(color))
                {
                    UnityEngine.Debug.Log(message);
                }
                else
                {
                    UnityEngine.Debug.LogFormat("<color={0}>{1}</color>", color, message);
                }
            }
        }
        
        public static void LogError(object obj)
        {
            if (Enable)
            {
                Debug.LogError(obj);
            }
        }
    }
}