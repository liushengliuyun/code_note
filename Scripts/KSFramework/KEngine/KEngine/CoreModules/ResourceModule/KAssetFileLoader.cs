#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
//
// Filename: AssetFileLoader.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System;
using System.Collections;
using System.IO;
using Core.Extensions;
using Core.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KEngine
{
    /// <summary>
    /// 根據不同模式，從AssetBundle中獲取Asset或從Resources中獲取,两种加载方式同时实现的桥接类
    /// 读取一个文件的对象，不做拷贝和引用
    /// </summary>
    public class AssetFileLoader : AbstractResourceLoader
    {
        public delegate void AssetFileBridgeDelegate(bool isOk, Object resultObj);

        public Object Asset
        {
            get { return ResultObject as Object; }
        }

        public override float Progress
        {
            get
            {
                if (_bundleLoader != null)
                    return _bundleLoader.Progress;
                return 0;
            }
        }

        private AssetBundleLoader _bundleLoader;


        /// <summary>
        /// 编辑器模式下，对指定GameObject刷新一下Material
        /// </summary>
        public static void RefreshMaterialsShaders(Renderer renderer)
        {
            if (renderer.sharedMaterials != null)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.shader != null)
                    {
                        mat.shader = Shader.Find(mat.shader.name);
                    }
                }
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();

            if (_bundleLoader != null)
                _bundleLoader.Release(); // 释放Bundle(WebStream)

            //if (IsFinished)
            {
                if (!AppConfig.IsLoadAssetBundle)
                {
                    Resources.UnloadAsset(ResultObject as Object);
                }
                else
                {
                    //Object.DestroyObject(ResultObject as UnityEngine.Object);

                    // Destroying GameObjects immediately is not permitted during physics trigger/contact, animation event callbacks or OnValidate. You must use Destroy instead.
//                    Object.DestroyImmediate(ResultObject as Object, true);
                }

                //var bRemove = Caches.Remove(Url);
                //if (!bRemove)
                //{
                //    Log.Warning("[DisposeTheCache]Remove Fail(可能有两个未完成的，同时来到这) : {0}", Url);
                //}
            }
            //else
            //{
            //    // 交给加载后，进行检查并卸载资源
            //    // 可能情况TIPS：两个未完成的！会触发上面两次！
            //}
        }
    }
}