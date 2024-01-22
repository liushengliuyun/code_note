using DataAccess.Utils.Static;
using UnityEngine;

namespace Core.Runtime.Game.Config
{
    public class GameConfig
    {
        /// <summary>
        /// 游戏共有多少种语言  mark 语言暂时只有英文
        /// </summary>
        public static string[] I18NLanguages = new string[] { GlobalEnum.ENGLISH };

        /// <summary>
        /// 当前的语言ID
        /// </summary>
        public static string LangId = GlobalEnum.ENGLISH;

        /// 开发中的语言，此语言下所有的文件不需要加后缀，否则文件名后加: --语种
        /// </summary>
        public const string dev_lang = GlobalEnum.ENGLISH;


        private static string langFileFlag;

        /// <summary>
        /// 多语言加载文件的标识，比如不同地区读的语言包、图片，会在资源名后面加上此标识(eg: atlas.ab -> atlas--en.ab)
        /// </summary>
        public static string LangFileFlag
        {
            get
            {
                if (langFileFlag == null)
                {
                    langFileFlag = string.IsNullOrEmpty(dev_lang) ? "--" + dev_lang : "--" + LangId;
                }

                return langFileFlag;
            }
        }


        //应用更新、覆盖安装时，都不会删除
        public static string LiteDBPath = $"{Application.persistentDataPath}/lite.db";
    }
}