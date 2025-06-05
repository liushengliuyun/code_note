using System.IO;
using Auto9Slicer;
using UnityEditor;
using UnityEngine;

namespace Development
{
    // 引用的这个插件 https://github.com/kyubuns/Auto9Slicer
    public static class Convert9SlicerEditor
    {

        [MenuItem("Assets/转换成9宫图", priority = 1)]
        public static void Convert9Slicer()
        {
            string[] assetGUIDs = Selection.assetGUIDs;
            foreach (var guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path);

                if (importer is TextureImporter textureImporter)
                {
                    if (textureImporter.spriteBorder != Vector4.zero) continue;
                    var targetTexture = new Texture2D(2, 2);
                    var bytes = File.ReadAllBytes(path);
                    targetTexture.LoadImage(bytes);
                    var slicedTexture = Slicer.Slice(targetTexture, SliceOptions.Default);
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.spriteBorder = slicedTexture.Border.ToVector4();
                    File.WriteAllBytes(path, slicedTexture.Texture.EncodeToPNG());
                }
            }
            AssetDatabase.Refresh();
        }
    }
}