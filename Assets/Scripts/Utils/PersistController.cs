using System;
using System.Collections.Generic;
using Functions;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public static class PersistController 
    {
        private static Dictionary<string, object> _persistValues;
        
        
        public static void SaveCache<T>(string key, T value, bool withUid = false, bool encrypted = false)
        {
            var obj = value;
            key = MakeKeyByUid(key, withUid);
            
            var type = typeof(T);

            if (encrypted)
            {
                if (type == typeof(string))
                {
                    var content = obj as string;
                    PlayerPrefs.SetString(key, content.Encrypt());
                }
                else
                {
                    Jerry_SaveObjAsJson_Mouse(key, value, true);
                }
            }
            else
            {
                if (type == typeof(int))
                {
                    PlayerPrefs.SetInt(key, obj is int i ? i : 0);
                }
                else if (type == typeof(float))
                {
                    PlayerPrefs.SetFloat(key, obj is float i ? i : 0);
                }
                else if (type == typeof(string))
                {
                    // XLog.LogColor($"保存游戏对象 key = {key}, value = {obj as string}");
                    PlayerPrefs.SetString(key, obj as string);
                }
                else if (type == typeof(bool))
                {
                    PlayerPrefs.SetInt(key, obj is true ? 1 : 0);
                }
                else
                {
                    Jerry_SaveObjAsJson_Mouse(key, value);
                }
            }
           
            PlayerPrefs.Save();
        }

        public static T GetCache<T>(string key, bool withUid = false, T defaultValue = default, bool encrypted = false)
        {
            return (T)GetCacheInternal<T>(key, withUid, defaultValue, encrypted);
        }
        
        private static object GetCacheInternal<T>(string key, bool withUid = false, T defaultValue = default, bool encrypted = false)
        {
            key = MakeKeyByUid(key, withUid);
            object result = defaultValue;

            if (encrypted)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    if (typeof(T) == typeof(string))
                    {
                        result = PlayerPrefs.GetString(key).Decrypt();
                    }
                    else
                    {
                        result = Jerry_GetObjectFromJson_Mouse<T>(key, true);
                    }
                }
            }
            else
            {
                if (typeof(T) == typeof(int))
                {
                    result = PlayerPrefs.GetInt(key, defaultValue is int intValue ? intValue  : 0 );
                }

                else if (typeof(T) == typeof(float))
                {
                    result = PlayerPrefs.GetFloat(key, defaultValue is float floatValue ? floatValue : 0);
                }
                else if (typeof(T) == typeof(string))
                {
                    result = PlayerPrefs.GetString(key, defaultValue as string);
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        result = PlayerPrefs.GetInt(key) > 0;
                    }
                }
                else
                {
                    result = Jerry_GetObjectFromJson_Mouse<T>(key, false);
                }
            }
            
            return result;
        }

        public static void Jerry_DeletePrefsValue_Mouse(string key, bool withUid = false)
        {
            key = MakeKeyByUid(key, withUid);

            XLog.LogColor($"删除缓存值 key = {key}");
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        private static string MakeKeyByUid(string key, bool withUid)
        {
            if (withUid)
            {
                //todo
                var userId = 1;
                if (userId <= 0)
                {
                    //随机赋予一个值
                    key = Guid.NewGuid().ToString();
                }
                else
                {
                    key = userId + key;
                }
            }

            return key;
        }

        private static void Jerry_SaveObjAsJson_Mouse<T>(string key, T value, bool encrypted = false)
        {
            var json = JsonConvert.SerializeObject(value);
            if (encrypted)
            {
                json = json.Encrypt();
            }
            PlayerPrefs.SetString(key, json);
        }
        
        public static void Jerry_DeleteAll_Mouse()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        private static T Jerry_GetObjectFromJson_Mouse<T>(string key, bool encrypted)
        {
            try
            {
                var json = PlayerPrefs.GetString(key);
                if (encrypted)
                {
                    json = json.Decrypt();
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                XLog.LogError("解析缓存json错误"+ e + "\n" + e.StackTrace);
                return default;
            }
        }
    }
}