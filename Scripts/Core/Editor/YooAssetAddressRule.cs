using System.IO;
using System.Linq;
using Core.Extensions;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace Core.Editor
{
    /// <summary>
    /// 以收集器名+文件名为定位地址
    /// </summary>
    public class AddressByGroupNameAndCollectorAndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            var collectorName = Path.GetFileNameWithoutExtension(data.CollectPath);
            return $"{data.GroupName}/{collectorName}/{fileName}".ToLower();
        }
    }

    /// <summary>
    /// 以1级目录+文件名为定位地址
    /// </summary>
    public class AddressByDirectoryLevel1AndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            var directoryPath = data.AssetPath.Replace("Assets/BundlesRes/", "");
            var paths = directoryPath.Split('/');
            for (int i = paths.Length - 1; i >= 0; i--)
            {
                if (i >= 1) paths = paths.RemoveAt(i);
            }

            return $"{paths.Join("/")}/{fileName}".ToLower();
        }
    }


    /// <summary>
    /// 以2级目录+文件名为定位地址
    /// </summary>
    public class AddressByDirectoryLevel2AndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            var directoryPath = data.AssetPath.Replace("Assets/BundlesRes/", "");
            var paths = directoryPath.Split('/');
            for (int i = paths.Length - 1; i >= 0; i--)
            {
                if (i >= 2) paths = paths.RemoveAt(i);
            }

            return $"{paths.Join("/")}/{fileName}".ToLower();
        }
    }

    /// <summary>
    /// 以3级目录+文件名为定位地址
    /// </summary>
    public class AddressByDirectoryLevel3AndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            var directoryPath = data.AssetPath.Replace("Assets/BundlesRes/", "");
            var paths = directoryPath.Split('/');
            for (int i = paths.Length - 1; i >= 0; i--)
            {
                if (i >= 3) paths = paths.RemoveAt(i);
            }

            return $"{paths.Join("/")}/{fileName}".ToLower();
        }
    }

    public class AddressByGroupNameAndPathSplit : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            var extension = Path.GetExtension(data.AssetPath);
            var pre = "Assets/BundlesRes".Length + 1;
            var fullPath = data.AssetPath.Substring(pre, data.AssetPath.Length - pre - extension.Length);
            return fullPath.ToLower();
        }
    }


    public class PackByFirstChar : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var collectPath = data.CollectPath;
            var bundleName = AssetDatabase.IsValidFolder(collectPath) ? collectPath : collectPath.RemoveExtension();

            var result = new PackRuleResult($"{bundleName}_{Path.GetFileName(data.AssetPath).ToLower().First()}");
            return result;
        }

        bool IPackRule.IsRawFilePackRule()
        {
            return false;
        }
    }

    [DisplayName("收集配置")]
    public class CollectAsset : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".asset";
        }
    }

    [DisplayName("收集shader")]
    public class CollectShader : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shader";
        }
    }

    [DisplayName("收集材质")]
    public class CollectMat : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) switch
            {
                ".mat" => true,
                _ => false
            };
        }
    }

    public class CollectUIAtlas : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return data.AssetPath.Contains("Atlas_");
        }
    }


    [DisplayName("收集文本")]
    public class CollectTxt : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("Dependencies"))
            {
                return false;
            }

            return Path.GetExtension(data.AssetPath) == ".txt";
        }
    }

    [DisplayName("收集纹理")]
    public class CollectTex : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}