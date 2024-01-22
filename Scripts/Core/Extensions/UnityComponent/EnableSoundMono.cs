using System;
using Core.Services.AudioService.API.Facade;
using UnityEngine;

namespace Core.Extensions.UnityComponent
{
    public class EnableSoundMono: MonoBehaviour
    {
        public bool SoundEnable = true;
        
        public SoundPack EnableSound = SoundPack.None;
        
        public SoundPack DisableSound = SoundPack.None;
        
        private void OnEnable()
        {
            if (!SoundEnable)
            {
                return;
            }

            if (EnableSound == SoundPack.None)
            {
                return;
            }
            
            AudioSystem.That.PlaySound(EnableSound);
        }

        private void OnDisable()
        {
            if (!SoundEnable)
            {
                return;
            }
            
            if (EnableSound == SoundPack.None)
            {
                return;
            }
            
            AudioSystem.That.PlaySound(DisableSound);
        }
    }
}