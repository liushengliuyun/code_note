using System;
using CatLib;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;

namespace Core.Services.UserInterfaceService.API
{
    public interface IUserInterfaceSystem : IUpdate , IReset
    {
        public GameObject EffectPanel { get; }

        public T Get<T>() where T : UIBase<T>;

        public IUIBehaviour GetTopNormalUI();
        
        void ShowUI<T>(params object[] args) where T : UIBase<T>;
        
        // void PreLoadUI<T>() where T : UIBase<T>;
           
        void ShowQueue<T>(Func<bool> condition, params object[] args) where T : UIBase<T>;
        
        void ShowQueue<T>(params object[] args) where T : UIBase<T>;
        
        /// <summary>
        /// 插入到显示队列的第一个
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        void TopInQueue<T>(params object[] args) where T : UIBase<T>;
        
        bool HaveUIInQueue();
        
        void SingleTonQueue<T>(params object[] args) where T : UIBase<T>;

        public void SingleTonQueue<T>(Func<bool> condition, params object[] args) where T : UIBase<T>;
        
        bool IsMatchHeight { get; }

        void RemoveUI(IUIBehaviour ui);

        void RemoveUIByName(string uiName);

        public void HideUIStack(string uiName);

        public void CloseAllUI(string[] excludeList = null);

        /// <summary>
        /// 通常都是单例显示， 这个接口可以显示多例
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void ShowAnotherUI<T>(params object[] args) where T : UIBase<T>;

        public void RemoveUIInQueue<T>() where T : UIBase<T>;
        
        public void RemoveAllQueue();
        
    }
}