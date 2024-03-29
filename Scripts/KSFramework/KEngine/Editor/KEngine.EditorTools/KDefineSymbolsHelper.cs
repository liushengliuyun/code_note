﻿#region Copyright(c) Kingsoft Xishanju

// Company: Kingsoft Xishanju
// Filename: KDefineSymbolsHelper.cs
// Date:     2015/11/07
// Author:   Kelly / chenpeilin1
// Email: chenpeilin1@kingsoft.com / 23110388@qq.com

#endregion

using System.Collections.Generic;
using Core.Editor;
using Core.Extensions;
using Core.Services;
using UnityEditor;
using UnityEngine;

namespace KUnityEditorTools
{
    /// <summary>
    /// 用于预编译指令（宏）的增删查
    /// </summary>
    public class KDefineSymbolsHelper
    {
        /// <summary>
        /// 是否有指定宏呢
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool HasDefineSymbol(string symbol)
        {
            var symbolStrs =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols =
                new List<string>(symbolStrs.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            return symbols.Contains(symbol);
        }

        /// <summary>
        /// 移除指定宏
        /// </summary>
        /// <param name="symbol"></param>
        public static void RemoveDefineSymbols(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                if (Application.isEditor) YZLog.LogError("cant add null");
                return;
            }

            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                List<string> symbols =
                    new List<string>(symbolStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                if (symbols.Contains(symbol))
                    symbols.Remove(symbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
            }
        }

        /// <summary>
        /// 添加指定宏（不重复）
        /// </summary>
        /// <param name="symbol">宏</param>
        public static void AddDefineSymbols(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                if (Application.isEditor) YZLog.LogError("cant add null");
                return;
            }

            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                List<string> symbols =
                    new List<string>(symbolStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                if (!symbols.Contains(symbol))
                {
                    symbols.Add(symbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
                }
            }
        }

        public static void AddDefineSymbols(string[] symbol)
        {
            if (symbol == null)
            {
                if (Application.isEditor) YZLog.LogError("cant add null");
                return;
            }

            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                List<string> symbols =
                    new List<string>(symbolStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                foreach (string t in symbol)
                {
                    if (!symbols.Contains(t))
                        symbols.Add(t);
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols.ToArray()));
            }
        }

        /// <summary>
        /// 设置为只有指定宏
        /// </summary>
        /// <param name="symbols"></param>
        public static void SetDefineSymbols(string[] symbols)
        {
            string define = symbols == null ? "" : string.Join(";", symbols);
            foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, define);
            }
        }
    }
}