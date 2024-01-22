using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Extensions;
using Cysharp.Threading.Tasks;

namespace DataAccess.Controller
{
    /// <summary>
    /// 不要实例化这个类
    /// </summary>
    public class Scheduler : global::Utils.Runtime.Singleton<Scheduler>
    {
        private Queue<Func<UniTask>> taskQueue;

        private UniTask currentTask;

        private bool isRunning;

        public virtual async void PostTask(Func<UniTask> task)
        {
            taskQueue ??= new Queue<Func<UniTask>>();
            taskQueue.Enqueue(task);

            if (isRunning)
            {
                return;
            }

            // if (currentTask is { Status: UniTaskStatus.Pending })
            // {
            //     return;
            // }

            await MoveNext();
        }

        private async UniTask MoveNext()
        {
            var taskWarp = SelectTask();

            if (taskWarp == null)
            {
                return;
            }

            isRunning = true;

            currentTask = taskWarp();
            try
            {
                await currentTask;
            }
            catch (Exception e)
            {
                // Log.Error(e.ToString());
            }

            isRunning = false;
            DeleteTask();

            await MoveNext();
        }

        protected virtual Func<UniTask> SelectTask()
        {
            return taskQueue.Count == 0 ? null : taskQueue.Peek();
        }

        protected virtual void DeleteTask()
        {
            taskQueue.Dequeue();
        }
    }
}