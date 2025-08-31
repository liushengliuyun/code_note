// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// // using JerryMouse.Extensions;
// // using SlotX.Utils.Singleton;
//
// namespace JerryMouse.Controller
// {
//     /// <summary>
//     /// 数字越小优先级越低
//     /// </summary>
//     public static class SchedulerIds
//     {
//         public static int BonusRes = -1;
//
//         public static int CoinRes = 0;
//         
//         public static int TackoutGuide = 5;
//
//         public static int Sign = 7;
//             
//         public static int BigWin = 10;
//         
//         public static int Wallet = 11;
//         
//         /// <summary>
//         /// 钥匙比钱包和BigWin的显示优先级更高
//         /// </summary>
//         public static int Key = 12;
//         
//         public static int ScratchTicket = 40;
//         
//         public static int SepcialGift = 50;
//
//         
//         /// <summary>
//         /// 任务完成
//         /// </summary>
//         public static int TaskDone = 60;
//
//         public static int SaveBoxFull = 63;
//         
//         public static int HatPieceFull = 65;
//         
//         public static int FruitMachinePieceFull = 68;
//         
//
//         
//         public static int GetLotteries = 70;
//         
//         public static int PopMysteryGift = 80;
//         
//         public static int GetPiece = 85;
//         
//         public static int OpenSavingPot = 90;
//         
//         public static int FreeSpin = 100;
//         
//         public static int BonusGame = 200;
//      
//         
//         public static int PopGreen = 250;
//         /// <summary>
//         /// 优先级别低了会被卡住
//         /// </summary>
//         public static int BigRewardsEff = 300;
//         
//         /// <summary>
//         /// 在freespin中可以触发
//         /// </summary>
//         public static int SpinGreenAd = 400;
//
//     }
//
//     public class MyScheduler : XSingleton<MyScheduler>
//     {
//         private Queue<Func<Task>> _taskQueue;
//
//         private Task _currentTask;
//
//         public bool IsRunning;
//
//         public bool IsAnyTask(bool excludedRes = false)
//         {
//             return IsRunning || _taskQueue is { Count: > 0 } ||
//                    (excludedRes ? IsPriorityHaveExcludedRes() : _taskPriority.Any());
//         }
//
//         public bool IsPriorityHaveExcludedRes()
//         {
//             if (_taskPriority == null)
//             {
//                 return false;
//             }
//
//             foreach (var pare in _taskPriority)
//             {
//                 if (pare.Key == SchedulerIds.CoinRes || pare.Key == SchedulerIds.BonusRes)
//                 {
//                     continue;
//                 }
//
//                 return true;
//             }
//
//             return false;
//         }
//         
//
//         public Dictionary<int, int> _taskPriority = new Dictionary<int, int>();
//
//         /// <summary>
//         /// 0的优先级最低
//         /// </summary>
//         public void AddWaitList(int id, int priority)
//         {
//             _taskPriority[id] = priority;
//         }
//
//         public void AddWaitList(int id)
//         {
//             _taskPriority[id] = id;
//         }
//         
//         public void RemoveWaitList(int id)
//         {
//             _taskPriority.Remove(id);
//         }
//
//         public bool IsCanTriggerTask(int id)
//         {
//             if (_taskPriority.TryGetValue(id, out var priority))
//             {
//                 foreach (var pair in _taskPriority)
//                 {
//                     // 存在优先级更高的还在队列里
//                     if (pair.Value > priority)
//                     {
//                         return false;
//                     }
//                 }
//
//                 return true;
//             }
//
//             //没有进入比较, 返回true
//             return true;
//         }
//
//
//         public bool IsTaskRunning(int id)
//         {
//             return _taskPriority.ContainsKey(id);
//         }
//         
//         public virtual async void PostTask(Func<Task> task)
//         {
//             _taskQueue ??= new Queue<Func<Task>>();
//             _taskQueue.Enqueue(task);
//
//             if (IsRunning)
//             {
//                 return;
//             }
//
//             // mark ? 
//             // if (currentTask is { Status: TaskStatus.Pending })
//             // {
//             //     return;
//             // }
//
//             await MoveNext();
//         }
//
//         private async Task MoveNext()
//         {
//             var taskWarp = SelectTask();
//
//             if (taskWarp == null)
//             {
//                 return;
//             }
//
//             IsRunning = true;
//
//             _currentTask = taskWarp();
//             try
//             {
//                 await _currentTask;
//             }
//             catch (Exception e)
//             {
//                 // _currentTask.Dispose();
//                 XLog.LogColor($"{nameof(_currentTask)}异常" + e + "\n" + e.StackTrace, "red");
//             }
//
//             IsRunning = false;
//             DeleteTask();
//
//             await MoveNext();
//         }
//
//         protected virtual Func<Task> SelectTask()
//         {
//             return _taskQueue.Count == 0 ? null : _taskQueue.Peek();
//         }
//
//         protected virtual void DeleteTask()
//         {
//             if (_taskQueue is { Count: > 0 })
//             {
//                 _taskQueue.Dequeue();
//             }
//         }
//
//         public void Clear()
//         {
//             if (_currentTask != null && _currentTask.IsCompleted)
//             {
//                 _currentTask.Dispose();
//             }
//
//             _currentTask = null;
//             
//             if (_taskQueue is { Count: > 0 })
//             {
//                 _taskQueue.Clear();
//             }
//
//             _taskPriority.Clear();
//
//             
//             Instance = new MyScheduler();
//         }
//     }
// }