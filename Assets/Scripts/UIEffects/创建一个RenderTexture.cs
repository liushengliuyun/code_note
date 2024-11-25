using UnityEngine;

public static class  创建一个RenderTexture
{
    public static void CreateRenderTexture()
    {
        RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
    }
    
}