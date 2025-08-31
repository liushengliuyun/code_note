using System;
using UnityEngine;


namespace JerryMouse
{
    public enum UILayer
    {
        NormalUI,
        Window,
        Top,
        Overlay
    }

    public enum UIOrientation
    {
        Landscape, 
        Portrait
    }
    
    public interface IUIInterface
    {
        public UIOrientation UIOrientation { get; set; }
        
        public float David_LastOnTopTime_Hume { get; set; }
        
        public UILayer UILayer { get; set; }

        public Type ClassType { get; }

        public void InitReact();

        public void AfterReact();

        public void InitListener();

        public void AfterStart();

        public string UIGameObjectName_Wolfgang { get; }

        public void Close();

        public Canvas UICanvas { get; }

        public bool IsEnabled { get; set; }

        object[] @params { get; set; }
        GameObject GameObject { get; }
        void UISetActive(bool visible);
        void Refresh_Dostoyevsky();
    }
}