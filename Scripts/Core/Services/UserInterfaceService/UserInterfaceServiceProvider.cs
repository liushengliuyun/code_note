using CatLib;
using CatLib.Container;
using Core.Services.UserInterfaceService.API;
using UnityEngine;

namespace Core.Services.UserInterfaceService
{
    public class UserInterfaceServiceProvider : MonoBehaviour, IServiceProvider
    {
        public void Init()
        {
            App.Make<IUserInterfaceSystem>();
        }

        public void Register()
        {
            App.Singleton<IUserInterfaceSystem, UserInterfaceSystem>()
                .OnAfterResolving<UserInterfaceSystem>(Framework.Instance.AddUpdateService);
        }
    }
}