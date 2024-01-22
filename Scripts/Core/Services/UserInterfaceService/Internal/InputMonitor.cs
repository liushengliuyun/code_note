using System;
using CatLib.EventDispatcher;
using DataAccess.Utils.Static;
using UnityEngine;

namespace Core.Services.UserInterfaceService.Internal
{
    public class InputMonitor : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // 检测鼠标左键按下
            {
                EventDispatcher.Root.Raise(GlobalEvent.Player_Click_Down);
            }
        }

        public static bool HitTest(RectTransform target)
        {
            Vector2 mousePosition = Input.mousePosition; // 获取鼠标位置的屏幕坐标

            // 将屏幕坐标转换为UGUI坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, mousePosition, UserInterfaceSystem.Camera, out Vector2 localPoint);

            // 判断点击是否在按钮的范围内
            if (target.rect.Contains(localPoint))
            {
                // 点击到了按钮
                // 执行相应的操作
                return true;
            }

            return false;
        }
    }
}