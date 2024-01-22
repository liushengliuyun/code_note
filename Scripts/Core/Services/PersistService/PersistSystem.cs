using System;
using System.Linq;
using Core.Extensions;
using Core.Runtime.Game.Config;
using Core.Services.PersistService.API;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UltraLiteDB;
using UnityEngine;


namespace Core.Services.PersistService
{
    public class PersistSystem : IPersistSystem
    {
        public void SaveValue<T>(string key, T value, bool withUid = false)
        {
            var obj = value;
            key = Key(key, withUid);

            if (typeof(T) == typeof(int))
            {
                PlayerPrefs.SetInt(key, obj is int i ? i : 0);
                PlayerPrefs.Save();
            }
            else if (typeof(T) == typeof(float))
            {
                PlayerPrefs.SetFloat(key, obj is float i ? i : 0);
                PlayerPrefs.Save();
            }
            else if (typeof(T) == typeof(string))
            {
                // YZLog.LogColor($"保存游戏对象 key = {key}, value = {obj as string}");
                PlayerPrefs.SetString(key, obj as string);
                PlayerPrefs.Save();
            }
            else if (typeof(T) == typeof(bool))
            {
                PlayerPrefs.SetInt(key, obj is true ? 1 : 0);
                PlayerPrefs.Save();
            }
            else
            {
                SaveObject(key, value);
            }
        }

        public object GetValue<T>(string key, bool withUid = false)
        {
            key = Key(key, withUid);

            if (typeof(T) == typeof(int))
            {
                return PlayerPrefs.GetInt(key);
            }

            if (typeof(T) == typeof(float))
            {
                return PlayerPrefs.GetFloat(key);
            }

            if (typeof(T) == typeof(string))
            {
                return PlayerPrefs.GetString(key);
            }

            if (typeof(T) == typeof(bool))
            {
                return PlayerPrefs.GetInt(key) > 0;
            }

            //return default
            return GetObject<T>(key);
        }

        public void DeletePrefsValue(string key, bool withUid = false)
        {
            key = Key(key, withUid);

            YZLog.LogColor($"删除缓存值 key = {key}");
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        private string Key(string key, bool withUid)
        {
            if (withUid)
            {
                key = Root.Instance.UserId + key;
            }

            return key;
        }

        void save_object<T>(string key, T value)
        {
            using var db = new UltraLiteDatabase(GameConfig.LiteDBPath);
            
            var col = db.GetCollection<T>(key);
            if (col.Count() == 0)
            {
                col.Insert(1, value);
            }
            else
            {
                if (col.Update(1, value)) return;
                var obj = col.FindOne(Query.All());
                col.Update((int)typeof(T).GetProperty("Id").GetValue(obj), value);
            }
        }

        private void SaveObject<T>(string key, T value)
        {
            try
            {
                save_object(key, value);
            }
            catch (Exception e)
            {
                using var db = new UltraLiteDatabase(GameConfig.LiteDBPath);
                foreach (var name in db.GetCollectionNames().ToList())
                {
                    db.DropCollection(name);
                }
                SaveObjectOnce(key, value);
                YZLog.LogColor(e, "red");
            }
        }

        private void SaveObjectOnce<T>(string key, T value)
        {
            try
            {
                save_object(key, value);
            }
            catch (Exception e)
            {
                YZLog.LogColor(e, "red");
            }
        }
        
        public void DeleteDB<T>(string key, bool withUid = false)
        {
            try
            {
                key = Key(key, withUid);
                using var db = new UltraLiteDatabase(GameConfig.LiteDBPath);
                db.DropCollection(key);
            }
            catch (Exception e)
            {
                YZLog.LogColor(e, "red");
            }
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        private T GetObject<T>(string key)
        {
            try
            {
                using var db = new UltraLiteDatabase(GameConfig.LiteDBPath);
                var col = db.GetCollection<T>(key);
                if (col.Count() == 0) return default;

                return col.FindOne(Query.All());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }


        public UltraLiteCollection<T> GetCollection<T>(string key)
        {
            using var db = new UltraLiteDatabase(GameConfig.LiteDBPath);
            return db.GetCollection<T>(key);
        }
    }
}