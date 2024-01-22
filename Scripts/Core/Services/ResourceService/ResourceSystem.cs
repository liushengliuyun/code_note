using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CatLib;
using Core.Extensions;
using Core.Runtime.Game;
using Core.Services.ResourceService.API;
using Core.Services.ResourceService.Internal;
using Core.Services.ResourceService.Internal.UniPooling;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Core.Services.ResourceService
{
    public class ResourceSystem : IResourceSystem
    {
        public const string DefaultPackage = nameof(DefaultPackage);
        public const string BuiltinTag = "buildin";
        private IResourceSystem resourceSystemImplementation;
        private Dictionary<string, Type> prefabAndScritMaps = new Dictionary<string, Type>();
        public int SubVersion { set; get; }

        /// <summary>
        /// Editor资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode { get; set; } = EPlayMode.EditorSimulateMode;

        public bool IsHotfixEnabled =>
            PlayMode == EPlayMode.HostPlayMode;

        internal void Init()
        {
            initScriptBinds();
        }
        
        public ResourceSystem(MonoBehaviour appMain)
        {
            YooAssets.Initialize();
            UniPooling.Initalize();

            // CheckAppVersion();

            //第一次确定有这个数据
            if (App.CanMake<EPlayMode>())
            {
                //监听数据的变化
                App.Watch<EPlayMode>(v =>
                {
                    PlayMode = v;
                    appMain.StartCoroutine(InitPackage(PlayMode));
                });
            }
            else
            {
                appMain.StartCoroutine(InitPackage(PlayMode));
            }
        }


        private IEnumerator InitPackage(EPlayMode playMode)
        {
            while (true)
            {
                // var startNew = Stopwatch.StartNew();
                // 创建默认的资源包
                var package = YooAssets.TryGetPackage(DefaultPackage);
                if (package == null)
                {
                    package = YooAssets.CreatePackage(DefaultPackage);
                    YooAssets.SetDefaultPackage(package);
                }

                YZLog.LogColor(playMode, "orange");
                switch (playMode)
                {
                    // 编辑器下的模拟模式
                    case EPlayMode.EditorSimulateMode:
                    {
                        var createParameters = new EditorSimulateModeParameters
                        {
                            SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(DefaultPackage),
                            LocationToLower = true,
                            AssetLoadingMaxNumber = 10,
                        };
                        yield return package.InitializeAsync(createParameters);
                        break;
                    }
                    // 单机运行模式
                    case EPlayMode.OfflinePlayMode:
                    {
                        var createParameters = new OfflinePlayModeParameters
                        {
                            DecryptionServices = App.Make<IDecryptionServices>(),
                            LocationToLower = true,
                            AssetLoadingMaxNumber = 10,
                        };
                        yield return package.InitializeAsync(createParameters);
                        break;
                    }

                    // 联机运行模式
                    case EPlayMode.HostPlayMode:
                    {
                        var createParameters = new HostPlayModeParameters
                        {
                            DecryptionServices = App.Make<IDecryptionServices>(),
                            QueryServices = App.Make<IQueryServices>(),
                            LocationToLower = true,
                            //todo
                            DefaultHostServer = "http://127.0.0.1",
                            FallbackHostServer = "http://127.0.0.1",
                            AssetLoadingMaxNumber = 10,
                        };
                        yield return package.InitializeAsync(createParameters);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // startNew.Stop();
                // Debug.LogError($"package.GetPackageVersion() -> {package.GetPackageVersion()}, elapsed: {startNew.ElapsedMilliseconds}");

                YZLog.LogColor($"资源包状态={package.InitializeStatus}");
                if (package.InitializeStatus == EOperationStatus.Succeed)
                {
                    var downloader = package.CreateResourceDownloader(BuiltinTag, 10, 1);
                    if (downloader.TotalDownloadCount != 0)
                    {
                        YooAssets.ClearVersion(DefaultPackage);
                        Main.Instance.Restart();
                        yield break;
                    }

                    if (int.TryParse(package.GetPackageVersion(), out var v))
                    {
                        SubVersion = v;
                    }

                    YZLog.LogColor("资源包创建成功");
                    App.That.GetDispatcher().Raise(YooEvents.OnInitializeSuccess, this,
                        new InitializeSuccessEventArgs());

                    break;
                }

                YZLog.LogColor("资源包创建失败", "red");
                App.That.GetDispatcher().Raise(YooEvents.OnInitializeFailed, this,
                    new InitializeFailedEventArgs());

                yield return new WaitForSeconds(1);
            }
        }

        void initScriptBinds()
        {
            //获取在其中定义指定类型的当前加载的程序集。返回的程序集中的类型。
            Assembly assembly = Assembly.GetAssembly(typeof(BindPrefab));
            //得到该程序集种所有的Type类型，也就是得到该程序集下面所有共有的方法
            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                //Debug.Log("Type:" + type);

                //得到每个Type类型所拥有的特性
                foreach (Attribute attribute in Attribute.GetCustomAttributes(type, true))
                {
                    // Debug.Log("Attribute:" + attribute);
                    if (attribute is BindPrefab)
                    {
                        BindPrefab date = attribute as BindPrefab;
                        bindScript(date.path, type);
                    }
                }
            }
        }

        void bindScript(string path, Type type)
        {
            if (!prefabAndScritMaps.ContainsKey(path))
            {
                prefabAndScritMaps.Add(path, type);
            }
            else
            {
                Debug.LogError("当前数据这已经存在该路径" + path);
            }
        }

        Type getScriptType(string path)
        {
            if (prefabAndScritMaps.ContainsKey(path))
            {
                return prefabAndScritMaps[path];
            }
            else
            {
                // Debug.LogError("当前数据中不存在该路径");
                return null;
            }
        }

        public GameObject InstantiateGameObjSync(string location, Transform parent = null)
        {
            location = location.ToLower();
            var assetOperationHandle = YooAssets.LoadAssetSync(location);
            if (assetOperationHandle == null)
            {
                YZLog.LogColor($"InstantiateGameObjSync失败！ {location}", "red");
                return null;
            }

            using (assetOperationHandle)
            {
                var gameObject = assetOperationHandle.InstantiateSync(parent);
                var scriptType = getScriptType(location);
                if (gameObject != null && scriptType != null)
                {
                    if (gameObject.TryGetComponent(scriptType, out var script))
                    {
                        if (script == null)
                        {
                            gameObject.AddComponent(scriptType);
                        }
                        // else
                        // {
                        //     (script as MonoBehaviour)!.enabled = true;
                        // }
                    }
                }

                return gameObject;
            }
        }

        public T LoadAssetSync<T>(string location) where T : Object
        {
            var obj = YooAssets.LoadAssetSync<T>(location.ToLower());
            using (obj)
            {
                return obj.AssetObject as T;
            }
        }

        public AssetOperationHandle GetAssetHandle<T>(string location)where T : Object
        {
            return YooAssets.LoadAssetSync<T>(location.ToLower());
        }
        
        public AssetOperationHandle GetAssetHandle(string location)
        {
            return YooAssets.LoadAssetSync(location.ToLower());
        }
        
        /// <summary>
        /// 卸载引用计数为0的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            var package = YooAssets.TryGetPackage(DefaultPackage);
            package.UnloadUnusedAssets();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        public void ForceUnloadAllAssets()
        {
            var package = YooAssets.TryGetPackage(DefaultPackage);
            package.ForceUnloadAllAssets();
        }
    }
}