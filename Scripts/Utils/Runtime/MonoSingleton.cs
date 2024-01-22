using UnityEngine;

namespace Utils.Runtime
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            Instance = GetComponent<T>();
        }

        private void OnApplicationQuit()
        {
            Instance = null;
        }
    }

    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get => _instance ??= new T();
            protected set => _instance = value;
        }

        public static void Reset()
        {
            _instance = null;
        }
    }
}