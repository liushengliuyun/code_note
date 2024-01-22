using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Services.ResourceService.API.Facade;
using Core.Services.ResourceService.Internal.UniPooling;
using Core.Services.UserInterfaceService.API;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using MyBox;
using UI;
using UI.Activity;
using UI.UIChargeFlow;

using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace Core.Services.UserInterfaceService
{
    public class UserInterfaceSystem : IUserInterfaceSystem
    {
        struct queueData
        {
            public object[] args;
            public Func<bool> condition;

            public queueData(object[] args, Func<bool> condition = null)
            {
                this.args = args;
                this.condition = condition;
            }

            public int GetQueueOrder()
            {
                int result = 0;
                if (args is { Length: > 0 } && args[0] is GameData table)
                {
                    if (table["queueOrder"] is int queueOrder)
                    {
                        result = queueOrder;
                    }
                }

                return result;
            }
        }

        private readonly string resourceDirectory = "ui/";

        //todo 提取到外部配置
        public const int StandardWidth = 750;

        public const int StandardHeight = 1334;

        private readonly int baseSortingOrder = -700;

        private List<IUIBehaviour> activeUIList;

        public static Camera Camera;

        private List<IUIBehaviour> ActiveUIList => activeUIList ??= new List<IUIBehaviour>();

        private int lastSortingOrder = -999999;

        private List<(Type, queueData)> showuiQueue;

        private CancellationTokenSource uiQueueTokenSource;

        /// <summary>
        /// 优先现实的ui对象
        /// </summary>
        private List<GameObject> topQueue;

        private bool isShowing = false;

        private Dictionary<Type, Func<GameData, bool>> preShow = new()
        {
            { typeof(UIBestOffer), table => Root.Instance.RoomChargeInfo.BChargeInfo != null },
            { typeof(UIJustForYou), table => Root.Instance.RoomChargeInfo.AChargeInfo != null },
            {
                typeof(UISign), table =>
                {
                    bool isJustShow = false;
                    if (table != null)
                    {
                        isJustShow = (bool)table["isjustShow"];
                    }

                    if (isJustShow)
                    {
                        return true;
                    }

                    return Root.Instance.SignInfo.sign_chance == 1;
                }
            },

            {
                typeof(UIGetRewards), table =>
                {
                    var items = table?["diff"] as List<Item>;

                    return items != null;
                }
            },
            {
                typeof(UIStartPack), table => MediatorActivity.Instance.IsActivityBegin(ActivityType.StartPacker)
            },
            {
                typeof(UIPiggyBank), table => MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank)
            },
            {
                typeof(UIMagicBall), table => MediatorActivity.Instance.IsActivityBegin(ActivityType.MagicBall)
            },
            {
                typeof(UIDragon), table => MediatorActivity.Instance.IsActivityBegin(ActivityType.Dragon)
            },
            {
                typeof(UIDailyMission), table => MediatorActivity.Instance.IsActivityOpen(ActivityType.DailyMission)
            },
            {
                typeof(UILuckyGuy), table =>
                {
                    if (table == null)
                    {
                        return false;
                    }

                    //避免计时器弹出
                    if (table["bravo"] is not true && Root.Instance.ChargeSuccessCount > 0)
                    {
                        return false;
                    }
                    
                    return true;
                }
            },
            {
                typeof(UIChargeCtrl), _ =>
                {
                    if (Root.Instance.illegalityPeople)
                    {
                        API.Facade.UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_illegalityPeople"));
                    }
                    return !Root.Instance.illegalityPeople;
                }
            },
        };

        public IUIBehaviour GetTopNormalUI()
        {
            for (int i = activeUIList.Count - 1; i >= 0; i--)
            {
                var ui = activeUIList[i];
                if (ui.uiType is UIType.NormalUI or UIType.Window)
                {
                    return ui;
                }
            }

            return null;
        }

        public void ShowUI<T>(params object[] args) where T : UIBase<T>
        {
            if (typeof(T) == typeof(UITip))
            {
                ShowUI(typeof(T), true, args);
            }
            else
            {
                ShowUI(typeof(T), false, args);
            }
        }

        public void PreLoadUI<T>() where T : UIBase<T>
        {
            // var uiName = typeof(T).Name;
            // var path = (resourceDirectory + uiName).ToLower();
            // //
            // pool.Spawner.SpawnSync(path, UIRoot).GameObj.Restore();
        }

        public void ShowAnotherUI<T>(params object[] args) where T : UIBase<T>
        {
            ShowUI(typeof(T), true, args);
        }

        public void Reset()
        {
            RemoveAllQueue();
        }

        public void RemoveAllQueue()
        {
            topQueue = null;
            isShowing = false;
            showuiQueue = null;

            uiQueueTokenSource?.Cancel();
            uiQueueTokenSource?.Dispose();
            uiQueueTokenSource = null;
        }

        public void ShowQueue<T>(params object[] args) where T : UIBase<T>
        {
            showuiQueue ??= new List<(Type, queueData)>();
            showuiQueue.Add((typeof(T), new queueData(args)));
            ShowNextUI().Forget();
        }

        public void ShowQueue<T>(Func<bool> condition, params object[] args) where T : UIBase<T>
        {
            showuiQueue ??= new List<(Type, queueData)>();
            showuiQueue.Add((typeof(T), new queueData(args, condition)));
            ShowNextUI().Forget();
        }

        /// <summary>
        /// 马上显示, 并且有队列的逻辑
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void TopInQueue<T>(params object[] args) where T : UIBase<T>
        {
            if (isShowing)
            {
                topQueue ??= new List<GameObject>();
                var uiGameObject = ShowUI(typeof(T), false, args);
                topQueue.Add(uiGameObject);
            }
            else
            {
                ShowQueue<T>(() => true, args);
            }
        }

        public bool HaveUIInQueue()
        {
            if (showuiQueue == null)
            {
                return false;
            }

            if (isShowing)
            {
                return true;
            }

            return showuiQueue.Any();
        }


        //todo 待测试
        public void RemoveUIInQueue<T>() where T : UIBase<T>
        {
            if (showuiQueue == null)
            {
                return;
            }

            bool finded = false;
            (Type, queueData) item = (null, default);

            foreach (var valueTuple in showuiQueue)
            {
                if (typeof(T) == valueTuple.Item1)
                {
                    finded = true;
                    item = valueTuple;
                }
            }

            if (finded)
            {
                showuiQueue.Remove(item);
            }
        }

        public void SingleTonQueue<T>(params object[] args) where T : UIBase<T>
        {
            showuiQueue ??= new List<(Type, queueData)>();
            if (showuiQueue.Exists(tuple => tuple.Item1 == typeof(T)))
            {
                return;
            }

            showuiQueue.Add((typeof(T), new queueData(args)));
            ShowNextUI().Forget();
        }

        public void SingleTonQueue<T>(Func<bool> condition, params object[] args) where T : UIBase<T>
        {
            showuiQueue ??= new List<(Type, queueData)>();
            if (showuiQueue.Exists(tuple => tuple.Item1 == typeof(T)))
            {
                return;
            }

            showuiQueue.Add((typeof(T), new queueData(args, condition)));
            ShowNextUI().Forget();
        }

        async UniTask ShowNextUI()
        {
            if (showuiQueue == null || !showuiQueue.Any())
            {
                EventDispatcher.Root.Raise(GlobalEvent.SHOW_UI_QUEUE_FINISH);
                return;
            }

            if (isShowing)
            {
                return;
            }

            uiQueueTokenSource ??= new CancellationTokenSource();
            var token = uiQueueTokenSource.Token;

            showuiQueue.Sort((a, b) => a.Item2.GetQueueOrder().CompareTo(b.Item2.GetQueueOrder()));

            var valueTuple = showuiQueue.First();
            showuiQueue.RemoveAt(0);
            isShowing = true;

            var condition = valueTuple.Item2.condition;

            //mark uiMain active 
            await UniTask.WaitUntil(() =>
            {
                try
                {
                    if (condition != null)
                    {
                        return condition.Invoke();
                    }
                    else
                    {
                        var uimain = Get<UIMain>();
                        return uimain != null && uimain.IsVisible;
                    }
                }
                catch (Exception e)
                {
                    YZLog.Error(e.ToString());
                    return true;
                }
            }, cancellationToken: token);

            GameObject nextUI = ShowUI(valueTuple.Item1, false, valueTuple.Item2.args);

            await UniTask.WaitUntil(() =>
                {
                    if (topQueue is { Count: > 0 })
                    {
                        return false;
                    }

                    if (nextUI == null)
                    {
                        return true;
                    }

                    if (activeUIList is { Count: > 0} && activeUIList[^1].UIName == nextUI.name)
                    {
                        return !nextUI.activeSelf;
                    }

                    return false;
                },
                cancellationToken: token);
            isShowing = false;

            // 显示下一个UI窗口
            await ShowNextUI();
        }

        private Transform UIRoot;

        // private MyPool pool = new MyPool();

        private GameObject ShowUI(System.Type type, bool alwayAddNew, params object[] args)
        {
            //preshow
            preShow.TryGetValue(type, out var condition);

            GameData table = null;
            if (args != null && args.Any())
            {
                table = args[0] as GameData;
            }

            if (condition != null && !condition.Invoke(table))
            {
                return null;
            }

            if (Camera == null)
            {
                Camera = GameObject.Find("Camera").GetComponent<Camera>();
            }

            var uiName = type.Name;
            YZLog.LogColor($"Show UI {uiName}", "olive");
            var path = (resourceDirectory + uiName).ToLower();

            IUIBehaviour ui = null;
            int classCount = 0;
            if (!alwayAddNew)
            {
                ui = ActiveUIList.Find(behaviour => behaviour.UIName == uiName);
            }
            else
            {
                classCount = ActiveUIList.Count(behaviour => behaviour.ClassType == type);
            }

            var existence = true;
            if (ui == null)
            {
                UIRoot ??= new GameObject("UIRoot").transform;
                UIRoot.TryGetComponent<InputMonitor>(out var inputMonitor);
                if (inputMonitor == null)
                {
                    UIRoot.gameObject.AddComponent<InputMonitor>();
                }

                var assetSync = ResourceSystem.That.GetAssetHandle<GameObject>(path);
                var obj = assetSync.InstantiateSync(UIRoot);
                if (classCount > 0)
                {
                    obj.name = uiName + classCount;
                }
                else
                {
                    obj.name = uiName;
                }

                ui = obj.GetComponent(type) as IUIBehaviour;

                if (ui == null)
                {
                    return null;
                }

                if (ui is MonoBehaviour monoBehaviour)
                {
                    monoBehaviour.enabled = true;
                }

                ui.AssetHandle = assetSync;
                ui.UICanvas.worldCamera = Camera;
                ActiveUIList.Add(ui);
                existence = false;
            }

            ui.args = args;

            if (existence)
            {
                //插入到最后
                ActiveUIList.Remove(ui);
                ActiveUIList.Add(ui);
                ui.SetActive(true);

                //可能会多次注册倒计时
                // ui.OnStart();
                //避免多次注册倒计时之类的
                ui.Refresh();
            }

            var curSortingOrder = Math.Max(baseSortingOrder + ActiveUIList.Count, lastSortingOrder + 6);
            if (ui.uiType == UIType.Top)
            {
                ui.UICanvas.sortingLayerName = "Top";
            }

            ui.UICanvas.sortingOrder = curSortingOrder;
            lastSortingOrder = curSortingOrder;
            return ui.GameObject;
        }

        public void RemoveUI(IUIBehaviour ui)
        {
            ActiveUIList.Remove(ui);
            if (ui is { uiType: UIType.Overlay or UIType.Top or UIType.Window })
            {
                //显示 window mask
                var mono = ActiveUIList[^1];
                if (ActiveUIList.Count >= 1 && mono.uiType == UIType.Window)
                {
                    var gameObject = mono.GameObject;
                    SetMaskEnabled(gameObject, true);
                }
                
                //恢复ui层级
                if (mono.UICanvas.sortingOrder < lastSortingOrder)
                {
                    lastSortingOrder = mono.UICanvas.sortingOrder;
                }
            }
            else
            {
                ShowUIStack(ui.UIName);
            }
        }

        private void SetMaskEnabled(GameObject gameObject, bool enabled)
        {
            if (gameObject != null)
            {
                var transform = gameObject.transform.GetChild(0);
                transform.TryGetComponent<Image>(out var image);
         
                if (image != null && image.color.a < 1)
                {
                    var alpha = image.color.a;
                    if (enabled && alpha < 0.5)
                    {
                        image.color = image.color.WithAlphaSetTo(2 * alpha);
                    }

                    if (!enabled && alpha > 0.5)
                    {
                        image.color = image.color.WithAlphaSetTo(alpha / 2);
                    }
                }
            }
        }

        public void RemoveUIByName(string uiName)
        {
            var ui = ActiveUIList.Find(behaviour => behaviour.UIName == uiName);
            ui?.Close();
        }

        private bool? _isMatchHeight;


        public void HideUIStack(string uiName)
        {
            if (ExtensionString.IsNullOrEmpty(uiName))
            {
                return;
            }
            
            //当前的ui
            var uiBehaviour = ActiveUIList[^1];
            if (uiBehaviour.UIName != uiName)
            {
                return;
            }

            //隐藏
            if (uiBehaviour.uiType is not (UIType.Overlay or UIType.Top) 
                && ActiveUIList.Count >= 2)
            {
                ActiveUIList[^2].SwitchClick++;
                if ( ActiveUIList[^2].uiType == UIType.Window)
                {
                    var gameObject = ActiveUIList[^2].GameObject;
                    SetMaskEnabled(gameObject, false);
                }
            }

            for (int i = ActiveUIList.Count - 1; i >= 0; i--)
            {
                var ui = ActiveUIList[i];
                if (ui.UIName == uiName)
                {
                    //如果自身是这些ui 不遮挡后面的
                    if (ui is { uiType: UIType.Top or UIType.Overlay or UIType.Window })
                    {
                        break;
                    }

                    continue;
                }

                //这些ui 不被隐藏
                if (ui is not { uiType: UIType.Top or UIType.Overlay })
                {
                    ui.SetActive(false);
                }
            }
        }

        private void ShowUIStack(string uiName)
        {
            if (ExtensionString.IsNullOrEmpty(uiName))
            {
                return;
            }

            var count = ActiveUIList.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                var mono = ActiveUIList[i];

                //显示 window mask
                if (i == count && mono.uiType == UIType.Window)
                {
                    var monoGameObject = mono.GameObject;
                    SetMaskEnabled(monoGameObject, true);
                }

                if (mono.UIName == uiName)
                {
                    continue;
                }

                //退回到上一个 sortingOrder
                if (mono.UICanvas.sortingOrder < lastSortingOrder)
                {
                    lastSortingOrder = mono.UICanvas.sortingOrder;
                }

                if (mono is { uiType: UIType.NormalUI or UIType.Window })
                {
                    break;
                }
            }

            for (int i = ActiveUIList.Count - 1; i >= 0; i--)
            {
                var mono = ActiveUIList[i];
                if (mono.UIName == uiName)
                {
                    continue;
                }

                mono.SetActive(true);
                if (mono is { uiType: UIType.NormalUI })
                {
                    break;
                }
            }
        }

        public void CloseAllUI(string[] excludeList = null)
        {
            if (activeUIList == null)
            {
                return;
            }

            for (int i = activeUIList.Count - 1; i >= 0; i--)
            {
                var ui = activeUIList[i];
                if (ui == null)
                {
                    continue;
                }

                if (ui.UIName == "UITip")
                {
                    continue;
                }

                if (ui.uiType == UIType.Overlay)
                {
                    continue;
                }

                if (excludeList != null && excludeList.Contains(ui.UIName))
                {
                    continue;
                }

                ui.Close();
            }
        }

        public bool IsMatchHeight
        {
            get
            {
                if (_isMatchHeight == null)
                {
                    //计算宽高比例  
                    float standardAspect = (float)StandardWidth / StandardHeight;
                    float deviceAspect = (float)Screen.width / Screen.height;
                    float adjustor = 0;
                    //计算矫正比例  
                    if (deviceAspect < standardAspect)
                    {
                        adjustor = standardAspect / deviceAspect;
                    }

                    _isMatchHeight = adjustor == 0;
                }

                return (bool)_isMatchHeight;
            }
        }


        public void OnUpdate()
        {
            if (topQueue is { Count: > 0 })
            {
                topQueue.RemoveAll(o => o == null);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //TODO 根据当前的UI来确定行为
                var ui = GetTopNormalUI();
                if (ui.ClassType == typeof(UILogin) || ui.ClassType == typeof(UIMatch) ||
                    ui.ClassType == typeof(UIChargeWebview))
                {
                    return;
                }

                ShowUI<UIConfirm>(new UIConfirmData
                {
                    desc = I18N.Get("EXIT_GAME_HINT"),
                    confirmCall = () =>
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        GameUtils.ExitGame();
#endif
                    }
                });

                Get<UIConfirm>().UICanvas.sortingLayerName = "Top";
            }

#if DAI_TEST

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                ShowUI<UITools>();
            }
#endif
        }

        private GameObject effectPanel;

        public GameObject EffectPanel
        {
            get
            {
                if (effectPanel == null)
                {
                    effectPanel = ResourceSystem.That.InstantiateGameObjSync("ui/effectpanel", UIRoot);
                    effectPanel.GetComponent<Canvas>().worldCamera = Camera;
                }

                return effectPanel;
            }
        }

        public T Get<T>() where T : UIBase<T>
        {
            var ui = ActiveUIList.Find(behaviour => behaviour.ClassType == typeof(T));

            return ui as T;
        }
    }
}