using System;
using System.Collections.Generic;
using Carbon.Util;
using UniFramework.Pooling;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace Core.Services.ResourceService.Internal.UniPooling
{
    public interface IFromSpawner
    {
        Spawner Spawner { get; set; }
        string PoolKey { get; set; }
    }


    public static class SpawnerClassExtensions
    {
        //回收到自定义池子
        public static void Restore(this IFromSpawner obj)
        {
            return;
            if (obj == null)
            {
                CarbonLogger.LogError("对象为空");
                return;
            }

            if (!obj.Spawner.ClassPool.TryGetValue(obj.PoolKey, out var tuple))
            {
                obj.Spawner.ClassPool[obj.PoolKey] = tuple = (new List<IFromSpawner>(), 10);
            }

            if (tuple.list.Contains(obj))
            {
                return;
            }

            tuple.list.Add(obj);

            if (tuple.list.Count > tuple.capacity)
            {
                tuple.list.RemoveRange(tuple.capacity, tuple.list.Count - tuple.capacity);
            }
        }

        public static void Discard(this IFromSpawner obj)
        {
        }
    }

    public class Spawner
    {
        private readonly List<GameObjectPool> _gameObjectPools = new List<GameObjectPool>(100);
        private readonly List<GameObjectPool> _removeList = new List<GameObjectPool>(100);
        private readonly ResourcePackage _assetPackage;

        public GameObject SpawnerRoot { get; }

        public string PackageName
        {
            get { return _assetPackage.PackageName; }
        }


        private Spawner()
        {
        }

        internal Spawner(GameObject poolingRoot, ResourcePackage assetPackage)
        {
            SpawnerRoot = new GameObject($"{assetPackage.PackageName}");
            SpawnerRoot.transform.SetParent(poolingRoot.transform);
            _assetPackage = assetPackage;
        }

        /// <summary>
        /// 更新游戏对象池系统
        /// </summary>
        internal void Update()
        {
            _removeList.Clear();
            foreach (var pool in _gameObjectPools)
            {
                if (pool.CanAutoDestroy())
                    _removeList.Add(pool);
            }

            foreach (var pool in _removeList)
            {
                _gameObjectPools.Remove(pool);
                pool.DestroyPool();
            }
        }

        /// <summary>
        /// 销毁游戏对象池系统
        /// </summary>
        public void Destroy()
        {
            ClassPool.Clear();
            DestroyAll(true);

            UniPooling.GetSpawners().Remove(this);

            Object.Destroy(SpawnerRoot.gameObject);
        }

        /// <summary>
        /// 销毁所有对象池及其资源
        /// </summary>
        /// <param name="includeAll">销毁所有对象池，包括常驻对象池</param>
        public void DestroyAll(bool includeAll)
        {
            if (includeAll)
            {
                foreach (var pool in _gameObjectPools)
                {
                    pool.DestroyPool();
                }

                _gameObjectPools.Clear();
            }
            else
            {
                List<GameObjectPool> removeList = new List<GameObjectPool>();
                foreach (var pool in _gameObjectPools)
                {
                    if (pool.DontDestroy == false)
                        removeList.Add(pool);
                }

                foreach (var pool in removeList)
                {
                    _gameObjectPools.Remove(pool);
                    pool.DestroyPool();
                }
            }
        }


        /// <summary>
        /// 异步创建指定资源的游戏对象池
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="dontDestroy">资源常驻不销毁</param>
        /// <param name="initCapacity">对象池的初始容量</param>
        /// <param name="maxCapacity">对象池的最大容量</param>
        /// <param name="destroyTime">静默销毁时间（注意：小于零代表不主动销毁）</param>
        public CreatePoolOperation CreateGameObjectPoolAsync(string location, bool dontDestroy = false,
            int initCapacity = 0, int maxCapacity = int.MaxValue, float destroyTime = -1f)
        {
            return CreateGameObjectPoolInternal(location, dontDestroy, initCapacity, maxCapacity, destroyTime);
        }

        /// <summary>
        /// 同步创建指定资源的游戏对象池
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="dontDestroy">资源常驻不销毁</param>
        /// <param name="initCapacity">对象池的初始容量</param>
        /// <param name="maxCapacity">对象池的最大容量</param>
        /// <param name="destroyTime">静默销毁时间（注意：小于零代表不主动销毁）</param>
        public CreatePoolOperation CreateGameObjectPoolSync(string location, bool dontDestroy = false,
            int initCapacity = 0, int maxCapacity = int.MaxValue, float destroyTime = -1f)
        {
            var operation = CreateGameObjectPoolInternal(location, dontDestroy, initCapacity, maxCapacity, destroyTime);
            operation.WaitForAsyncComplete();
            return operation;
        }

        /// <summary>
        /// 创建指定资源的游戏对象池
        /// </summary>
        private CreatePoolOperation CreateGameObjectPoolInternal(string location, bool dontDestroy = false,
            int initCapacity = 0, int maxCapacity = int.MaxValue, float destroyTime = -1f)
        {
            if (maxCapacity < initCapacity)
                throw new Exception("The max capacity value must be greater the init capacity value.");

            GameObjectPool pool = TryGetGameObjectPool(location);
            if (pool != null)
            {
                // Debug.LogWarning($"GameObject pool is already existed : {location}");
                var operation = new CreatePoolOperation(pool.AssetHandle);
                YooAssets.StartOperation(operation);
                return operation;
            }
            else
            {
                pool = new GameObjectPool(SpawnerRoot, location, dontDestroy, initCapacity, maxCapacity, destroyTime);
                pool.CreatePool(_assetPackage);
                _gameObjectPools.Add(pool);

                var operation = new CreatePoolOperation(pool.AssetHandle);
                YooAssets.StartOperation(operation);
                return operation;
            }
        }


        public SpawnHandle SpawnAsync(string location)
        {
            return SpawnAsync(location, false);
        }

        /// <summary>
        /// 异步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnAsync(string location, bool forceClone, params System.Object[] userDatas)
        {
            return SpawnInternal(location, null, Vector3.zero, Quaternion.identity, forceClone, userDatas);
        }

        /// <summary>
        /// 异步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnAsync(string location, Transform parent, bool forceClone = false,
            params System.Object[] userDatas)
        {
            return SpawnInternal(location, parent, Vector3.zero, Quaternion.identity, forceClone, userDatas);
        }

        /// <summary>
        /// 异步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="position">世界坐标</param>
        /// <param name="rotation">世界角度</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnAsync(string location, Transform parent, Vector3 position, Quaternion rotation,
            bool forceClone = false, params System.Object[] userDatas)
        {
            return SpawnInternal(location, parent, position, rotation, forceClone, userDatas);
        }

        public SpawnHandle SpawnSync(string location)
        {
            return SpawnSync(location, false);
        }

        public GameObject SpawnGameObjSync(string location)
        {
            return SpawnSync(location, false).GameObj;
        }

        /// <summary>
        /// 同步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnSync(string location, bool forceClone, params System.Object[] userDatas)
        {
            SpawnHandle handle =
                SpawnInternal(location, null, Vector3.zero, Quaternion.identity, forceClone, userDatas);
            handle.WaitForAsyncComplete();
            return handle;
        }

        public GameObject SpawnGameObjSync(string location, bool forceClone, params System.Object[] userDatas)
        {
            return SpawnSync(location, forceClone, userDatas).GameObj;
        }

        /// <summary>
        /// 同步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnSync(string location, Transform parent, bool forceClone = false,
            params System.Object[] userDatas)
        {
            SpawnHandle handle =
                SpawnInternal(location, parent, Vector3.zero, Quaternion.identity, forceClone, userDatas);
            handle.WaitForAsyncComplete();
            return handle;
        }

        public GameObject SpawnGameObjSync(string location, Transform parent, bool forceClone = false,
            params System.Object[] userDatas)
        {
            return SpawnSync(location, parent, forceClone, userDatas).GameObj;
        }

        /// <summary>
        /// 同步实例化一个游戏对象
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="position">世界坐标</param>
        /// <param name="rotation">世界角度</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
        /// <param name="userDatas">用户自定义数据</param>
        public SpawnHandle SpawnSync(string location, Transform parent, Vector3 position, Quaternion rotation,
            bool forceClone = false, params System.Object[] userDatas)
        {
            SpawnHandle handle = SpawnInternal(location, parent, position, rotation, forceClone, userDatas);
            handle.WaitForAsyncComplete();
            return handle;
        }

        public GameObject SpawnGameObjSync(string location, Transform parent, Vector3 position, Quaternion rotation,
            bool forceClone = false, params System.Object[] userDatas)
        {
            return SpawnSync(location, parent, position, rotation, forceClone, userDatas).GameObj;
        }

        /// <summary>
        /// 实例化一个游戏对象
        /// </summary>
        private SpawnHandle SpawnInternal(string location, Transform parent, Vector3 position, Quaternion rotation,
            bool forceClone, params System.Object[] userDatas)
        {
            var pool = TryGetGameObjectPool(location);
            if (pool != null)
            {
                return pool.Spawn(parent, position, rotation, forceClone, userDatas);
            }

            // 如果不存在创建游戏对象池
            pool = new GameObjectPool(SpawnerRoot, location, false, 0, int.MaxValue, -1f);
            pool.CreatePool(_assetPackage);
            _gameObjectPools.Add(pool);
            return pool.Spawn(parent, position, rotation, forceClone, userDatas);
        }


        private GameObjectPool TryGetGameObjectPool(string location)
        {
            foreach (var pool in _gameObjectPools)
            {
                if (pool.Location == location)
                    return pool;
            }

            return null;
        }

        #region 扩展spawner

        internal readonly Dictionary<string, (List<IFromSpawner> list, int capacity)> ClassPool =
            new Dictionary<string, (List<IFromSpawner> list, int capacity)>();

        public void CreateClassPool<T>(int initCapacity = 0, int maxCapacity = int.MaxValue, string customExtend = null)
            where T : IFromSpawner, new()
        {
            var key = typeof(T).Name;
            if (!string.IsNullOrEmpty(customExtend))
            {
                key += customExtend;
            }

            if (ClassPool.TryGetValue(key, out var tuple)) return;
            ClassPool[key] = tuple = (new List<IFromSpawner>(), maxCapacity);

            for (var i = 0; i < Math.Min(initCapacity, maxCapacity); i++)
            {
                tuple.list.Add(new T
                {
                    Spawner = this,
                    PoolKey = key
                });
            }
        }

        //从自定义池子获取
        public IFromSpawner Spawn<T>(string customExtend = null) where T : IFromSpawner, new()
        {
            var key = typeof(T).Name;
            if (!string.IsNullOrEmpty(customExtend))
            {
                key += customExtend;
            }

            if (!ClassPool.TryGetValue(key, out var tuple))
            {
                CreateClassPool<T>(1, 10, customExtend);
            }

            tuple = ClassPool[key];

            if (tuple.list.Count > 0)
            {
                var o = tuple.list[tuple.list.Count - 1];
                tuple.list.RemoveAt(tuple.list.Count - 1);
                return o;
            }

            return new T
            {
                Spawner = this,
                PoolKey = key
            };
        }

        #endregion
    }
}