using UnityEngine;

public static class  创建一个RenderTexture
{
    ///这个RenderTexture可以保留深度信息和颜色信息
    /// CUIGraphic graphic 通过控制它的RefCurvesControlRatioPoints , 可以调节曲度
    public static void CreateRenderTexture()
    {
        RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
    }
    
}