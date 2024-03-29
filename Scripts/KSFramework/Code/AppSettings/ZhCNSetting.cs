﻿#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Asset Bundle framework for Unity3D
// ===================================
// 
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

// 此代码由工具生成，请勿修改，如需扩展请编写同名Class并加上partial关键字

using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using Core.Services;
using Core.Services.ResourceService.Internal;
using KEngine;
using KEngine.Modules;
using TableML;

namespace AppSettings
{
    /// <summary>
    /// Auto Generate for Tab File: "zh_CN.tsv"
    /// Excel File: zh_CN.xlsx
    /// No use of generic and reflection, for better performance,  less IL code generating
    /// </summary>>
    public partial class ZhCNSettings : IReloadableSettings
    {
        /// <summary>
        /// How many reload function load?
        /// </summary>>
        public static int ReloadCount { get; private set; }

        public static readonly string[] TabFilePaths =
        {
            "zh_CN.tsv"
        };

        public static ZhCNSettings _instance = new ZhCNSettings();
        Dictionary<string, ZhCNSetting> _dict = new Dictionary<string, ZhCNSetting>();

        /// <summary>
        /// Trigger delegate when reload the Settings
        /// </summary>>
        public static System.Action OnReload;

        /// <summary>
        /// Constructor, just reload(init)
        /// When Unity Editor mode, will watch the file modification and auto reload
        /// </summary>
        private ZhCNSettings()
        {
        }

        /// <summary>
        /// Get the singleton
        /// </summary>
        /// <returns></returns>
        public static ZhCNSettings GetInstance()
        {
            if (ReloadCount == 0)
            {
                _instance._ReloadAll(true);
#if UNITY_EDITOR
                if (SettingModule.IsFileSystemMode)
                {
                    for (var j = 0; j < TabFilePaths.Length; j++)
                    {
                        var tabFilePath = TabFilePaths[j];
                        SettingModule.WatchSetting(tabFilePath, (path) =>
                        {
                            if (path.Replace("\\", "/").EndsWith(path))
                            {
                                _instance.ReloadAll();
                                YZLog.LogConsole_MultiThread("File Watcher! Reload success! -> " + path);
                            }
                        });
                    }
                }
#endif
            }

            return _instance;
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        /// <summary>
        /// Do reload the setting file: ZhCN, no exception when duplicate primary key
        /// </summary>
        public void ReloadAll()
        {
            _ReloadAll(false);
        }

        /// <summary>
        /// Do reload the setting class : ZhCN, no exception when duplicate primary key, use custom string content
        /// </summary>
        public void ReloadAllWithString(string context)
        {
            _ReloadAll(false, context);
        }

        /// <summary>
        /// Do reload the setting file: ZhCN
        /// </summary>
        void _ReloadAll(bool throwWhenDuplicatePrimaryKey, string customContent = null)
        {
            for (var j = 0; j < TabFilePaths.Length; j++)
            {
                var tabFilePath = TabFilePaths[j];
                TableFile tableFile;
                if (customContent == null)
                    tableFile = SettingModule.Get(tabFilePath, false);
                else
                    tableFile = TableFile.LoadFromString(customContent);

                using (tableFile)
                {
                    foreach (var row in tableFile)
                    {
                        var pk = ZhCNSetting.ParsePrimaryKey(row);
                        ZhCNSetting setting;
                        if (!_dict.TryGetValue(pk, out setting))
                        {
                            setting = new ZhCNSetting(row);
                            _dict[setting.Id] = setting;
                        }
                        else
                        {
                            if (throwWhenDuplicatePrimaryKey)
                                throw new System.Exception(string.Format(
                                    "DuplicateKey, Class: {0}, File: {1}, Key: {2}", this.GetType().Name, tabFilePath,
                                    pk));
                            else setting.Reload(row);
                        }
                    }
                }
            }

            if (OnReload != null)
            {
                OnReload();
            }

            ReloadCount++;
            YZLog.Info("Reload settings: {0}, Row Count: {1}, Reload Count: {2}", GetType(), Count, ReloadCount);
        }

        /// <summary>
        /// foreachable enumerable: ZhCN
        /// </summary>
        public static IEnumerable GetAll()
        {
            foreach (var row in GetInstance()._dict.Values)
            {
                yield return row;
            }
        }

        /// <summary>
        /// GetEnumerator for `MoveNext`: ZhCN
        /// </summary> 
        public static IEnumerator GetEnumerator()
        {
            return GetInstance()._dict.Values.GetEnumerator();
        }

        /// <summary>
        /// Get class by primary key: ZhCN
        /// </summary>
        public static ZhCNSetting Get(string primaryKey)
        {
            ZhCNSetting setting;
            if (GetInstance()._dict.TryGetValue(primaryKey, out setting)) return setting;
            return null;
        }

        // ========= CustomExtraString begin ===========

        // ========= CustomExtraString end ===========
    }

    /// <summary>
    /// Auto Generate for Tab File: "zh_CN.tsv" 
    /// Excel File: zh_CN.xlsx
    /// Singleton class for less memory use
    /// </summary>
    public partial class ZhCNSetting : TableRowFieldParser
    {
        /// <summary>
        /// 原文
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// 翻译
        /// </summary>
        public string Value { get; private set; }


        internal ZhCNSetting(TableFileRow row)
        {
            Reload(row);
        }

        internal void Reload(TableFileRow row)
        {
            Id = row.Get_string(row.Values[0], "int");
            Value = row.Get_string(row.Values[1], "");
        }

        /// <summary>
        /// Get PrimaryKey from a table row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string ParsePrimaryKey(TableFileRow row)
        {
            var primaryKey = row.Get_string(row.Values[0], "int");
            return primaryKey;
        }
    }
}