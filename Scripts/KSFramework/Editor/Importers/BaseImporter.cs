using UnityEngine;
using System.Collections;
using System.IO;
using KEngine;
using UnityEditor;


public class BaseImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        //deletedAssets movedFromAssetPaths 删除资源
        //importedAssets movedAssets  新增和移动资源

        OnPostprocessAssets(importedAssets);
        OnPostprocessAssets(movedAssets);
    }

    static void OnPostprocessAssets(string[] assets)
    {
    }

    public void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (importer == null)
            return;
        //TODO 根据情况是否导入动画
        // importer.importAnimation = false;
        // importer.importBlendShapes = false;
        //非effect目录，则不勾选Read/Write
        if (importer.assetPath.Contains(KEngineDef.EffectPath))
            return;
        if (importer.isReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }
    }
}