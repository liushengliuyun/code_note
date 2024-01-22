using UnityEngine;
using YooAsset;

namespace Core.Services.ResourceService.API
{
    public interface IResourceSystem
    {
        GameObject InstantiateGameObjSync(string location, Transform parent = null);

        T LoadAssetSync<T>(string location) where T : Object;

        EPlayMode PlayMode { get; set; }

        public void UnloadUnusedAssets();

        /// <summary>
        /// 强制卸载所有资源
        /// </summary>
        public void ForceUnloadAllAssets();

        public AssetOperationHandle GetAssetHandle<T>(string location) where T : Object;
        
        public AssetOperationHandle GetAssetHandle(string location);
        
    }
}