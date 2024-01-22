using System;
using BrunoMikoski.AnimationsSequencer;
using Coffee.UIEffects;
using Core.Extensions;
using DataAccess.Model;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    /// <summary>
    /// 道具的Mono容器
    /// </summary>
    public class BingoPropContainer : MonoBehaviour
    {
        /// <summary>
        /// 要保证是数据层的对象
        /// </summary>
        private Prop _prop;

        public Image PropPlat => gameObject.FindChild<Image>("PropPlat");

        private UIShiny Shiny => gameObject.FindChild<UIShiny>("PropIcon");

        private Image PropIcon => gameObject.FindChild<Image>("PropIcon");

        private AnimationSequence animationSequence => gameObject.FindChild<AnimationSequence>("PropIcon");

        private Transform crossEffect => transform.Find("eff_ui_cross");
        
        public Prop prop
        {
            get { return _prop; }
            set
            {
                if (_prop == null || _prop.id != value.id)
                {
                    PropIcon.sprite = value.sprite;
                    if (value != null)
                    {
                        /*有bug*/
                        // if (value.id != Const.DoubleScore)
                        // {
                        //     transform.localScale = Vector3.one; 
                        // }
                        // else
                        // {
                        //     transform.localScale = Vector3.one * 1.15f;
                        // }
                        
                        crossEffect.SetActive(value.id == Const.Cross);
                        PropIcon.SetNativeSize();
                    }
                }
                //为什么 always true？
                IsActive = value != null;
                _prop = value;
            }
        }

        public bool IsActive
        {
            get => PropIcon.IsActive();
            set
            {
                if (value)
                {
                    PropIcon.SetActive(true);
                    if (Shiny != null)
                    {
                        Shiny.Play();
                    }
                }
                else
                {

                    PropIcon.transform.HideUIByEffect(animationSequence, callback:() =>
                    {
                        crossEffect.SetActive(false);
                        PropIcon.SetActive(false);
                        //移动到最后
                        transform.SetAsLastSibling();
                    });
                }
            }
        }
    }
}