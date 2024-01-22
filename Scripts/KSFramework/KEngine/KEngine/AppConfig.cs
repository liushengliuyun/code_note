using KEngine;
using UnityEngine;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/4 
/// Desc：app常量配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// 编辑器下默认不打印硬件设备的信息，真机写入到日志中
    /// </summary>
    public static bool IsLogDeviceInfo = false;

    /// <summary>
    /// 是否打印ab加载的日志
    /// </summary>
    public static bool IsLogAbInfo = false;

    /// <summary>
    /// 是否打印ab加载耗时
    /// </summary>
    public static bool IsLogAbLoadCost = false;

    /// <summary>
    /// 是否打印函数执行耗时
    /// </summary>
    public static bool IsLogFuncCost = false;

    /// <summary>
    /// 是否记录到文件中，包括：UI的ab加载耗时，UI函数执行耗时
    /// </summary>
    public static bool IsSaveCostToFile = Application.isEditor;

    /// <summary>
    /// 是否调试模式，可与Unity的Debug区分开
    /// </summary>
    public static bool IsDebugBuild = Debug.isDebugBuild;

    /// <summary>
    /// 是否Editor模式，可在某些情况下与Unity的isEditor区分，比如详细/网络日志调试模式下在真机输出
    /// </summary>
    public static bool isEditor = Application.isEditor;

    /// <summary>
    /// 是否创建AssetDebugger
    /// </summary>
    public static bool UseAssetDebugger = Application.isEditor;

    /// <summary>
    /// 仅对Editor有效，Editor下加载资源默认从磁盘的相对目录读取，如果需要从Aplication.streamingAssets则设置为true
    /// </summary>
    public static bool ReadStreamFromEditor;

    /// <summary>
    /// cdn资源地址，正式项目通过服务器下发
    /// </summary>
    public static string resUrl = "http://127.0.0.1:8080/cdn/";

    //public static string resUrl = "http://192.168.190.112:8080/cdn/";
    /// <summary>
    /// 是否开启下载更新的功能，doc:https://mr-kelly.github.io/KSFramework/advanced/autoupdate/
    /// </summary>
    public static bool IsDownloadRes = false;

    private static string versionTextPath = null;

    public static string VersionTextPath
    {
        get
        {
            if (string.IsNullOrEmpty(versionTextPath))
                versionTextPath = KResourceModule.AppBasePath + "/" + VersionTxtName;
            return versionTextPath;
        }
    }

    private static string versionTxtName = null;

    public static string VersionTxtName
    {
        get
        {
            if (string.IsNullOrEmpty(versionTxtName))
                versionTxtName = KResourceModule.GetBuildPlatformName() + "-version.txt";
            return versionTxtName;
        }
    }

    private static string filelistPath = null;

    /// <summary>
    /// filelist.txt的相对路径
    /// </summary>
    public static string FilelistPath
    {
        get
        {
            if (string.IsNullOrEmpty(filelistPath))
                filelistPath = $"Bundles\\{KResourceModule.GetBuildPlatformName()}\\filelist.txt";
            return filelistPath;
        }
    }

    #region AppConfigs.txt中的内容,AssetLink.bat也有一份

    //
    /// <summary>
    /// Product目录的相对路径
    /// </summary>
    public const string ProductRelPath = "Product";

    public const string AssetBundleBuildRelPath = "Product/Bundles";
    public const string StreamingBundlesFolderName = "Bundles";

    public static bool IsLoadAssetBundle = true;

    //whether use assetdata.loadassetatpath insead of load asset bundle, editor only
    public static bool IsEditorLoadAsset = false;

    /// <summary>
    /// 配置表编译出文件的后缀名, 可修改
    /// </summary>
    public const string SettingExt = ".tsv";

    //; config use lua  or c# + tsv
    public static bool IsUseLuaConfig = false;
    public const string SettingSourcePath = "Product/SettingSource";
    public const string ExportLuaPath = "Product/Lua/configs/";
    public const string ExportTsvPath = "Product/Setting";
    public const string ExportCSharpPath = "Assets/Scripts/Configs/";


    //;Folder in Resources
    public const string SettingResourcesPath = "Setting";

    //; Ignore genereate code for these excel.
    public const string SettingCodeIgnorePattern = "(I18N/.*)|(StringsTable.*)";

    /// <summary>
    /// UI设计的分辨率
    /// </summary>
    public static Vector2 UIResolution = new Vector2(1280, 720);

    #endregion

    public static void Init()
    {
        IsLogDeviceInfo = !Application.isEditor;
    }
}