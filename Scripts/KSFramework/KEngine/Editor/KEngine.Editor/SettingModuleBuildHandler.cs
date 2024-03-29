﻿#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
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
using UnityEngine;
using System.IO;
using KEngine;
using KUnityEditorTools;
using UnityEditor;
using UnityEditor.Callbacks;

namespace KSFramework.Editor
{
    /// <summary>
    /// Build App阶段，进行处理的钩子
    /// </summary>
    [InitializeOnLoad]
    public class SettingModuleBuildHandler
    {
        /// <summary>
        /// 标记是否重复执行BeforeBuildApp
        /// </summary>
        private static bool _hasBeforeBuildApp = false;

        /// <summary>
        /// 复制文件事件, 可以进行加密行为
        /// </summary>
        public static Action<string> OnCopyFile;

        static SettingModuleBuildHandler()
        {
            //目前是通过link方式，且没有加密配置表，所以不copy
            // KUnityEditorEventCatcher.OnBeforeBuildAppEvent -= OnPostProcessScene; 
            // KUnityEditorEventCatcher.OnBeforeBuildAppEvent += OnPostProcessScene;
        }

        /// <summary>
        /// 完成Scene后，编译DLL后，未出APK前
        /// </summary>
        //[PostProcessScene]
        private static void OnPostProcessScene()
        {
            if (!_hasBeforeBuildApp && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _hasBeforeBuildApp = true;
                // 这里是编译前, Setting目录的配置文件拷贝进去StreamingAssetse
                
                if (Directory.Exists(AppConfig.ExportTsvPath) == false)
                {
                    Debug.LogWarning(string.Format("[SettingModuleBuildHandler]DirectoryNotFound: {0}", AppConfig.ExportTsvPath));
                    return;
                }
                Debug.Log("[SettingModuleBuildHandler]Start copy settings...");
                var luaCount = 0;

                var toDir = "Assets/StreamingAssets/" + AppConfig.SettingResourcesPath; // 文件夹名称获取

                //拷贝所有的tsv到StreamingAssets
                var allFiles = Directory.GetFiles(AppConfig.ExportTsvPath, "*", SearchOption.AllDirectories);
                foreach (var path in allFiles)
                {
                    var cleanPath = path.Replace("\\", "/");

                    var relativePath = cleanPath.Replace(AppConfig.ExportTsvPath + "/", "");
                    var toPath = Path.Combine(toDir, relativePath);

                    if (!Directory.Exists(Path.GetDirectoryName(toPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(toPath));

                    File.Copy(cleanPath, toPath, true);
                    if (OnCopyFile != null)
                        OnCopyFile(toPath);
                    luaCount++;
                }
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log(string.Format("[SettingModuleBuildHandler]compile setting file count: {0}", luaCount));
            }
        }

        // /// <summary>
        // /// 构建完成,恢复标记
        // /// </summary>
        // [PostProcessBuild()]
        // static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        // {
        //     _hasBeforeBuildApp = false;
        // }
    }

}
