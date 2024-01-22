using CatLib;
using Core.Services.AudioService.API;
using Core.Services.PersistService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.ResourceService.Internal;
using DataAccess.Utils.Static;
using UnityEngine;
using YooAsset;

namespace Core.Services.AudioService
{
    public class AudioSystem : IAudioSystem
    {
        public AudioMonoDriver AudioMonoDriver { get; private set; }

        public AssetOperationHandle AssetHandle;
        
        internal void Init()
        {
            App.That.GetDispatcher()
                ?.AddListener(YooEvents.OnInitializeSuccess, (sender, args) =>
                {
                    InitVolumeSetting();
                });
        }


        private void FindAudioMono()
        {
            if (AudioMonoDriver != null)
                return;

            if (AssetHandle == null)
            {
                AssetHandle = YooAssets.LoadAssetSync("audiomono");
            }
            
            AudioMonoDriver = AssetHandle.InstantiateSync().GetComponent<AudioMonoDriver>();
        }

        public void InitVolumeSetting()
        {
            FindAudioMono();
            var soundVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.SOUND_VOLUME);
            var muteSound = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_SOUND);

            SetSoundVolume(muteSound ? GlobalEnum.MinPreSliderValue : soundVolume);

            var musicVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.MUSIC_VOLUME);
            var muteMusic = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_MUSIC);

            SetMusicVolume(muteMusic ? GlobalEnum.MinPreSliderValue : musicVolume);
        }

        public void PauseSound(SoundPack soundPack)
        {
            AudioMonoDriver.PauseSound(soundPack);
        }

        public void Resume(SoundPack soundPack)
        {
            AudioMonoDriver.Resume(soundPack);
        }

        public void StopSound(SoundPack soundPack)
        {
            AudioMonoDriver.Stop(soundPack);
        }

        internal void Reset()
        {
            if (AudioMonoDriver)
                Object.Destroy(AudioMonoDriver.gameObject);
        }

        public void PlaySound(string url)
        {
            AudioMonoDriver.PlaySound(url);
        }

        public void PlayMusic(string url)
        {
            FindAudioMono();
            AudioMonoDriver.PlayMusic(url);
        }

        public void PlaySound(AudioClip AudioClip)
        {
            AudioMonoDriver.PlaySound(AudioClip);
        }

        public void PlayMusic(AudioClip AudioClip)
        {
            AudioMonoDriver.PlayMusic(AudioClip);
        }

        public void PlaySound(SoundPack soundPack, float duration = 0, float delay = 0)
        {
            FindAudioMono();
            AudioMonoDriver.PlaySound(soundPack, duration, delay);
        }

        public void SetSoundVolume(float volume)
        {
            volume += GlobalEnum.SliderValueOffset;
            AudioMonoDriver.SetSoundVolume(volume);
        }

        public void SetMusicVolume(float volume)
        {
            volume += GlobalEnum.SliderValueOffset;
            AudioMonoDriver.SetMusicVolume(volume);
        }

        public void SetMasterVolume(float volume)
        {
            volume += GlobalEnum.SliderValueOffset;
            AudioMonoDriver.SetMaxterVolume(volume);
        }
    }
}