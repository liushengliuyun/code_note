using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Controller;
using DataAccess.Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    public class MuseumItemMono : MonoBehaviour
    {
        /// <summary>
        /// 进度
        /// </summary>
        public Text text;
        /// <summary>
        /// 大卡片有特殊背景
        /// </summary>
        public Image BG;
        /// <summary>
        /// 
        /// </summary>
        public Image Icon;
        
        public Image LockImage;

        public Transform GetLight;
        
        public MyButton BoxButton;
        public MyButton CollectBtn;
        public MyButton IconButton;

        private Transform TextGroup => text.transform.parent;
        
        private int order;

        public int Order
        {
            set
            {
                order = value;
                Init();
            }

            get => order;
        }

        private MuseumItem museumItem => Root.Instance.MuseumItems.Find(item => item.order == order);

        private void Init()
        {
            if (museumItem == null)
            {
                return;
            }

            var data = museumItem;
            
            Icon.sprite = data.DisplaySmall;

            BG.sprite = data.DisplayBG;
            
            //进度
            text.text = $@"{data.currentPoint}/{data.weight}";

            if (!data.IsBigCard)
            {
                // MediatorItem.Instance.SetItemText("smallCard", text);
            }
            else
            {
                // if (data.State == -1)
                // {
                //     MediatorItem.Instance.SetItemText("bigCardShow", text);
                // }
                //
                // if (data.State == 0)
                // {
                //     MediatorItem.Instance.SetItemText("bigCardMask", text);
                // } 
                
                LockImage.SetActive(data.State == 0);
            }
            
            var canGet = data.State == 1;
            BoxButton.SetActive(canGet);
            Icon.SetActive(!canGet);
            CollectBtn.SetActive(canGet);
            
            GetLight.SetActive(data.State != 0 && data.IsBigCard);

            TextGroup.SetActive(data.IsRowUnlock);
            BoxButton.SetClick(OnClick);
            CollectBtn.SetClick(OnClick);
            IconButton.SetClick(OnClick);
        }

        private void OnClick()
        {
            var item = museumItem;
            if (item == null)
            {
                return;
            }

            var state = item.State;
            switch (state)
            {
                //已领取
                case -1:
                    UserInterfaceSystem.That.ShowUI<UIMuseumDisplay>(item);
                    break;
                //解锁 未领取
                case 1:
                    //领奖
                    MediatorRequest.Instance.ClaimMuseumReward(order);
                    UserInterfaceSystem.That.ShowUI<UIMuseumDisplay>(item);
                    break;
                //未解锁
                case 0:
                    UserInterfaceSystem.That.ShowUI<UIMuseumDisplay>(item);
                    break;
            }
        }
    }
}