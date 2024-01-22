using System;
using Core.Extensions;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utils
{
    public class YZJsonUtil
    {
        public YZJsonUtil()
        {
        }

        public static T DeserializeJObject<T>(string path, JObject jObject)
        {
            try
            {
                var json = jObject.SelectToken(path)?.ToString();
                if (!json.IsNullOrEmpty() && json != "[]")
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception e)
            {
                YZLog.LogError(e.ToString());
            }

            return default;
        }

        public static bool ContainsYZKey(JsonData json, string key)
        {
            if (json == null)
            {
                return false;
            }

            if (json.IsObject)
            {
                return json.Keys.Contains(key);
            }

            return false;
        }

        public static int GetYZInt(JsonData json, string key, int dft = 0)
        {
            if (ContainsYZKey(json, key) == false || json[key] == null)
            {
                return dft;
            }

            JsonType type = json[key].GetJsonType();
            if (type == JsonType.Array || type == JsonType.Object)
            {
                return dft;
            }

            if (type == JsonType.Boolean)
            {
                return (Boolean)json[key] ? 1 : 0;
            }

            if (type == JsonType.Double)
            {
                return (int)Math.Round((Double)(json[key]), 0);
            }

            if (type == JsonType.Long)
            {
                return (int)((long)(json[key]));
            }

            if (type == JsonType.String)
            {
                string value = (string)json[key];
                int num = 0;
                int flag = 1;
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == '.')
                    {
                        return num * flag;
                    }

                    if (i == 0 && c == '-')
                    {
                        flag = -1;
                        continue;
                    }

                    if (c >= '0' && c <= '9')
                    {
                        int n = (int)c - 48;
                        num = num * 10 + n;
                    }
                    else
                    {
                        YZDebug.Log(json.ToJson());
                        return dft;
                    }
                }

                return num * flag;
            }

            if (type == JsonType.Int)
            {
                return (int)json[key];
            }

            YZDebug.Log(json.ToJson());
            return dft;
        }

        public static long GetYZLong(JsonData json, string key, long dft = 0)
        {
            if (ContainsYZKey(json, key) == false || json[key] == null)
            {
                return dft;
            }

            JsonData data = json[key];
            JsonType type = data.GetJsonType();
            if (type == JsonType.Array || type == JsonType.Object)
            {
                YZDebug.Log(data.ToJson());
                return dft;
            }

            if (type == JsonType.Boolean)
            {
                return (Boolean)data ? 1 : 0;
            }

            if (type == JsonType.Double)
            {
                return (long)Math.Round((Double)(data), 0);
            }

            if (type == JsonType.Long)
            {
                return (long)data;
            }

            if (type == JsonType.String)
            {
                string value = (string)data;
                long num = 0;
                int flag = 1;
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == '.')
                    {
                        return num * flag;
                    }

                    if (i == 0 && c == '-')
                    {
                        flag = -1;
                        continue;
                    }

                    if (c >= '0' && c <= '9')
                    {
                        int n = (int)c - 48;
                        num = num * 10 + n;
                    }
                    else
                    {
                        YZDebug.Log(data.ToJson());
                        return dft;
                    }
                }

                return num * flag;
            }

            if (type == JsonType.Int)
            {
                return (long)((int)data);
            }

            YZDebug.Log(data.ToJson());
            return dft;
        }

        public static float GetYZFloat(JsonData json, string key, float dft = 0.0f)
        {
            if (ContainsYZKey(json, key) == false || json[key] == null)
            {
                return dft;
            }

            JsonType type = json[key].GetJsonType();
            if (type == JsonType.Array || type == JsonType.Object)
            {
                YZDebug.Log(json.ToJson());
                return dft;
            }

            if (type == JsonType.Boolean)
            {
                return (bool)json[key] ? 1.0f : 0.0f;
            }

            if (type == JsonType.Double)
            {
                return (float)((double)(json[key]));
            }

            if (type == JsonType.Long)
            {
                return (float)((long)(json[key]));
            }

            if (type == JsonType.String)
            {
                string value = (string)json[key];
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == '.')
                    {
                        continue;
                    }

                    if (i == 0 && c == '-')
                    {
                        continue;
                    }

                    if ((c < '0' || c > '9'))
                    {
                        YZDebug.Log(json.ToJson());
                        return dft;
                    }
                }

                decimal result;
                bool t = decimal.TryParse(value, out result);
                if (t)
                {
                    return (float)result;
                }

                return dft;
            }

            if (type == JsonType.Int)
            {
                return (float)((int)(json[key]));
            }

            YZDebug.Log(json.ToJson());
            return dft;
        }

        public static string GetYZString(JsonData json, string key, string dft = "")
        {
            if (ContainsYZKey(json, key) == false || json[key] == null)
            {
                return dft;
            }

            return json[key].ToString();
        }
    }
}