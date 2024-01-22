using System;
using System.Collections.Generic;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.ResourceService.API.Facade;
using Core.Services.ResourceService.Internal.UniPooling;
using Core.Services.UserInterfaceService.API;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DG.Tweening;
using E7.NotchSolution;
using Reactive.Bindings;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityTimer;
using YooAsset;

// using Core.Services.UserInterfaceService.API.Facade;

namespace Core.Services.UserInterfaceService.Internal
{
    using UserInterfaceSystem = Core.Services.UserInterfaceService.API.Facade.UserInterfaceSystem;

    /// <summary>
    /// 所有UI的基类
    /// </summary>
    public abstract class UIBase<T> : MonoBehaviour, IUIBehaviour
    {
        protected Dictionary<string, ReactivePropertySlim> vm;

        protected EventDispatcher dispatcher;

        protected bool IsCloseing;

        private Canvas _canvas;

        /*protected Camera renderCamera
        {
            get
            {
                if (_canvas == null)
                {
                    return null;
                }

                return _canvas.worldCamera;
            }
        }*/

        public AssetOperationHandle AssetHandle { get; set; }

        class timer
        {
            public float LastTriggerTime;
            public UnityEvent TimerEvent;
        }

        private Dictionary<float, timer> intervalMap;

        private void Update()
        {
            if (intervalMap == null)
            {
                return;
            }

            foreach (var item in intervalMap)
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
        protected void RegisterInterval(float interval, UnityAction action, bool runImmediately = false)
        {
            intervalMap ??= new Dictionary<float, timer>();
            if (!intervalMap.TryGetValue(interval, out var timer))
            {
                var timeradd = new timer()
                {
                    TimerEvent = new UnityEvent()
                };
                timeradd.TimerEvent.AddListener(action);
                intervalMap[interval] = timeradd;
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

        public bool IsVisible
        {
            get => isActiveAndEnabled;
            set => SetActive(value);
        }

        public object[] args { get; set; }

        public GameObject GameObject => gameObject;

        public string UIName => gameObject != null ? gameObject.name : "";

        #region 生命周期函数

        //TODO PreShow（）

        public abstract void OnStart();

        public Type ClassType => typeof(T);

        public abstract void InitVm();

        public abstract void InitBinds();

        public abstract void InitEvents();

        private float startTime;

        protected float duration;

        protected bool closeByBtn;
        
        protected virtual void AfterInitEvents()
        {
            startTime = Time.time;
        }

        #endregion

        protected void Awake()
        {
            YZLog.LogColor("UI Awake =" + UIName, "blue");
            dispatcher = new EventDispatcher();
            dispatcher.SetParent(EventDispatcher.Root);

            GetComponent<CanvasScaler>().matchWidthOrHeight = UserInterfaceSystem.That.IsMatchHeight ? 1 : 0;

            var root = transform.Find("root");
            if (root !=null)
            {
                if (root.TryGetComponent<SafePadding>(out var safePadding))
                {
                    safePadding.enabled = true;
                }
            }
        }

        /*private void OnEnable()
        {
        }*/

        [NonSerialized]
        public bool IsInitEnd;

        protected void Start()
        {
            UserInterfaceSystem.That.HideUIStack(UIName);
            vm ??= new Dictionary<string, ReactivePropertySlim>();
            OnStart();

            InitVm();

            InitBinds();

            InitEvents();
            IsInitEnd = true;

            AfterInitEvents();
            OnAnimationIn();
        }

        public void AddEventListener(string eventName, EventHandler handler)
        {
            if (dispatcher == null)
            {
                dispatcher = new EventDispatcher();
                dispatcher.SetParent(EventDispatcher.Root);
            }

            dispatcher.AddListener(eventName, handler);
        }

        public void AddEventListener(IEnumerable<string> eventNames, EventHandler handler)
        {
            foreach (var eventName in eventNames)
            {
                dispatcher.AddListener(eventName, handler);
            }
        }

        public void SetActive(bool b)
        {
            transform.SetActive(b);
        }

        protected T GetArgsByIndex<T>(int index)
        {
            if (args == null || !args.Any())
            {
                return default;
            }

            if (index < 0 || index >= args.Length)
                return default;

            return args[index] is T ? (T)args[index] : default;
        }

        public virtual void Refresh()
        {
            if (vm == null)
            {
                return;
            }

            //只用刷新所有叶子节点（有parent）
            InitVm();

            InitBinds();

            // foreach (var item in vm)
            // {
            //     if (!item.Value.HaveChild())
            //     {
            //         item.Value.Refresh();
            //     }
            // }
        }


        protected TValue GetVmValue<TValue>(string key, TValue backward, out bool success)
        {
            if (vm.ContainsKey(key))
            {
                success = true;
                return vm[key].ToIObservable<TValue>().Value;
            }
            else
            {
                success = false;
                return backward;
            }
        }

        protected TValue GetVmValue<TValue>(string key, out bool success)
        {
            TValue result;
            if (vm.ContainsKey(key))
            {
                success = true;
                result = vm[key].ToIObservable<TValue>().Value;
            }
            else
            {
                success = false;
                result = default;
            }

            return result;
        }
        
        protected void SetVmValue<TValue>(string key, TValue value, out bool success)
        {
            if (vm.ContainsKey(key))
            {
                success = true;
                vm[key].ToIObservable<TValue>().Value = value;
            }
            else
            {
                success = false;
            }

        }
        
        protected void SetVmValue<TValue>(string key, ref TValue backward, TValue value, out bool success)
        {
            if (vm.ContainsKey(key))
            {
                success = true;
                vm[key].ToIObservable<TValue>().Value = value;
            }
            else
            {
                backward = value;
                success = false; 
            }
        }
        
        protected virtual void OnAnimationIn()
        {
            if (uiType is UIType.Window)
            {
                if (transform.childCount < 2)
                {
                    return;
                }

                transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
                transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        }

        protected virtual void OnAnimationOut()
        {
            if (uiType is UIType.Window)
            {
                if (transform.childCount < 2)
                {
                    return;
                }

                transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                    .SetId(UIName);
                
                // DOTween.Sequence().SetDelay(0.1f).SetId(UIName);
                //
                // UIEffectUtils.Instance.CaptureAndDoSomething(transform, UICanvas);
            }
        }

        protected virtual void OnClose()
        {
            OnAnimationOut();
        }

        public virtual void OnCloseBtnClick()
        {
            closeByBtn = true;
            Close();
        }
        
        public virtual void Close()
        {
            if (this == null)
            {
                return;
            }

            if (startTime > 0)
            {
                duration = Time.time - startTime;
            }
            
            IsCloseing = true;

            OnClose();
            
            var tweenList = DOTween.TweensById(UIName);
            if (tweenList != null && tweenList.Any())
            {
                foreach (var tween in tweenList)
                {
                    if (!tween.IsActive())
                    {
                        continue;
                    }

                    tween.OnComplete(() => { BeforeDestroy(); });
                }
            }
            else
            {
                BeforeDestroy();
            }
        }

        private void BeforeDestroy()
        {
            UserInterfaceSystem.That.RemoveUI(this);
            intervalMap = null;
            RemoveAllListener();
            gameObject.Discard();
        }

        private void UnLoadAsset()
        {
            AssetHandle?.Release();
            AssetHandle = null;
            if (uiType is UIType.NormalUI or UIType.Window)
            {
                ResourceSystem.That.UnloadUnusedAssets();
            }
        }


        public void LoadSpine(Transform parent, out AssetOperationHandle AssetOperationHandle, float scale = 1,
            Vector3 position = default)
        {
            MediatorBingo.Instance.LoadSpine(parent, out AssetOperationHandle, scale, position);
        }

        protected void OnDestroy()
        {
            // DOTween.Kill("UI_Close_Tween");
            UnLoadAsset();
        }

        void RemoveAllListener()
        {
            if (dispatcher != null)
                dispatcher.Dispose();
        }

        /// <summary>
        /// uimain 使用， 会导致点穿
        /// </summary>
       protected void ForbidSelfClick()
        {
            transform.GetComponent<GraphicRaycaster>().enabled = false;
        }
        
        protected void ResumeSelfClick()
        {
            transform.GetComponent<GraphicRaycaster>().enabled = true;
        }
        
        public T FindChild<T>(string uri)
        {
            return FindChild<T>(uri, null, true);
        }

        public T FindChild<T>(string uri, Transform findTrans, bool raise_error = false)
        {
            if (findTrans == null)
                findTrans = transform;

            Transform trans = findTrans.FindChildX(uri, false, raise_error);
            if (trans == null)
            {
                return default(T);
            }

            return trans.GetComponent<T>();
        }

        public static void ForbidClick()
        {
            FindObjectOfType<EventSystem>().enabled = false;
        }

        public static void ResumeClick()
        {
            FindObjectOfType<EventSystem>().enabled = true;
        }

        public GameData GetTable()
        {
            return GetArgsByIndex<GameData>(0);
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
        
        public int SwitchClick { get; set; }
        
        public virtual UIType uiType { get; set; } = UIType.NormalUI;
    }
}