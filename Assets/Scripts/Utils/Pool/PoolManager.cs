using System.Collections.Generic;
using UnityEngine;


namespace SlotX.Utils.Pool
{
    public static class PoolManager
    {
        private static Dictionary<GameObject, MyGameObjectPool> _pools = new ();

        public static MyGameObjectPool GetPool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool;
            }
            else
            {
                var objectPool = new MyGameObjectPool(prefab);
                _pools[prefab] = objectPool;
                return objectPool;
            }
        }
    }
}