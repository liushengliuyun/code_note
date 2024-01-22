using CatLib;
using CatLib.Container;
using Core.Services.AudioService.API;
using UnityEngine;

namespace Core.Services.AudioService
{
    public class AudioServiceProvider : MonoBehaviour, IServiceProvider
    {
        public void Init()
        {
            App.Make<IAudioSystem>();
        }

        public void Register()
        {
            App.Singleton<IAudioSystem, AudioSystem>()
                .OnAfterResolving<AudioSystem>(system => system.Init())
                .OnRelease<AudioSystem>(system => system.Reset());
        }
    }
}