﻿using System;
using System.IO;
using UnityEngine;

namespace PostProcessBuild
{
    public class XcodeFile : IDisposable
    {
        private string filePath;

        public XcodeFile(string fPath)
        {
            filePath = fPath;
            if( !File.Exists( filePath ) ) {
                Debug.LogError( filePath +"路径下文件不存在" );
            }
        }

        public void WriteBelow(string below, string text)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string text_all = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = text_all.IndexOf(below);
            if(beginIndex == -1){
                Debug.LogError(filePath +"中没有找到标致"+below);
                return; 
            }

            int endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);

            text_all = text_all.Substring(0, endIndex) + "\n"+text+"\n" + text_all.Substring(endIndex);

            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }

        public void Replace(string below, string newText)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string text_all = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = text_all.IndexOf(below);
            if(beginIndex == -1){
                Debug.LogError(filePath +"中没有找到标致"+below);
                return; 
            }

            text_all =  text_all.Replace(below,newText);
            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }

        public void Dispose()
        {

        }
    }
}