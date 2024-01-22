using System;
using Core.Services.AudioService.API.Facade;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Core.Extensions.UnityComponent
{
    public class MyButton : Button, IClickSound
    {
        [SerializeField] private bool clickSound = true;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private SoundPack soundPack;

        private bool _gray;

        public bool Gray
        {
            get => _gray;
            set
            {
                if (_gray != value)
                {
                    transform.SetGray(value);
                    _gray = value;
                }
            }
        }

        public bool ClickSound
        {
            get => clickSound;
            set => clickSound = value;
        }

        public AudioClip AudioClip
        {
            get => audioClip;
            set => audioClip = value;
        }

        public SoundPack SoundPack
        {
            get => soundPack;
            set => soundPack = value;
        }

        private bool AddedClickSound;

        private Text titleCom;

        public string title
        {
            set
            {
                titleCom ??= transform.Find("Title")?.GetComponent<Text>() ??
                             transform.Find("title")?.GetComponent<Text>();
                if (titleCom != null)
                {
                    titleCom.text = value;
                }
                else
                {
                    YZLog.LogColor("未找到title " + gameObject.name, "red");
                }
            }
        }

        void OnEnable()
        {
            base.OnEnable();
            AddClickSound();
        }

        void AddClickSound()
        {
            if (AddedClickSound)
            {
                return;
            }

            if (ClickSound)
            {
                AddedClickSound = true;
                onClick.AddListener(PlayClickSound);
            }
        }

        void CancelClickSound()
        {
            if (ClickSound)
            {
                AddedClickSound = false;
                onClick?.RemoveListener(PlayClickSound);
            }
        }

        void PlayClickSound()
        {
            if (AudioClip != null)
            {
                AudioSystem.That.PlaySound(AudioClip);
            }
            else
            {
                AudioSystem.That.PlaySound(soundPack);
            }
        }

        public void SetClick(UnityAction action)
        {
            onClick.RemoveAllListeners();
            AddedClickSound = false;
            onClick.AddListener(action);
            AddClickSound();
        }


        private static UnityAction hold = () => {};
        
        public void ClearClick()
        {
            SetClick(hold);
        }
        
        void OnDisable()
        {
            base.OnDisable();
            CancelClickSound();
        }
    }
}