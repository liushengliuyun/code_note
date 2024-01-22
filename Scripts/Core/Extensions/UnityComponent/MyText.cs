using System;
using Core.Third.I18N;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.Extensions.UnityComponent
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2021/5/27 15:02
    /// Desc：扩展Text，支持多语言
    /// </summary>
    [Serializable]
    public class MyText : Text
    {
        [SerializeField] public bool UseLangId = false;
        [SerializeField] public string LangId;
        [SerializeField] public string[] LangParams;
        [SerializeField] public bool IsUpper;
        [SerializeField] public bool useMyAdjust;
        [SerializeField] public float maxSize;
        [SerializeField] public float minSize = 1;

        public override string text
        {
            get
            {
                return base.text;
            }
            set
            {
                base.text = value;
                AdjustTextSize();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AdjustTextSize();
        }


#if UNITY_EDITOR
        /// <summary>
        /// 监听编辑器的改动
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            AdjustTextSize();
        }
#endif

        void AdjustTextSize()
        {
            if (useMyAdjust)
            {
                float textLength = preferredWidth;

                // Adjust the font size based on the text length
                float newSize = Mathf.Clamp(fontSize * (rectTransform.rect.width / textLength), minSize, maxSize);
                fontSize = Convert.ToInt32(Math.Floor(newSize));
            }
        }
        
        protected override void Awake()
        {
            if (UseLangId)
            {
                if (!string.IsNullOrEmpty(LangId))
                {
                    if (IsUpper)
                    {
                        text = I18N.Get(LangId, LangParams).ToUpper();
                    }
                    else
                    {
                        text = I18N.Get(LangId, LangParams);
                    }
                }
                else
                {
                    // Log.LogError($"{GameUtils.GetRootPathName(this.transform)},lang id is null");
                }
            }
        }
    }
}