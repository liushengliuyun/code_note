using UniFramework.Pooling;
using UnityEngine;

namespace Core.Services.ResourceService.Internal.UniPooling
{
    public static class GameObjectSpawnHandleExtensions
    {
        public static void TryAddSpawnHandle(this GameObject gameObject, SpawnHandle h)
        {
            if (!gameObject)
                return;

            if (!gameObject.TryGetComponent<SpawnHandleHolder>(out var handle))
            {
                handle = gameObject.AddComponent<SpawnHandleHolder>();
            }

            handle.Handle = h;
        }

        public static void Restore(this GameObject gameObject)
        {
            // gameObject.Discard();
            // return;
            if (!gameObject)
                return;
            gameObject.SetActive(false);
            if (gameObject.TryGetComponent<SpawnHandleHolder>(out var handle))
            {
                handle.Restore();
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }

        public static void Discard(this GameObject gameObject)
        {
            if (!gameObject)
                return;

            if (gameObject.TryGetComponent<SpawnHandleHolder>(out var handle))
            {
                handle.Discard();
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }

        public static void Restore(this Transform transform)
        {
            // transform.Discard();
            // return;
            
            if (!transform)
                return;
            transform.SetActive(false);
            if (transform.TryGetComponent<SpawnHandleHolder>(out var handle))
            {
                handle.Restore();
            }
            else
            {
                Object.Destroy(transform.gameObject);
            }
        }

        public static void Discard(this Transform transform)
        {
            if (!transform)
                return;

            if (transform.TryGetComponent<SpawnHandleHolder>(out var handle))
            {
                handle.Discard();
            }
            else
            {
                Object.Destroy(transform.gameObject);
            }
        }
    }

    public class SpawnHandleHolder : MonoBehaviour
    {
        internal SpawnHandle Handle { get; set; }

        public void Restore()
        {
            Handle?.Restore();
            Handle = null;
        }

        public void Discard()
        {
            Handle?.Discard();
            Handle = null;
        }
    }
}