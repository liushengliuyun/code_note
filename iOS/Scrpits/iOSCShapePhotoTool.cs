using UnityEngine;
using System.Runtime.InteropServices;
using System;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.MyAttribute;
using DataAccess.Utils.Static;

namespace iOSCShape
{
    public class iOSCShapePhotoTool : YZBaseController<iOSCShapePhotoTool>
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ObjcStartPickerImageUnity(string param);
#endif

        private Action<Sprite, string> func;

        public void IOSYZStartPickerImage(YZPickerImageParam param, Action<Sprite, string> func)
        {
            this.func = func;
#if UNITY_IOS && !UNITY_EDITOR
            string json = JsonUtility.ToJson(param);
            ObjcStartPickerImageUnity(json);
#endif
        }

        [CallByIOS]
        public void CShapePickerImageFinish(string image_data)
        {
            if (!image_data.IsNullOrEmpty())
            {
                string base64 = image_data;
                byte[] bytes = Convert.FromBase64String(base64);
                Texture2D tex2D = new Texture2D(60, 60);
                tex2D.LoadImage(bytes);
                Sprite s = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                func?.Invoke(s, base64);
            }

            EventDispatcher.Root.Raise(GlobalEvent.Pick_Image_Finish);
        }

        [CallByIOS]
        public void iOSClickCamera()
        {
            EventDispatcher.Root.Raise(GlobalEvent.iOS_Click_Camera_Btn);
        }
        
    }
}