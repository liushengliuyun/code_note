using System;
using Core.Services.UserInterfaceService.API;
using UnityEngine;
using YooAsset;

namespace Core.Services.UserInterfaceService.Internal
{
    public enum UIType
    {
        NormalUI,
        Window,
        Top,
        Overlay
    }

    public interface IUIBehaviour
    {
        /// <summary>
        /// 点击跳转到其他界面, 或者 然后回来; 切换tab + 1
        /// </summary>
        public int SwitchClick{ get; set; }
        
        public UIType uiType { get; set; }

        public Type ClassType { get; }

        public void InitVm();

        public void InitBinds();

        public void InitEvents();

        public void OnStart();

        public string UIName { get; }

        public void Close();

        public Canvas UICanvas { get; }

        public bool IsVisible { get; set; }

        object[] args { get; set; }
        GameObject GameObject { get; }
        void SetActive(bool b);
        void Refresh();
        
        //保留引用计数
        public AssetOperationHandle AssetHandle{ get; set; }  
    }
}