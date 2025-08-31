using System;
using System.Collections.Generic;
using System.Linq;
using Ad;
using DG.Tweening;
using JerryMouse.Controller;
using JerryMouse.EventDispatcher;
using JerryMouse.Extensions;
using JerryMouse.Model;
using JerryMouse.Model.JsonConfig;
using JerryMouse.Utils;
using JerryMouse.Utils.Static;
using MyNotch;
using SlotX;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JerryMouse
{
    /// <summary>
    /// 所有UI的基类
    /// </summary>
    public abstract class FatherUI<T> : MyMonoBehaviour, IUIInterface
    {
#if use_react
         protected Dictionary<string, RxProperty_Slim> vm;
#endif

        private MyEventDispatcher _dispatcher;

        // protected bool IsCloseing;

        private Canvas _canvas;
        
        class InternalTimer
        {
            public float LastTriggerTime;
            public UnityEvent TimerEvent;
        }
        
        private Dictionary<float, InternalTimer> _intervalTimerMap;

        protected virtual void Update()
        {
            if (_intervalTimerMap == null)
            {
                return;
            }

            foreach (var item in _intervalTimerMap)
            {
                //Time.time 会随着游戏暂停而停止计算
                var interval = item.Key;
                var timer = item.Value;
                if (timer.LastTriggerTime == 0)
                {
                    timer.LastTriggerTime = Time.time;
                }
                else
                {
                    if (Time.time - timer.LastTriggerTime > interval)
                    {
                        timer.LastTriggerTime = Time.time;
                        timer.TimerEvent?.Invoke();
                    }
                }
            }
        }
        
        /// <summary>
        /// update 在被隐藏时 不会执行
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="runImmediately"></param>
        protected void RegisterInterval_Goethe(float interval, UnityAction action, bool runImmediately = false)
        {
            _intervalTimerMap ??= new Dictionary<float, InternalTimer>();
            if (!_intervalTimerMap.TryGetValue(interval, out var timer))
            {
                var timeradd = new InternalTimer()
                {
                    TimerEvent = new UnityEvent()
                };
                timeradd.TimerEvent.AddListener(action);
                _intervalTimerMap[interval] = timeradd;
            }
            else
            {
                timer.TimerEvent.AddListener(action);
            }

            if (runImmediately)
            {
                action.Invoke();
            }
        }

        public Canvas UICanvas
        {
            get
            {
                if (_canvas == null && gameObject)
                {
                    _canvas = gameObject.GetComponent<Canvas>();
                }

                return _canvas;
            }
        }

        public bool IsEnabled
        {
            get => isActiveAndEnabled;
            set => UISetActive(value);
        }

        public object[] @params { get; set; }

        public GameObject GameObject => gameObject;

        public string UIGameObjectName_Wolfgang
        {
            get
            {
                try
                {
                    if (gameObject != null)
                    {
                        return gameObject.name;
                    }
                }
                catch (Exception e)
                {
                    XLog.LogError("UIGameObjectName :" + e + "\n"+ e.StackTrace);
                    return "";
                }
                
                return "";
            }
        }

        private float _uiStartTime;

        private float _uiShowDuration;

        protected bool IsUseBtnClose;
        
        #region 生命周期函数

        public abstract void AfterStart();

        public Type ClassType => typeof(T);

        public virtual void InitReact(){}

        public virtual void AfterReact(){}

        public virtual void InitListener()
        {
            // AddEventListener(Franz_GlobalEvent_Kafka.change_orientation, (sender, eventArgs) =>
            // {
            //     RefreshCanvasScaler(true);
            // });
        }
        
        protected virtual void AfterInitEvents()
        {
            _uiStartTime = Time.time;
        }

        #endregion

        private static bool IsMatchHeight(float width, float height, bool changeWidthHeight = false)
        {
            //计算宽高比例  
            float standardAspect = width / height;
            float deviceAspect;
            var screenHeight = DeviceInfoUtils.Instance.ScreenHeight;
            var screenWidth = DeviceInfoUtils.Instance.ScreenWidth;
            if (changeWidthHeight)
            {
                deviceAspect = (float)screenHeight / screenWidth;
            }
            else
            {
                deviceAspect = (float)screenWidth / screenHeight;
            }
            
            float adjustor = 0;
            //计算矫正比例  
            if (deviceAspect < standardAspect)
            {
                adjustor = standardAspect / deviceAspect;
            }

            return adjustor == 0;
        }
        
        protected virtual void Awake()
        {
            XLog.LogColor("UI Awake =" + UIGameObjectName_Wolfgang, "blue");
            _dispatcher = MyEventDispatcher.New();

            RefreshCanvasScaler(UIOrientation is UIOrientation.Portrait);

            // var root = transform.Find("root");
            // if (root !=null)
            // {
            //     if (root.TryGetComponent<SafePadding>(out var safePadding))
            //     {
            //         safePadding.enabled = true;
            //     }
            // }
        }

        private void RefreshCanvasScaler(bool changeWidthHeight = false)
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            var referenceResolution = canvasScaler.referenceResolution;
            canvasScaler.matchWidthOrHeight = IsMatchHeight(referenceResolution.x, referenceResolution.y, changeWidthHeight) ? 1 : 0;
        }
        
        public bool IsStartEnd { get; private set; }

        protected virtual bool IsAutoPop => GetTableValue<bool>("isAutoPop");
        
        protected bool _isdestory;

        private MyNotchMain notchMain => MyNotchMain.Instance;
        
        //是否注册了
        protected bool IsRegisteredAdUI;
        
        protected void Start()
        {
            UIController.Instance.HideUIInStack(UIGameObjectName_Wolfgang);
            MediatorGame.Instance.DoOperation();
#if use_react
             vm ??= new Dictionary<string, RxProperty_Slim>();  
#endif
            David_LastOnTopTime_Hume = Time.time;
            AfterStart();

            InitReact();

            AfterReact();

            InitListener();
            IsStartEnd = true;

            AfterInitEvents();
            OnUIDisplay();

            if (UILayer is UILayer.NormalUI or UILayer.Window)
            {
                if (notchMain && notchMain.IsManualAutoSpin)
                {
                    notchMain.StopAutoSpin();
                }
            }

            var uiClassName = ClassType.Name;

            if (AdmobConfig.Instance.IsRewardAdUI(uiClassName))
            {
                AdManager.Instance.AddAllRewardAdCount(uiClassName);
                
                if (IsShouldRegisterUI())
                {
                    AdManager.Instance.RegisterPopAd(uiClassName);
                    if (ResConfig.Instance.IsBeginRecordAdClose() || RunTimeUtils.IsDebugCloseAdOpen)
                    {
                        IsRegisteredAdUI = true;
                    }
                }
        
            }
        }
        
        protected virtual bool IsShouldRegisterUI()
        {
            return IsAutoPop;
        }

        public void UISetActive(bool visible)
        {
            transform.SetActive(visible);
        }
        
        protected void AddListener_Dostoyevsky(string eventName, EventHandler handler)
        {
            if (_dispatcher == null)
            {
                _dispatcher = MyEventDispatcher.New();
            }

            _dispatcher.AddListener(eventName, handler);
        }

        protected void AddListener_Dostoyevsky(IEnumerable<string> eventNames, EventHandler handler)
        {
            foreach (var eventName in eventNames)
            {
                _dispatcher.AddListener(eventName, handler);
            }
        }
        
        public virtual void Refresh_Dostoyevsky()
        {
            try
            {
                //只用刷新所有叶子节点（有parent）
                InitReact();

                AfterReact();
            }
            catch (Exception e)
            {
                XLog.LogError($"ui refresh {e}\n{e.StackTrace}");
            }
        }

        protected T GetParamByOrder_Dostoyevsky<T>(int index)
        {
            if (@params == null || !@params.Any())
            {
                return default;
            }

            if (index < 0 || index >= @params.Length)
                return default;

            return @params[index] is T ? (T)@params[index] : default;
        }
        
        protected virtual void OnUIHide()
        {
            if (UILayer is UILayer.Window)
            {
                if (transform.childCount < 2)
                {
                    return;
                }

                transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                    .SetId(UIGameObjectName_Wolfgang);
            }
        }

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnUIDisplay()
        {
            if (UILayer is UILayer.Window)
            {
                if (transform.childCount < 2)
                {
                    return;
                }

                var root = transform.GetChild(1);
          
                root.localScale = 0.2f*  Vector3.one;
                DOTween.Sequence().Append(root.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack , 1.05f))
                    .Append(root.DOPunchScale(new Vector3(-0.03f,-0.03f, 0), 0.1f, 2, 1))
                    ;
            }
        }

        private const int by_click = 0;
        private const int auto_pop = 1;
        private const int dont_know = 2;
        private Dictionary<Type, int> recordDictionary = new Dictionary<Type, int>()
        {
            {typeof(FatherUIAddSpinCount), dont_know},
            {typeof(FatherUISavingpot), by_click},
            {typeof(FatherUIGreen), by_click},
            {typeof(FatherUISlotMachineUnlock), by_click},
            {typeof(FatherUIShowPayTable), by_click},
            {typeof(FatherUIPumpkinPop), by_click},
            {typeof(FatherUIDeleteAccount), by_click},
            {typeof(FatherUIFreeSpinsResult), auto_pop},
            {typeof(FatherUIFreeSpinWheel), auto_pop},
            {typeof(FatherUIFlopMoreChance), auto_pop},
            {typeof(FatherUIFlopWinResult), auto_pop},
            {typeof(FatherUIFlopFreeSpin1), auto_pop},
            //todo
            // {typeof(FatherUIFlopFreeSpin2), auto_pop},
            // {typeof(FatherUIFlopFreeSpin3), auto_pop},
            {typeof(FatherUIFlopCard), auto_pop},
            // {typeof(FatherUIFlopBonus), auto_pop},
            {typeof(FatherUIGetRewards), auto_pop},
            {typeof(FatherUIHaveUnlock), auto_pop},
            {typeof(FatherUIBigWin), auto_pop},
        };
        
        protected virtual void OnUIClose()
        {
            try
            {
                OnUIHide();

                bool isEnterAd = false;
                var uiClassName = ClassType.Name;

                bool isSpin5AdOpenedOrWillOpen = false;

                if (IsRegisteredAdUI)
                {
                    var isWatchAd = AdManager.Instance.IsWatchedOrNotRegisterAd(uiClassName);

                    if (!MyApp.Instance.IsAdControllerSystem)
                    {
                        if (!isWatchAd)
                        {
                            var curCount = MsgManager.Instance.AddValue(PersistKey.CloseRewardAdCount);

                            isSpin5AdOpenedOrWillOpen
                                = MediatorGame.Instance.IsSpin5AdWillPlay()
                                  //没有开启Spin5Buff广告 或者已经过了一轮了
                                  || (
                                      MyApp.Instance.Spin5AdPlayedAt > 0
                                      && MyApp.Instance.GodClass.HistorySpinCount <= MyApp.Instance.Spin5AdPlayedAt
#if !debug_mode
                && MyApp.Instance.IsSpin5AdOpen()
#endif
                                  );

                            if (!isSpin5AdOpenedOrWillOpen && curCount >= MediatorGame.Instance.GetCloseAdPopInterval())
                            {
                                isEnterAd = true;
                                RunTimeUtils.IsDebugCloseAdOpen = false;
                                MediatorGame.Instance.WatchAd(AdPlacement.RewardCloseAd, success =>
                                {
                                    if (success)
                                    {
                                        MsgManager.Instance.SetValue(PersistKey.CloseRewardAdCount, 0);
                                        MsgManager.Instance.AddValue(PersistKey.TodayPlayCloseAdCount);
                                    }
                                }, isShowTip: false);
                            }
                        }

#if debug_mode
                        if (!isEnterAd
                            && !isSpin5AdOpenedOrWillOpen
                            && RunTimeUtils.IsDebugCloseAdOpen
                           )
                        {
                            isEnterAd = true;
                            MediatorGame.Instance.WatchAd(AdPlacement.RewardCloseAd, success =>
                            {
                                if (success)
                                {
                                    MsgManager.Instance.SetValue(PersistKey.CloseRewardAdCount, 0);
                                    MsgManager.Instance.AddValue(PersistKey.TodayPlayCloseAdCount);
                                }
                            }, isShowTip: false);

                            RunTimeUtils.IsDebugCloseAdOpen = false;
                        }
#endif
                    }
                    else
                    {
                        if (!isWatchAd)
                        {
                            if (MediatorGame.Instance.IsPopNewCloseAdReward())
                            {
                                MediatorGame.Instance.WatchAd(AdPlacement.CloseRewardAdNew, success =>
                                {
                                    if (success)
                                    {
                                        AdManager.Instance.SetLastPopCloseAdAt();
                                        MsgManager.Instance.AddValue(PersistKey.TodayPlayCloseAdCount);
                                    }
                                }, isShowTip: false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
               XLog.LogError($"OnUIClose {e}\n {e.StackTrace}");
            }
        }

        public virtual void OnCloseBtnClick()
        {
            IsUseBtnClose = true;
            Close();
        }
        
        public virtual void Close()
        {
            if (this == null)
            {
                return;
            }
            
            OnUIClose();
            
            var tweenList = DOTween.TweensById(UIGameObjectName_Wolfgang);
            if (tweenList != null && tweenList.Any())
            {
                foreach (var tween in tweenList)
                {
                    if (!tween.IsActive())
                    {
                        continue;
                    }

                    tween.OnComplete(() => { CloseDirect(); });
                }
            }
            else
            {
                CloseDirect();
            }
        }

        protected void CloseDirect()
        {
            if (_isdestory)
            {
                return;
            }
            
            MediatorGame.Instance.DoOperation();
            
            if (_uiStartTime > 0)
            {
                _uiShowDuration = Time.time - _uiStartTime;
            }
            
            MyEventDispatcher.Root.Raise(GlobalEvent_Kafka.back_to_slotmachine_ui, ClassType);

            if (recordDictionary.TryGetValue(ClassType, out var pop_id))
            {
                bool isAutoPop = false;
                if (pop_id == by_click)
                {
                    isAutoPop = false;
                }
                else if (pop_id == auto_pop)
                {
                    isAutoPop = true;
                }
                else
                {
                    isAutoPop = IsAutoPop;
                }

                MsgManager.Instance.LogActivityPop(ClassType.ToString(), isAutoPop, _uiShowDuration, IsUseBtnClose ? 1 : 2);
            }

            UIController.Instance.RemoveUI(this);
            UIController.Instance.LastCloseUITime = Time.time;
            
            _isdestory = true;
            _intervalTimerMap = null;
            DisposeDispatcher_Friedrich();
            gameObject.Destroy();
        }

        private void Georg_UnLoadResource_Hegel()
        {
            if (UILayer is UILayer.NormalUI or UILayer.Window)
            {
                if (ResourceController.Instance != null)
                {
                    ResourceController.Instance.UnloadAssets();
                }
            }
        }
        
        void DisposeDispatcher_Friedrich()
        {
            if (_dispatcher != null)
                _dispatcher.Dispose();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            CloseDirect();
            // DOTween.Kill("UI_Close_Tween");
            Georg_UnLoadResource_Hegel();
        }
        
       //  /// <summary>
       //  /// uimain 使用， 会导致点穿
       //  /// </summary>
       // protected void ForbidSelfClick()
       //  {
       //      GetComponent<GraphicRaycaster>().enabled = false;
       //  }
       //  
       //  protected void ResumeSelfClick()
       //  {
       //      GetComponent<GraphicRaycaster>().enabled = true;
       //  }
        
        // public T FindChild<T>(string uri)
        // {
        //     return FindChild<T>(uri, null, true);
        // }
        //
        // public T FindChild<T>(string uri, Transform findTrans, bool raise_error = false)
        // {
        //     if (findTrans == null)
        //         findTrans = transform;
        //
        //     Transform trans = findTrans.FindChildX(uri, false, raise_error);
        //     if (trans == null)
        //     {
        //         return default(T);
        //     }
        //
        //     return trans.GetComponent<T>();
        // }
        
        public MyTable GetTable()
        {
            return GetParamByOrder_Dostoyevsky<MyTable>(0);
        }

        public T GetTableValue<T>(string key)
        {
            var table = GetTable();
            if (table == null)
            {
                return default;
            }
            return table[key] is T ? (T)table[key] : default;
        }

        // public bool IsUIOnTop()
        // {
        //     var uiWaitNet = UIController.That.Get<FatherUIWaitNet>();
        //
        //     if (uiWaitNet != null)
        //     {
        //         return false;
        //     }
        //
        //     var topUI = UIController.That.GetTopNormalUI();
        //     if (topUI == null)
        //     {
        //         return false;
        //     }
        //
        //     if (this == null)
        //     {
        //         return false;
        //     }
        //
        //     if (topUI.UIName != UIName)
        //     {
        //         return false;
        //     }
        //
        //     //ui要保持在前台一会
        //     if (lastOnTopTime > 0 && Time.time - lastOnTopTime > 0.2f)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }
        
        // public int SwitchClick { get; set; }

        public virtual UIOrientation UIOrientation { get; set; } = UIOrientation.Landscape;

        public float David_LastOnTopTime_Hume { get; set; }

        public virtual UILayer UILayer { get; set; } = UILayer.NormalUI;
    }
}