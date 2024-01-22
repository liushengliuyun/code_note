using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UI.Mono
{
    public class SampleCameraFilter : MonoBehaviour
    {
        [NonSerialized] public RenderTexture rt;

        private void Awake()
        {
            if (rt == null)
            {
                //这样才能保留深度信息和颜色信息 
                rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            }

            TryGetComponent<Camera>(out var camera);
            if (camera != null)
            {
                camera.targetTexture = rt;
            }
        }
    
        private void OnDestroy()
        {
            if (rt == null)
            {
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}