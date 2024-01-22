using CatLib;
using Core.Services.NetService.API;
using UnityEngine;

namespace Core.Services.NetService
{
    public class NetServiceProvider : MonoBehaviour, IServiceProvider
    {
        public void Init()
        {
            App.Make<INetSystem>();
        }

        public void Register()
        {
            App.Singleton<INetSystem, NetSystem>();
        }
    }
}