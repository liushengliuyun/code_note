using UnityEngine;

namespace Core.Services.AudioService.API
{
    public interface IAudioSystem
    {
        void PlaySound(string url);

        void PlayMusic(string url);

        void PlaySound(AudioClip clip);

        void PlayMusic(AudioClip clip);

        void PlaySound(SoundPack soundPack, float duration = 0, float delay = 0);

        void SetSoundVolume(float volume);

        void SetMusicVolume(float volume);

        void SetMasterVolume(float volume);

        void InitVolumeSetting();

        public void PauseSound(SoundPack soundPack);

        public void Resume(SoundPack soundPack);

        public void StopSound(SoundPack soundPack);
    }
}