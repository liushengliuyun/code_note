using System;
using System.Collections.Generic;
using Core.Extensions;
using KEngine;

using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/9 
/// Desc：ab加载管理器
///	关于图集(SpriteAtlas)的建议:
///		1.物品，称号，技能图标等不放在图集中，使用单独的图片 
///		2.每个UIPrefab界面(系统)一个图集
/// </summary>
public class ABManager
{
    /// <summary>
    /// type -> <url,loader>
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> _loadersPool =
        new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();

    /// <summary>
    /// todo 等待加载中的ab
    /// </summary>
    private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> waiting =
        new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();

    public static int MAX_LOAD_NUM = 10;

    public static int GetCount<T>()
    {
        return GetTypeDict(typeof(T)).Count;
    }

    public static Dictionary<string, AbstractResourceLoader> GetTypeDict(Type type)
    {
        Dictionary<string, AbstractResourceLoader> typesDict;
        if (!_loadersPool.TryGetValue(type, out typesDict))
        {
            typesDict = _loadersPool[type] = new Dictionary<string, AbstractResourceLoader>();
        }

        return typesDict;
    }

    public static int GetRefCount<T>(string url)
    {
        var dict = GetTypeDict(typeof(T));
        AbstractResourceLoader loader;
        if (dict.TryGetValue(url, out loader))
        {
            return loader.RefCount;
        }

        return 0;
    }

    #region 对外接口

    #endregion

    public static Dictionary<string, SpriteAtlas> SpriteAtlases = new Dictionary<string, SpriteAtlas>();

    public static void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {
        SpriteAtlas atlas = null;
        if (SpriteAtlases.TryGetValue(tag, out atlas))
        {
            if (atlas != null)
            {
                if (Application.isEditor) YZLog.Info($"Request spriteAtlas {tag}");
                callback(atlas);
            }
            else
            {
                SpriteAtlases.Remove(tag);
                YZLog.LogError($"not load spriteAtlas {tag}");
            }
        }
        else
        {
            YZLog.LogError($"not load spriteAtlas {tag}");
        }
    }
}