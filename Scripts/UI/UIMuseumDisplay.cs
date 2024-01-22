using CatLib.EventDispatcher;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMuseumDisplay : UIBase<UIMuseumDisplay>
    {
        public Text UnlockDisplayText;
        
        public MyButton[] CloseBtns;

        public MyButton GotoBtn;

        public Transform GetPanel;

        public Transform UNGetPanel;

        public Text NameText;

        public Image[] DisplayIcons;

        public Image[] ItemIcons;

        public Text[] ItemTexts;

        public Text ProgressText;

        public Image MaskRototeImage;

        private MuseumItem museumItem;
        public override UIType uiType { get; set; } = UIType.Window;

        public EnableSoundMono EnableSoundMono;
        
        public override void OnStart()
        {
            museumItem = GetArgsByIndex<MuseumItem>(0);

            if (museumItem == null)
            {
                Close();
                return;
            }

            var state = museumItem.State;

            if (state == 1)
            {
                EnableSoundMono.enabled = true;
            }
            
            var get = state != 0;

            GetPanel.SetActive(get);
            UNGetPanel.SetActive(!get);

            foreach (var closeBtn in CloseBtns)
            {
                closeBtn.SetClick(OnCloseBtnClick);
            }

            NameText.text = museumItem.name;

            foreach (var displayIcon in DisplayIcons)
            {
                displayIcon.sprite = museumItem.DisplayBig;
            }


            foreach (var itemIcon in ItemIcons)
            {
                itemIcon.sprite = MediatorItem.Instance.GetItemSprite(museumItem.type);
            }


            foreach (var itemText in ItemTexts)
            {
                itemText.text = "x" + museumItem.amount;

                MediatorItem.Instance.SetItemText(museumItem.type, itemText);
            }

            GotoBtn.SetClick(OnGotoBtnClick);

            ProgressText.text = museumItem.currentPoint + "/" + museumItem.weight;

            ProgressText.transform.parent.SetActive(museumItem.IsRowUnlock);

            if (!get)
            {
                MaskRototeImage.sprite = GetRotateLight();
            }

            if (!museumItem.IsRowUnlock)
            {
                UnlockDisplayText.text = I18N.Get("key_museum_text_unlock");
            }
            else
            {
                UnlockDisplayText.text = I18N.Get("key_museum_text_lock");
            }
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
        }

        void OnGotoBtnClick()
        {
            //转到 roomList?
            EventDispatcher.Root.Raise(GlobalEvent.GO_TO_MAIN_ROOM_PAGE);
            Close();
        }


        Sprite GetRotateLight()
        {
            //已领取或者解锁未领取
            if (museumItem.State != 0)
            {
                return MediatorBingo.Instance.GetSpriteByUrl("museum/rotate_unlock"); 
            }

            if (museumItem.IsBigCard)
            {
                return MediatorBingo.Instance.GetSpriteByUrl("museum/rotate_bigcard"); 
            }
            
            return MediatorBingo.Instance.GetSpriteByUrl("museum/rotate_noraml");
        }
    }
}