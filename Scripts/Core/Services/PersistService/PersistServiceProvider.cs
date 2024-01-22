using CatLib;
using Core.Services.PersistService.API;
using UnityEngine;

namespace Core.Services.PersistService
{
    public class PersistServiceProvider : MonoBehaviour, IServiceProvider
    {
        public void Init()
        {
            App.Make<IPersistSystem>();
        }

        public void Register()
        {
            App.Singleton<IPersistSystem, PersistSystem>();
        }
    }
}