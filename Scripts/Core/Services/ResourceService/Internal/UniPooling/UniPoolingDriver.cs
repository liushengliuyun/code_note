using UnityEngine;

namespace Core.Services.ResourceService.Internal.UniPooling
{
    internal class UniPoolingDriver : MonoBehaviour
    {
        void Update()
        {
            UniPooling.Update();
        }
    }
}