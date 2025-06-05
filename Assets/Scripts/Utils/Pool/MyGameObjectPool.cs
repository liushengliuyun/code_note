using JerryMouse.Extensions;
using UnityEngine;
using UnityEngine.Pool;

namespace SlotX.Utils.Pool
{
    public class MyGameObjectPool
    {
        private int defaultSize = 16;
        private const int ConstMaxSize = 100 ;
        private bool collectionCheck = true;

        private GameObject prefab;
        
        ObjectPool<GameObject> pool;
        
        // public int ActiveCount => pool.CountActive;
        //
        // public int InactiveCount => pool.CountInactive;
        //
        // public int TotalCount => pool.CountAll;
        
        public MyGameObjectPool(GameObject prefab)
        {
            this.prefab = prefab;
            pool = new ObjectPool<GameObject>(OnCreatePoolItem, OnGetPoolItem, OnReleasePoolItem, OnDestroyPoolItem, collectionCheck, defaultSize, ConstMaxSize);
        }

        GameObject OnCreatePoolItem() => Object.Instantiate(prefab);
        void OnGetPoolItem(GameObject obj) => obj.gameObject.SetActive(true);
        void OnReleasePoolItem(GameObject obj) => obj.gameObject.SetActive(false);
        void OnDestroyPoolItem(GameObject obj) => Object.Destroy(obj.gameObject);

        public GameObject Get()
        {
            GameObject gameObject = pool.Get();
            gameObject.TryAddComponent<PoolObject>().Pool = this;
            return gameObject;
        }

        public void Release(GameObject obj) => pool.Release(obj);
        public void Clear() => pool.Clear();
    }
}