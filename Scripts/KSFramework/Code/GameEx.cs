#region Copyright (c) 2015 KEngine / Kelly <http: //github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AppEngineInspector.cs
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
using System.Collections;
using System.Collections.Generic;
using AppSettings;
using Core.Extensions;
using Core.Services;
using Core.Third.I18N;
using DataAccess.Utils;
using KEngine;
using KEngine.UI;
using KSFramework;
using UnityEngine.U2D;


public class GameEx : KSGame
{
    /// <summary>
    /// Add Your Custom Initable(Coroutine) Modules Here...
    /// </summary>
    /// <returns></returns>
    protected override IList<IModuleInitable> CreateModules()
    {
        var modules = base.CreateModules();

        // TIP: Add Your Custom Module here
#if xLua || SLUA
        modules.Add(LuaModule.Instance);
#elif ILRuntime
        modules.Add(ILRuntimeModule.Instance);
#endif
        return modules;
    }

    public override IEnumerator OnBeforeInit()
    {
        return null;
    }

    /// <summary>
    /// After Init Modules, coroutine
    /// </summary>
    /// <returns></returns>
    public override IEnumerator OnGameStart()
    {
        YZLog.Info(I18N.Get("btn_billboard"));
        // Print AppConfigs
        // Log.Info("======================================= Read Settings from C# =================================");
        foreach (BillboardSetting setting in BillboardSettings.GetAll())
        {
            Debug.Log(string.Format("C# Read Setting, Key: {0}, Value: {1}", setting.Id, setting.Title));
        }

        yield return null;
        KResourceModule.Collect();
    }

    private void OnApplicationQuit()
    {
#if ILRuntime
        ILRuntimeModule.Instance.OnDestroy();
#endif
    }
}