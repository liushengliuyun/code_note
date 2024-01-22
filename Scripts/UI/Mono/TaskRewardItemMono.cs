using System;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Third.I18N;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    public class TaskRewardItemMono : MonoBehaviour

    {
        public bool IsTopReward;
        // Start is called before the first frame update
        [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
        [SerializeField] private Slider slider;
        [SerializeField] private Text ItemCountText;
        [SerializeField] private Image Icon;
        [SerializeField] private Transform lockTransform;
        [SerializeField] private Transform Tip;
        [SerializeField] private Transform ItemGroup;
        
        [SerializeField] private Image drop;
        
        public MyButton Btn;
        private bool isLock;
        private bool isPremium;

        [SerializeField] private Transform selectTrans;

        private Item item;

        private bool tipshow;

        public bool TipShow
        {
            set
            {
                tipshow = value;
                Tip.SetActive(value);
            }
            get => tipshow;
        }

        public Item Item
        {
            get => item;
            set
            {
                item = value;
                if (item.id is Const.Bonus or Const.Cash)
                {
                    ItemCountText.text = I18N.Get("key_money_count", item.Count);
                }
                else
                {
                    ItemCountText.text = item.Count.ToString();
                }

                Icon.sprite = item.GetIcon();
            }
        }

        public bool IsLock
        {
            get => isLock;
            set
            {
                lockTransform.SetActive(value);
                // ItemGroup.SetGray(value);
                isLock = value;

                if (drop != null)
                {
                    drop.transform.SetGray(value);
                }
            }
        }

        private bool isGet;

        public bool IsGet
        {
            get => isLock;
            set
            {
                // transform.SetAlpha(value ? 0.5f : 1);
                ItemGroup.SetGray(value);
                selectTrans.SetActive(value);
                isGet = value;
            }
        }

        public bool ShowProgress
        {
            set
            {
                horizontalLayoutGroup.padding.bottom = value ? 70 : IsTopReward ? 35 : 25;
                slider.SetActive(value);
            }
        }

        private int progress;

        public int Progress
        {
            set
            {
                slider.value = value;
                progress = value;
                ChangeSliderText();
            }
            get => progress;
        }

        void ChangeSliderText()
        {
            slider.gameObject.FindChild<Text>("slider Text").text = $"{Math.Min(progress, target)} / {target}";
        }

        private float target;

        public float Target
        {
            set
            {
                target = value;
                slider.maxValue = value;
                ChangeSliderText();
            }

            get => target;
        }
    }
}