using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Carbon.Util
{
    public static class FileUtils
    {
        public const string PrivateKey = "xxxxxBxxxxxxxxxxxDxxxxFxxxHxxxxx";

        public static byte[] ReadBytesFromFile(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                return !File.Exists(path) ? null : File.ReadAllBytes(path);
            }
            catch (Exception)
            {
                Debug.Log($"can not loadFile At {path}");
                return null;
            }
        }

        public static void SetData(string fileName, string content)
        {
            try
            {
                var fullPath = $"{Application.persistentDataPath}/{fileName}";
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                var dirName = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                //将对象序列化为字符串
                //对字符串进行加密,32位加密密钥
                content = Encrypt(content, PrivateKey);
                File.WriteAllText(fullPath, content);
            }
            catch (Exception e)
            {
            }
        }

        public static object GetData(string fileName)
        {
            var fullPath = $"{Application.persistentDataPath}/{fileName}";
            if (!File.Exists(fullPath))
            {
                return "";
            }

            var content = File.ReadAllText(fullPath);
            if (string.IsNullOrEmpty(content))
            {
                ResetFile(fullPath);
                return "";
            }

            content = Decrypt(content, PrivateKey);
            if (string.IsNullOrEmpty(content))
            {
                CarbonLogger.LogError($"删除文件:{fileName}");
                ResetFile(fullPath);
                return "";
            }

            return content;
        }

        /// <summary>
        /// Rijndael加密算法
        /// </summary>
        /// <param name="pString">待加密的明文</param>
        /// <param name="pKey">密钥,长度可以为:64位(byte[8]),128位(byte[16]),192位(byte[24]),256位(byte[32])</param>
        /// <param name="iv">iv向量,长度为128（byte[16])</param>
        /// <returns></returns>
        public static string Encrypt(string pString, string pKey)
        {
            try
            {
                //密钥
                var keyArray = Encoding.UTF8.GetBytes(pKey);
                //待加密明文数组
                var toEncryptArray = Encoding.UTF8.GetBytes(pString);

                var rDel = new RijndaelManaged { Key = keyArray, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
                var cTransform = rDel.CreateEncryptor();

                //返回加密后的密文
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// ijndael解密算法
        /// </summary>
        /// <param name="pString">待解密的密文</param>
        /// <param name="pKey">密钥,长度可以为:64位(byte[8]),128位(byte[16]),192位(byte[24]),256位(byte[32])</param>
        /// <param name="iv">iv向量,长度为128（byte[16])</param>
        /// <returns></returns>
        public static string Decrypt(string pString, string pKey = PrivateKey)
        {
            try
            {
                //解密密钥
                var keyArray = Encoding.UTF8.GetBytes(pKey);
                //待解密密文数组
                var toEncryptArray = Convert.FromBase64String(pString);

                //Rijndael解密算法
                var rDel = new RijndaelManaged { Key = keyArray, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
                var cTransform = rDel.CreateDecryptor();

                //返回解密后的明文
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString() + $"content is {pString}");
                return "";
            }
        }

        #region 关于IO

        /// <summary>
        /// 拷贝某个文件夹
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="searchPattern"></param>
        /// <param name="option"></param>
        public static void CopyDirectory(string source, string dest, string searchPattern = "*.*",
            SearchOption option = SearchOption.AllDirectories)
        {
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
            }

            var files = Directory.GetFiles(source, searchPattern, option);
            foreach (var file in files)
            {
                var str = file.Remove(0, source.Length);
                var path = $"{dest}/{str}";
                var dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
                File.Copy(file, path, true);
            }
        }

        /// <summary>
        /// 拷贝某个文件
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        public static void SafelyCopyFile(string sourcePath, string targetPath)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.Log($"找不到文件{sourcePath}");
                return;
            }

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            var dir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(sourcePath, targetPath);
        }

        /// <summary>
        /// 保证文件夹存在
        /// </summary>
        /// <param name="dir"></param>
        public static void MakeSureDirExist(string dir)
        {
            if (Directory.Exists(dir))
            {
                return;
            }

            Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// 创建某个文件的时候,保证文件能创建
        /// </summary>
        public static void MakeSureFileDirExists(string filePath)
        {
            MakeSureDirExist(Path.GetDirectoryName(filePath));
        }

        /// <summary>
        /// 安全的删除某个文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="recursive"></param>
        public static void SafelyDeleteDir(string dir, bool recursive = true)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive);
            }
        }

        public static void SafelyDeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// 重置某个文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void ResetDirectory(string dir)
        {
            SafelyDeleteDir(dir);
            Directory.CreateDirectory(dir);
        }

        public static void ResetFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// 得到文件的大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return 0;
            }

            return (int)new FileInfo(filePath).Length;
        }

        /// <summary>
        /// 是否存在某个文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool ExistsFile(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 给一个文件命名
        /// </summary>
        /// <param name="originFilePath"></param>
        /// <param name="targetFilePath"></param>
        /// <param name="deleteOrigin"></param>
        /// <returns></returns>
        public static bool RenameFile(string originFilePath, string targetFilePath, bool deleteOrigin = true)
        {
            if (string.IsNullOrEmpty(originFilePath) || string.IsNullOrEmpty(targetFilePath))
            {
                return false;
            }

            if (!File.Exists(originFilePath))
            {
                return false;
            }

            ResetFile(targetFilePath);

            File.Move(originFilePath, targetFilePath);

            if (deleteOrigin && File.Exists(originFilePath))
            {
                File.Delete(originFilePath);
            }

            return true;
        }

        /// <summary>
        /// 写文本
        /// </summary>
        /// <param name="text"></param>
        /// <param name="path"></param>
        public static void WriteText(string text, string path)
        {
            try
            {
                var fullPath = $"{Application.persistentDataPath}/{path}";

                MakeSureFileDirExists(fullPath);

                ResetFile(fullPath);

                File.WriteAllText(fullPath, text);
            }
            catch (Exception e)
            {
            }
        }

        public static void WriteTextByFullPath(string text, string fullPath)
        {
            try
            {
                MakeSureFileDirExists(fullPath);

                ResetFile(fullPath);

                File.WriteAllText(fullPath, text);
            }
            catch (Exception e)
            {
            }
        }

        static System.Security.Cryptography.MD5 md5Service = null;

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <param name="lastLen"></param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return "";
            }

            try
            {
                //计算文件的MD5->md5
                if (md5Service == null)
                {
                    md5Service = new System.Security.Cryptography.MD5CryptoServiceProvider();
                }

                FileStream fs = new FileStream(filename, FileMode.Open);
                byte[] retVal = md5Service.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        #endregion

        // /// <summary>
        //  /// 解压
        //  /// </summary>
        //  /// <param name="Value"></param>
        //  /// <returns></returns>
        //  public static DataSet GetDatasetByString(string Value)
        //  {
        //      DataSet ds = new DataSet();
        //      string CC = GZipDecompressString(Value);
        //      System.IO.StringReader Sr = new StringReader(CC);
        //      ds.ReadXml(Sr);
        //      return ds;
        //  }
        //  /// <summary>
        //  /// 根据DATASET压缩字符串
        //  /// </summary>
        //  /// <param name="ds"></param>
        //  /// <returns></returns>
        //  public static string GetStringByDataset(string ds)
        //  {
        //      return GZipCompressString(ds);
        //  }
        //  /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }
        }

        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
            }
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }

            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        //记录上次访问的时间
        private static readonly Dictionary<string, double> FileAccessTimeMap = new Dictionary<string, double>();

        /// <summary>
        /// 保证目录只留下多少个文件
        /// </summary>
        public static void CleanFolder(string dir, string searchPattern = "*.*", int maxCount = 500)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            var files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
            if (files.Length <= maxCount)
            {
                return;
            }

            var fileList = files.ToList();
            fileList.Sort((fileA, fileB) =>
            {
                if (!FileAccessTimeMap.TryGetValue(fileA, out double accessA))
                {
                    var lastAccessTime = new FileInfo(fileA).LastAccessTime;
                    accessA = (lastAccessTime - new DateTime(1970, 1, 1)).TotalSeconds;
                    FileAccessTimeMap[fileA] = accessA;
                }

                if (!FileAccessTimeMap.TryGetValue(fileB, out double accessB))
                {
                    var lastAccessTime = new FileInfo(fileB).LastAccessTime;
                    accessB = (lastAccessTime - new DateTime(1970, 1, 1)).TotalSeconds;
                    FileAccessTimeMap[fileB] = accessB;
                }

                //先创建的排在前面
                return (int)(accessA - accessB);
            });

            int removeCount = fileList.Count - maxCount;
            for (int i = 0; i < removeCount; i++)
            {
                File.Delete(fileList[i]);
            }
        }
    }
}