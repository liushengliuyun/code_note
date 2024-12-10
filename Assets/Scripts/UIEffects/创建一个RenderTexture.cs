using UnityEngine;

public static class  创建一个RenderTexture
{
    public static void CreateRenderTexture()
    {
        //这个RenderTexture可以保留深度信息和颜色信息
        RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
    }
    
}