using System;
using UnityEngine;
using UnityTimer;

namespace Utils
{
    public class TinyTimer
    {
        public static void StartTimer(Action action, float delay, MonoBehaviour monoBehaviour = null)
        {
            Timer.Register(delay, action, autoDestroyOwner: monoBehaviour);
        }
    }
}