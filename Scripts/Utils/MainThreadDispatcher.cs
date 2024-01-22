using System;
using Carbon.Util;
using Utils.Runtime;

namespace Utils
{
    public class MainThreadDispatcher : MonoSingleton<MainThreadDispatcher>
    {
        private readonly LockFreeQueue<Action> ExecutionQueue = new LockFreeQueue<Action>();

        public void Dispatch(Action action)
        {
            ExecutionQueue.Enqueue(action);
        }

        public void Update()
        {
            while (ExecutionQueue.Dequeue(out var logFunc))
            {
                logFunc?.Invoke();
            }
        }
    }
}