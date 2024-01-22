using System;
using System.Collections.Generic;
using System.IO;
using Castle.Core.Internal;
using Core.Extensions;

namespace Core.Third.I18N
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2021/1/23 19:02
    /// Desc：文本解析器
    /// </summary>
    public static class TextParser
    {
        public static void ParseText(string text, Dictionary<string, string> dict)
        {
            if (dict == null) dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(text))
            {
                using StringReader reader = new StringReader(text);
                string key = null;
                string value = null;

                var setvalue = new Action(() =>
                {
                    if (!key.IsNullOrEmpty())
                    {
                        dict[key] = value;
                        key = null;
                        value = null;
                    }
                });

                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (IsCommentLine(line)) continue;
                    var newLine = DecodeLine(line);

                    if (newLine.IsNullOrEmpty())
                    {
                        setvalue();
                    }
                    else
                    {
                        var index = newLine.IndexOf('=');
                        if (index > 0)
                        {
                            setvalue();
                            key = newLine.Substring(0, index).Trim();
                            value = newLine.Substring(index + 1).Trim();
                        }
                        else
                        {
                            value += "\n" + newLine;
                        }
                    }
                }

                setvalue();
            }
        }

        public static bool IsCommentLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return true;
            return line[0] == '#' || line[0] == '/' || line[1] == '/';
        }

        public static string DecodeLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return "";
            if (line.IndexOf("\\", StringComparison.Ordinal) >= 0)
            {
                line = line.Replace("\\r", "\r");
                line = line.Replace("\\n", "\n");
                line = line.Replace("\\t", "\t");
            }

            return line;
        }
    }
}