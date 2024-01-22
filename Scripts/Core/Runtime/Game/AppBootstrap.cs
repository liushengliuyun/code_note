using System.Collections;
using System.Linq;
using CatLib;
using Core.Controllers;
using Core.Extensions;
using Core.Services.ResourceService.Internal;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Utils;
using UnityEngine;
using YooAsset;
using Core.Runtime.Game.Config;
using DataAccess.Controller;
using DataAccess.Model;
using iOSCShape;
using Utils;
using Application = UnityEngine.Application;

namespace Core.Runtime.Game
{
    public class AppBootstrap : MonoBehaviour
    {
        /// <summary>
        /// Editor资源系统运行模式
        /// </summary>
        public EPlayMode editorPlayMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode playMode = EPlayMode.HostPlayMode;

        public bool ShouldResumeGame = true;

        // public bool AsVisitor = false;

        /// <summary>
        /// 加载的App
        /// </summary>
        [SerializeField] private string path = "";

        [SerializeField] private GameObject tempCanvas;

        private IEnumerator Start()
        {
            YZLog.LogColor("游戏开始");
            while (Framework.Instance && Framework.Instance.gameObject != null)
            {
                yield return null;
            }

            //锁定屏幕方向
            Screen.orientation = ScreenOrientation.Portrait;
            Application.targetFrameRate = 90;

            Instantiate(Resources.Load<GameObject>(path)).name = "App";
            YZLog.LogColor("App加载成功");
            var eventDispatcher = App.That.GetDispatcher();

            //todo 游戏入口
            //加载游戏语言
            var language = DeviceInfoUtils.Instance.GetLanguage();
            if (GameConfig.I18NLanguages.ToList().Contains(language))
            {
                GameConfig.LangId = language;
            }

            //Firebase.FirebaseApp app = null;
            //Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            //{
            //    var dependencyStatus = task.Result;
            //    if (dependencyStatus == Firebase.DependencyStatus.Available)
            //    {
            //        // Create and hold a reference to your FirebaseApp,
            //        // where app is a Firebase.FirebaseApp property of your application class.
            //        app = Firebase.FirebaseApp.DefaultInstance;
            //        // Set a flag here to indicate whether Firebase is ready to use by your app.
            //    }
            //    else
            //    {
            //        UnityEngine.Debug.LogError(System.String.Format(
            //            "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            //        // Firebase Unity SDK is not safe to use here.
            //    }
            //});

#if UNITY_ANDROID || UNITY_IOS
            YZSDKsController.Shared.SetupAppsFlyer();
            //YZSDKsController.Shared.PromptForPush();
#endif

            eventDispatcher?.AddListener(YooEvents.OnInitializeSuccess,
                (sender, args) =>
                {
                    if (tempCanvas == null)
                    {
                        tempCanvas = GameObject.Find("TempCanvas"); 
                    }
                
                    tempCanvas.Destroy();
                    
                    YZLog.LogColor("进入游戏界面 YZDefineUtil.IsDebugger" + YZDefineUtil.IsDebugger);
                    
                    // if (AsVisitor)
                    // {
                    //     //清除token     
                    //     PersistSystem.That.DeletePrefsValue(GlobalEnum.AUTHORIZATION);
                    // }

                    //添加红点事件注册

#if UNITY_IOS
                    var ipAddressByAmazon = YZGameUtil.GetIpAddressByAmazon();
                    Root.Instance.IP = ipAddressByAmazon;
#endif
                    
                    var redPointComponent = new GameObject("RedPointDriver");
                    redPointComponent.AddComponent<RedPointMonoDriver>();
#if !RTEST
                    if (Debug.isDebugBuild)
                    {
                        UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.GMLogin);
                        YZDebug.Log("测试包显示登录按钮");
                    }
                    else     
#endif
                    {
#if UNITY_EDITOR || UNITY_STANDALONE
                        MediatorRequest.Instance.PlayerLogin();
#endif
                        UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.NormalLogin);   
                        
                        YZDebug.Log("非测试包  不显示登录按钮");
                    }
                });

            void InitPlayMode()
            {
#if UNITY_EDITOR
                App.Instance<EPlayMode>(editorPlayMode);
#else
                App.Instance<EPlayMode>(playMode);
#endif
            }

            InitPlayMode();
            eventDispatcher?.AddListener(ApplicationEvents.OnStartCompleted, (sender, args) => InitPlayMode());

            yield return null;
            Destroy(gameObject);
        }
    }
}