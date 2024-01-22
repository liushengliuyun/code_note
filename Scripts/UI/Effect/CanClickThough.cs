using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effect
{
    /// <summary>
    /// 使图片透明部分可点穿
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CanClickThough : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        }
    }
}