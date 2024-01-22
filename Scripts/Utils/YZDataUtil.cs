using LitJson;
using UnityEngine;

namespace Utils
{
    public class YZDataUtil
    {
        
        
        #region CreditCardCachData
        public static JsonData GetCreditCardData()
        {
            JsonData result = null;
            var saveStr = GetLocaling(YZConstUtil.YZDepositCreditCardDetailsStr);
            if (!string.IsNullOrEmpty(saveStr))
            {
                result = JsonMapper.ToObject(saveStr);
                if (result != null)
                {
                    // 缓存数据升级，有旧字段的老数据直接丢弃
                    if (YZJsonUtil.ContainsYZKey(result, "mmyy"))
                    {
                        SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
                        result = null;
                    }
                }
            }

            return result;
        }
        #endregion
        
        public static void SetYZString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        
        public static void SetYZInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        
        public static int GetYZInt(string key, int dft)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetInt(key);
            }
            return dft;
        }

        public static string GetLocaling(string key, string defaultValue = "")
        {
            string ret = PlayerPrefs.GetString(key, defaultValue);
            if (ret == "")
                return defaultValue;
            else
                return ret;
        }
    }
}