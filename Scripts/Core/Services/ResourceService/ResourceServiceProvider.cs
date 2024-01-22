using CatLib;
using CatLib.Container;
using Core.Services.ResourceService.API;
using UnityEngine;
using YooAsset;

namespace Core.Services.ResourceService
{
    public class ResourceServiceProvider : MonoBehaviour, IServiceProvider
    {
        public void Init()
        {
            App.Make<IResourceSystem>();
        }

        public void Register()
        {
            App.Singleton<IDecryptionServices, GameDecryptionServices>();
            App.Singleton<IResourceSystem, ResourceSystem>()
                .OnAfterResolving<ResourceSystem>(system => system.Init());
        }
    }
}