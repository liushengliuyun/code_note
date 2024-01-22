using Core.Services.AudioService.API.Facade;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Extensions.UnityComponent
{
    public class ClickSoundMono : MonoBehaviour, IPointerClickHandler, IClickSound
    {
        [SerializeField] private bool clickSound = true;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private SoundPack soundPack;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayClickSound();
        }

        void PlayClickSound()
        {
            if (!ClickSound)
            {
                return;
            }

            if (AudioClip != null)
            {
                AudioSystem.That.PlaySound(AudioClip);
            }
            else
            {
                AudioSystem.That.PlaySound(SoundPack);
            }
        }
    }
}