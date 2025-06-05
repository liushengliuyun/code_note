using UnityEngine;


namespace SlotX.Utils.Pool
{
    public class PoolObject : MonoBehaviour
    {
        public MyGameObjectPool Pool;

        public void Release()
        {
            Pool.Release(gameObject);
        }

        private void OnDestroy()
        {
            Pool.Clear();
        }
    }
}