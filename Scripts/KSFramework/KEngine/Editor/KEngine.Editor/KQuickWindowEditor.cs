using System;
using UnityEngine;
using System.IO;
using AppSettings;
using Core.Editor;
using Core.Extensions;
using Core.Services;
using DataAccess.Utils;
using KEngine;
using KEngine.Editor;
using KEngine.UI;
using KSFramework.Editor;
using KUnityEditorTools;
using UnityEditor;

public class KQuickWindowEditor : EditorWindow
{
    Vector3 scrollPos = Vector2.zero;
    [SerializeField] private string reloadUIAb;
    [SerializeField] private string reloadUIScript;
    private GUIStyle textFieldStyle;
    private bool init = false;
    private PanelType panelType = PanelType.HeadInfoUI;
    private UIController topUI;

    [MenuItem("KEngine/Open Quick Window %&Q")]
    static void DoIt()
    {
        //note 在Editor下Scree.Width,Screen.Height获取的并不是真实的显示器宽度和高度
        KQuickWindowEditor window = EditorWindow.GetWindow<KQuickWindowEditor>();
        window.titleContent = new GUIContent("快捷工具窗");
        window.position = new Rect(400, 100, 640, 480);
        window.Show();
    }

    void InitStyle()
    {
        if (init)
            return;
        textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.alignment = TextAnchor.MiddleLeft;

        init = true;
    }

    public void OnGUI()
    {
        InitStyle();
        scrollPos = GUI.BeginScrollView(new Rect(10, 10, position.width, position.height), scrollPos,
            new Rect(0, 0, 360, 1000));

#if ILRuntime
        DrawILRuntime();
        GUILayout.Space(20);
#endif
        DrawHotReLoadUI();
        GUILayout.Space(20);

        DrawKEngineUI();
        GUILayout.Space(20);

        DrawAssetBundleUI();
        GUILayout.Space(20);

        DrawDebugUI();
        GUILayout.Space(20);
        GUI.EndScrollView();
    }

    public void DrawHotReLoadUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== 热重载 ==");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重载配置表", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            SettingsManager.AllSettingsReload();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("快速编译配置表", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            SettingModuleEditor.QuickCompileSettings();
        }

        if (GUILayout.Button("编译配置表并生成代码", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            SettingModuleEditor.CompileSettings();
        }

        GUILayout.EndHorizontal();
#if xLua || SLUA
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重载所有UI的Lua代码", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadAllUIScript();
        }

        if (GUILayout.Button("重新打开所有UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadAllUI();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("UI名字:", GUILayout.Width(40));
        reloadUIScript = EditorGUILayout.TextField(reloadUIScript, GUILayout.MinWidth(120),GUILayout.MaxHeight(30));
        panelType = (PanelType)EditorGUILayout.EnumPopup("",panelType, GUILayout.MinWidth(80),GUILayout.MinHeight(30));
        if (panelType != PanelType.HeadInfoUI && Application.isPlaying)
        {
            topUI = UIModule.Instance.GetTopUI(panelType);
            //Log.Info($"now top ui:{topUI?.UIName}");
            if (topUI != null) reloadUIScript = topUI.UIName;
        }
        if (GUILayout.Button("重载Lua脚本", GUILayout.MinWidth(100), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadUIScript(reloadUIScript);
        }

        GUILayout.EndHorizontal();
#endif
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("UI名字:", GUILayout.Width(40));
        reloadUIAb = EditorGUILayout.TextField(reloadUIAb, GUILayout.MinWidth(120), GUILayout.MaxHeight(30));
        if (GUILayout.Button("重载AB并打开", GUILayout.MinWidth(100), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.ReloadUIAB(reloadUIAb);
        }

        GUILayout.EndHorizontal();
        //GUILayout.Space(10);
    }

    public void DrawILRuntime()
    {
#if ILRuntime
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== ILRuntime ==");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("生成Dll", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
           ILRuntimeEditor.GenDll();
        }

        if (GUILayout.Button("打包dll为ab", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            ILRuntimeEditor.BuildDllToAb();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("生成CLR绑定", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            ILRuntimeEditor.GenerateCLRBindingByAnalysis();
        }

        if (GUILayout.Button("生成适配器", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            ILRuntimeEditor.GenerateCrossbindAdapter();
        }
        if (GUILayout.Button("删除生成代码", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            ILRuntimeEditor.ClearGenCode();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("开启调试", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KDefineSymbolsHelper.AddDefineSymbols(new string[]{"ILRuntime","DEBUG"});
        }

        if (GUILayout.Button("默认设置", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KDefineSymbolsHelper.AddDefineSymbols(new string[]{"ILRuntime"});
        }
        if (GUILayout.Button("最佳性能", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KDefineSymbolsHelper.AddDefineSymbols(new string[]{"ILRuntime","DISABLE_ILRUNTIME_DEBUG"});
        }
        GUILayout.EndHorizontal();
#endif
    }

    public void DrawKEngineUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== UI相关辅助 ==");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建常规UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.CreateNormalUI();
        }
#if xLua || SLUA
        if (GUILayout.Button("为当前UI创建Lua脚本", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KSFrameworkEditor.AutoMakeUILuaScripts();
        }
#endif
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建主界面UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.CreateMainUI();
        }

        if (GUILayout.Button("创建Tips界面", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.CreateTipsUI();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出当前UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.ExportCurrentUI();
        }

        if (GUILayout.Button("导出全部UI", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.ExportAllUI();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("导出公共图集", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(30)))
        {
            KUGUIBuilder.BuildCommonAtlas();
        }

        GUILayout.EndHorizontal();
    }

    public void DrawAssetBundleUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== Assetbundle相关 ==");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();
    }

    void DrawDebugUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("== Debug Module ==");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Dump", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            KProfiler.Dump();
        }

        if (GUILayout.Button("打开AB加载耗时记录", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(20)))
        {
            OpenFolder(Application.persistentDataPath, true);
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="createNew">如果指定的目录不存在则创建</param>
    void OpenFolder(string path, bool createNew = false)
    {
        var fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath) == false)
        {
            if (createNew)
            {
                Directory.CreateDirectory(fullPath);
                YZLog.Info("{0} 目录不存在，已创建。", fullPath);
            }
            else
            {
                YZLog.Info("{0} 目录不存在，尝试定位到父目录。", fullPath);
                DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
                if (directoryInfo.Parent != null) fullPath = directoryInfo.Parent.FullName;
            }
        }

        YZLog.Info("open: {0}", fullPath);
        System.Diagnostics.Process.Start("explorer.exe", fullPath);
    }
}