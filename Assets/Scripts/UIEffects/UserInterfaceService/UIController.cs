using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JerryMouse.EventDispatcher;
using JerryMouse.Extensions;
using JerryMouse.Interface;
using JerryMouse.Model;
using JerryMouse.Model.JsonConfig;
using JerryMouse.Utils;
using JerryMouse.Utils.Static;
using MyNotch;
using Slot;
using SlotX;
using SlotX.Utils;
using SlotX.Utils.Singleton;
using UI.CaTakeout;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace JerryMouse
{
    public class UIController : CaSystem<UIController>, IUpdate
    {
        struct QueueData
        {
            public object[] Args;
            public Func<bool> Condition;

            public QueueData(object[] args, Func<bool> condition = null)
            {
                this.Args = args;
                this.Condition = condition;
            }

            public int GetQueueOrder()
            {
                int result = 0;
                if (Args is { Length: > 0 } && Args[0] is MyTable table)
                {
                    if (table["queueOrder"] is int queueOrder)
                    {
                        result = queueOrder;
                    }
                }

                return result;
            }
        }
        
        public UIController()
        {
            RegisterInterval();
      
        }
        
        private const string resourceDirectory = "UIPrefab/";
        private const string prefix = "Father";
        
        private const int _base_sorting_order_ = -700;
        
        private int baseSortingOrder = _base_sorting_order_;

        private List<IUIInterface> activeUIList;

        private Camera _camera;
        
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Object.FindObjectOfType<Camera>(true);
                }

                return _camera;
            }
        }

        private List<IUIInterface> ActiveUIList => activeUIList ??= new List<IUIInterface>();

        private int _lastSortingOrder = -999999;

        private List<(Type, QueueData)> _showUIQueue;

        private CancellationTokenSource _uiQueueTokenSource;

        private const float _fadeTime = 0.2f;
        
        public float LastCloseUITime { get; set; }
        
        public bool NoUIInList()
        {
            if (ActiveUIList == null)
            {
                return false;
            }

            return ActiveUIList.Count == 0;
        }
        
        /// <summary>
        /// 优先现实的ui对象
        /// </summary>
        private List<GameObject> _topQueue;

        private bool _isShowing = false;

        private Dictionary<Type, Func<MyTable, bool>> _preShow = new()
        {
            {
                typeof(FatherUIGetRewards), table =>
                {
                    var items = table?["diff"] as List<ServerRes>;

                    if (items == null)
                    {
                        return false;
                    }

                    if (!items.Exists(item => item.Count > 0))
                    {
                        return false;
                    }
                    
                    return true;
                }
            },{
                typeof(FatherUITreasureRush), table =>
                {
                    if (MyNotchMain.Instance == null)
                    {
                        return false;
                    }
                    
                    return true;
                }
            },
        };

        public IUIInterface GetTopUI()
        {
            if (UIRoot == null) return null;
            
            for (int i = UIRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = UIRoot.transform.GetChild(i);
                child.TryGetComponent<IUIInterface>(out var ui);

                if (ui == null)
                {
                    continue;
                }
                
                if (ui is { UILayer: UILayer.NormalUI or UILayer.Window })
                {
                    return ui;
                }

                if (ui is {UILayer: UILayer.Top})
                {
                    if (ui.ClassType != typeof(FatherUITip)
#if debug_mode
                        && ui.ClassType != typeof(FatherUITools)
#endif          
                        )
                    {
                        return ui;
                    }
                }
            }

            return null;
        }


        private List<string> _uniUIName = new List<string>();

        public void AddUniUI(string name)
        {
            XLog.LogColor($"AddUniUI = {name}");
            _uniUIName.Add(name);
        } 
        
        public void RemoveUniUI(string name)
        {
            _uniUIName.Remove(name);
        }

        private bool IsUniUIInList()
        {
            return _uniUIName.Any();
        }
        
        public GameObject DisplayUI<T>(params object[] args) where T : FatherUI<T>
        {
            XLog.LogColor($"Show UI {typeof(T).Name}", "olive");
            
            if (typeof(T) == typeof(FatherUITip))
            {
              return  DisplayUI(typeof(T), true, args);
            }
            else
            {
              return  DisplayUI(typeof(T), false, args);
            }
        }

        // public void AddOtherUI<T>(params object[] args) where T : FatherUI<T>
        // {
        //     DisplayUI(typeof(T), true, args);
        // }

        public void OnReset()
        {
            _uiRoot = null;
            effectPanel = null;
            activeUIList?.Clear();
            RemoveAllQueue();
        }


        public void AddInQueue<T>(bool needWaitUI, params object[] args) where T : FatherUI<T>
        {
            _showUIQueue ??= new List<(Type, QueueData)>();
            if (needWaitUI)
            {
                _showUIQueue.Add((typeof(T),
                    new QueueData(args, () => { return !IsHaveAnyTopUIInternal(); })));
            }
            else
            {
                _showUIQueue.Add((typeof(T), new QueueData(args)));
            }
            ShowNextUI();
        }
        
        public bool IsHaveAnyTopUI()
        {
            if (Time.time - LastCloseUITime < _fadeTime)
            {
                return true;
            }
            
            return GetTopUI() != null || IsUniHaveUI();
        }

        public bool IsHaveAnyTopUIInternal()
        {
            return GetTopUI() != null || IsUniHaveUI();
        }
        
        public bool IsUniHaveUI()
        {
            if (MyNotchMain.Instance && !MyNotchMain.Instance.IsVirtualMachine)
            {
                return false;
            }

            return IsUniUIInList() ||  Time.time - RunTimeUtils.SendOpenUniUITime < 0.5f;
        }
        
        public void AddInQueue<T>(Func<bool> condition, params object[] args) where T : FatherUI<T>
        {
            _showUIQueue ??= new List<(Type, QueueData)>();
            _showUIQueue.Add((typeof(T), new QueueData(args, condition)));
            ShowNextUI();
        }
        
        public void AddFirstInQueue<T>(params object[] args) where T : FatherUI<T>
        {
            if (_isShowing)
            {
                _topQueue ??= new List<GameObject>();
                var uiGameObject = DisplayUI(typeof(T), false, args);
                _topQueue.Add(uiGameObject);
            }
            else
            {
                AddInQueue<T>(() => true, args);
            }
        }

        public void RemoveAllQueue()
        {
            _topQueue = null;
            _isShowing = false;
            _showUIQueue = null;
            
            _uiQueueTokenSource?.Cancel();
            _uiQueueTokenSource?.Dispose();
            _uiQueueTokenSource = null;
        }

        
        // public bool QueueIsNotEmpty()
        // {
        //     if (_showUIQueue == null)
        //     {
        //         return false;
        //     }
        //
        //     if (_isShowing)
        //     {
        //         return true;
        //     }
        //
        //     return _showUIQueue.Any();
        // }


        //todo 待测试
        public void RemoveUIInQueue<T>() where T : FatherUI<T>
        {
            if (_showUIQueue == null)
            {
                return;
            }

            bool finded = false;
            (Type, QueueData) item = (null, default);

            foreach (var valueTuple in _showUIQueue)
            {
                if (typeof(T) == valueTuple.Item1)
                {
                    finded = true;
                    item = valueTuple;
                }
            }

            if (finded)
            {
                _showUIQueue.Remove(item);
            }
        }

        private bool CheckSingle<T>() where T : FatherUI<T>
        {
            _showUIQueue ??= new List<(Type, QueueData)>();
            if (_showUIQueue.Exists(tuple => tuple.Item1 == typeof(T)))
            {
                return true;
            }

            var ui = GetTopUI();
            //uiconfirm 可以在已经显示的基础上弹出
            if (ui.ClassType != typeof(FatherUIConfirm) && ui.ClassType != typeof(FatherUIGetRewards) && ui.ClassType == typeof(T))
            {
                return true;
            }

            return false;
        }
        
        // public void AddSingleTonInQueue<T>(params object[] args) where T : FatherUI<T>
        // {
        //     if (CheckSingle<T>()) return;
        //
        //     _showUIQueue.Add((typeof(T), new QueueData(args)));
        //     ShowNextUI();
        // }
        //
        // public void AddSingleTonInQueue<T>(Func<bool> condition, params object[] args) where T : FatherUI<T>
        // {
        //     if (CheckSingle<T>()) return;
        //
        //     _showUIQueue.Add((typeof(T), new QueueData(args, condition)));
        //     ShowNextUI();
        // }


        async Task RegisterInterval()
        {
            await MyTask.WaitUntil(() => XFramework.Instance);
            XFramework.Instance.RegisterUpdate(this);
        }
        
        
        async Task ShowNextUI()
        {
            if (_showUIQueue == null || !_showUIQueue.Any())
            {
                MyEventDispatcher.Root.Raise(GlobalEvent_Kafka.ShowUIQueueFinish);
                return;
            }

            if (_isShowing)
            {
                return;
            }

            _uiQueueTokenSource ??= new CancellationTokenSource();
            var token = _uiQueueTokenSource.Token;

            _showUIQueue.Sort((a, b) => a.Item2.GetQueueOrder().CompareTo(b.Item2.GetQueueOrder()));

            var valueTuple = _showUIQueue.First();
            _showUIQueue.RemoveAt(0);
            _isShowing = true;

            var condition = valueTuple.Item2.Condition;

            //mark uiMain active 
            await MyTask.WaitUntil(() =>
            {
                try
                {
                    if (condition != null)
                    {
                        return condition.Invoke();
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    XLog.LogError(e.ToString());
                    return true;
                }
            }, cancellationToken: token);

            GameObject nextUI = DisplayUI(valueTuple.Item1, false, valueTuple.Item2.Args);

            await MyTask.WaitUntil(() =>
                {
                    if (_topQueue is { Count: > 0 })
                    {
                        return false;
                    }

                    if (nextUI == null)
                    {
                        return true;
                    }

                    if (activeUIList is { Count: > 0} && activeUIList[^1].UIGameObjectName_Wolfgang == nextUI.name)
                    {
                        return !nextUI.activeSelf;
                    }

                    return false;
                },
                cancellationToken: token);
            _isShowing = false;

            // 显示下一个UI窗口
            await ShowNextUI();
        }

        private GameObject _uiRoot;

        private GameObject UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    _uiRoot = GameObject.Find("UIRoot");
                    
                    if (_uiRoot == null)
                    {
                        _uiRoot =  new GameObject("UIRoot");
                    }
                    else
                    {
                        for (int i = 0; i < _uiRoot.transform.childCount; i++)
                        {
                            var uibehavior = _uiRoot.transform.GetChild(i).GetComponent<IUIInterface>();
                            if (uibehavior != null)
                            {
                                activeUIList.Add(uibehavior);
                            }
                        }
                    }
                }
                
                return _uiRoot;
            }
        }

        // private MyPool pool = new MyPool();

        public GameObject DisplayUI(Type type, bool alwayAddNew, params object[] args)
        {
            //preshow
            _preShow.TryGetValue(type, out var condition);

            MyTable table = null;
            if (args != null && args.Any())
            {
                table = args[0] as MyTable;
            }

            if (condition != null && !condition.Invoke(table))
            {
                return null;
            }
            
            var uiName = type.Name;
            if (type == typeof(FatherUISlotMachine))
            {
                uiName += $"-{SlotManager.Instance.CurrentSlotMachineId}";
            }
            
          
            var path = (resourceDirectory + uiName).Replace(prefix, "");

            IUIInterface ui = null;
            int classCount = 0;
            if (!alwayAddNew)
            {
                ui = ActiveUIList.Find(behaviour => behaviour.UIGameObjectName_Wolfgang == uiName);
            }
            else
            {
                classCount = ActiveUIList.Count(behaviour => behaviour.ClassType == type);
            }

            var existence = true;
            if (ui == null)
            {
                SceneManager.MoveGameObjectToScene(UIRoot.gameObject, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
                // UIRoot.TryGetComponent<InputMonitor>(out var inputMonitor);
                // if (inputMonitor == null)
                // {
                //     UIRoot.gameObject.AddComponent<InputMonitor>();
                // }

                GameObject obj;
                if (type != typeof(FatherUIWaitScene)
                    && type != typeof(FatherUIMockAdPlay)
                    )
                {
                    obj = ResourceController.Instance.GetObjSync(path, UIRoot.transform);
                }
                else
                {
                    obj = ResourceController.Instance.GetObjSync(path);
                }
             
          
                if (classCount > 0)
                {
                    obj.name = (uiName + classCount);
                }
                else
                {
                    obj.name = uiName;
                }

                ui = obj.GetComponent(type) as IUIInterface;

                if (ui == null)
                {
                    return null;
                }
          
                if (ui is MonoBehaviour monoBehaviour)
                {
                    monoBehaviour.enabled = true;
                }

                ui.UICanvas.worldCamera = Camera;
                if (ui.UILayer != UILayer.Top)
                {
                    ui.UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
                ui.UICanvas.sortingLayerName = "ui";
                if (ui is not FatherUISlotMachine 
                    && ui is not FatherUITreasureRush)
                {
                    ui.UICanvas.planeDistance = 10;
                }
                else
                {
                    ui.UICanvas.planeDistance = 50;
                }
            
                ActiveUIList.Add(ui);
                existence = false;
            }

            ui.@params = args;

            if (existence)
            {
                //插入到最后
                ActiveUIList.Remove(ui);
                ActiveUIList.Add(ui);
                ui.UISetActive(true);

                //可能会多次注册倒计时
                // ui.OnStart();
                //避免多次注册倒计时之类的
                ui.Refresh_Dostoyevsky();
            }

            var curSortingOrder = Math.Max(baseSortingOrder + ActiveUIList.Count, _lastSortingOrder + 6);
            if (ui.UILayer == UILayer.Top)
            {
                // ui.UICanvas.sortingLayerName = "Top";
            }

            ui.UICanvas.sortingOrder = curSortingOrder;
            _lastSortingOrder = curSortingOrder;
            return ui.GameObject;
        }

        public void RemoveUI(IUIInterface ui)
        {
            ActiveUIList.Remove(ui);
            if (ActiveUIList.Count == 0)
            {
                baseSortingOrder = _base_sorting_order_;
                return;
            }
            if (ui is { UILayer: UILayer.Overlay or UILayer.Top or UILayer.Window })
            {
                //显示 window mask
                var mono = ActiveUIList[^1];
                if (ActiveUIList.Count >= 1 && mono.UILayer == UILayer.Window)
                {
                    var gameObject = mono.GameObject;
                    SetUIMaskEnabled(gameObject, true);
                }

                //恢复ui层级
                if (mono.UICanvas.sortingOrder < _lastSortingOrder)
                {
                    _lastSortingOrder = mono.UICanvas.sortingOrder;
                }
            }
            else
            {
                ShowUIInStack(ui.UIGameObjectName_Wolfgang);
            }
        }

        private void SetUIMaskEnabled(GameObject gameObject, bool enabled)
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
                        image.color = image.color.AlphaChange(2 * alpha);
                    }

                    if (!enabled && alpha > 0.5)
                    {
                        image.color = image.color.AlphaChange(alpha / 2);
                    }
                }
            }
        }

        public void RemoveUIByName(string uiName)
        {
            var ui = ActiveUIList.Find(behaviour =>
            {
                if (behaviour == null)
                {
                    return false;
                }
                
                return behaviour.UIGameObjectName_Wolfgang == uiName;
            });
            ui?.Close();
        }

        public void HideUIInStack(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                return;
            }

            if (ActiveUIList is not {Count: > 0})
            {
                return;
            }
            
            //当前的ui
            var uiBehaviour = ActiveUIList[^1];
            if (uiBehaviour.UIGameObjectName_Wolfgang != uiName)
            {
                return;
            }

            //隐藏
            if (uiBehaviour.UILayer is UILayer.NormalUI or UILayer.Window
                && ActiveUIList.Count >= 2)
            {
                // ActiveUIList[^2].SwitchClick++;
                ActiveUIList[^2].David_LastOnTopTime_Hume = 0;
                if (ActiveUIList[^2].UILayer == UILayer.Window)
                {
                    var gameObject = ActiveUIList[^2].GameObject;
                    SetUIMaskEnabled(gameObject, false);
                }
            }
            
            for (int i = ActiveUIList.Count - 1; i >= 0; i--)
            {
                var ui = ActiveUIList[i];
                if (ui.UIGameObjectName_Wolfgang == uiName)
                {
                    //如果自身是这些ui 不遮挡后面的
                    if (ui is { UILayer: UILayer.Top or UILayer.Overlay or UILayer.Window })
                    {
                        break;
                    }

                    continue;
                }

                //这些ui 不被隐藏
                if (ui is not { UILayer: UILayer.Top or UILayer.Overlay })
                {
                    ui.UISetActive(false);
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

                if (ui.UIGameObjectName_Wolfgang.Contains("UITip"))
                {
                    continue;
                }

                if (ui.UILayer == UILayer.Overlay)
                {
                    continue;
                }

                if (excludeList != null && excludeList.Contains(ui.UIGameObjectName_Wolfgang))
                {
                    continue;
                }

                ui.Close();
            }
        }
        
        private void ShowUIInStack(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                return;
            }

            var count = ActiveUIList.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                var mono = ActiveUIList[i];

                //显示 window mask
                if (i == count && mono.UILayer == UILayer.Window)
                {
                    var monoGameObject = mono.GameObject;
                    SetUIMaskEnabled(monoGameObject, true);
                }
                
                if (!string.IsNullOrEmpty(mono.UIGameObjectName_Wolfgang) && mono.UIGameObjectName_Wolfgang == uiName)
                {
                    continue;
                }

                //退回到上一个 sortingOrder
                if (mono.UICanvas != null && mono.UICanvas.sortingOrder < _lastSortingOrder)
                {
                    _lastSortingOrder = mono.UICanvas.sortingOrder;
                }

                if (mono is { UILayer: UILayer.NormalUI or UILayer.Window })
                {
                    break;
                }
            }

            for (int i = ActiveUIList.Count - 1; i >= 0; i--)
            {
                var mono = ActiveUIList[i];
                if (!string.IsNullOrEmpty(mono.UIGameObjectName_Wolfgang) && mono.UIGameObjectName_Wolfgang == uiName)
                {
                    continue;
                }

                mono.UISetActive(true);
                if (mono is { UILayer: UILayer.NormalUI })
                {
                    break;
                }
            }
        }

        public void OnUpdate()
        {
            if (_topQueue is { Count: > 0 })
            {
                _topQueue.RemoveAll(o => o == null);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                  //TODO 根据当前的UI来确定行为
                var ui = GetTopUI();
                if (ui != null)
                {
                    if (ui.ClassType == typeof(FatherUITakeout))
                    {
                        FatherUITakeout takeoutFatherUI = ui as FatherUITakeout;
                        if (takeoutFatherUI.ConfirmPanel.gameObject.activeSelf)
                        {
                            return;
                        }
                        if (takeoutFatherUI.ExceptionPanel.gameObject.activeSelf)
                        {
                            return;
                        }
                        if (takeoutFatherUI.CommitDetailInfoPanel.gameObject.activeSelf)
                        {
                            takeoutFatherUI.CommitDetailInfoPanel.Hide();
                            return;
                        }
                        if (takeoutFatherUI.ReSendPanel.gameObject.activeSelf)
                        {
                            takeoutFatherUI.ReSendPanel.OnBackButtonClick();
                            return;
                        }
                        if (takeoutFatherUI.RegisterPanel.gameObject.activeSelf)
                        {
                            takeoutFatherUI.RegisterPanel.OnBackButtonClick();
                            return;
                        }
                        if (takeoutFatherUI.takeoutPanel.takeoutByGreenPanel.gameObject.activeSelf)
                        {
                            takeoutFatherUI.takeoutPanel.takeoutByGreenPanel.OnBackButtonClick();
                            return;
                        }
                        if (takeoutFatherUI.takeoutPanel.takeoutByCoinsPanel.gameObject.activeSelf)
                        {
                            takeoutFatherUI.takeoutPanel.takeoutByCoinsPanel.OnBackButtonClick();
                            return;
                        }
                        takeoutFatherUI.Close();
                        return;
                    }
                    
                    if (ui.ClassType == typeof(FatherUILogin))
                    {
                        return;
                    }
                }

                DisplayUI<FatherUIConfirm>(new UIConfirmContext
                {
                    desc = MyLang.ExitGameHint,
                    ConfirmCall = () =>
                    {
#if UNITY_EDITOR && false
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        MainGameUtils.ExitGame();
#endif
                    }
                });

                // Get<UIConfirm>().UICanvas.sortingLayerName = "Top";
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
                    effectPanel = ResourceController.Instance.GetObjSync("CommonPrefab/EffectPanel", UIRoot.transform);
                    var canvas = effectPanel.GetComponent<Canvas>();
                    canvas.worldCamera = Camera;
                    canvas.planeDistance = 10;
                    canvas.sortingLayerName = "ui";
                }

                return effectPanel;
            }
        }

        public T Get<T>() where T : FatherUI<T>
        {
            IUIInterface ui = null;
            
            ui = ActiveUIList.Find(behaviour => behaviour.ClassType == typeof(T));

            if (ui == null)
            {
                for (int i = 0; i < UIRoot.transform.childCount; i++)
                {
                    var uiBehavior = _uiRoot.transform.GetChild(i).GetComponent<IUIInterface>();
                    if (uiBehavior != null && uiBehavior.ClassType == typeof(T))
                    {
                        ui = uiBehavior;
                    }
                }
            }
            
            return ui as T;
        }

        public void HideUIRoot()
        {
            if (_uiRoot)
            {
                _uiRoot.SetActive(false);
            }
        }

        public void ShowUIRoot()
        {
            if (_uiRoot)
            {
                _uiRoot.SetActive(true);
            }
        }
    }
}