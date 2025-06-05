using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using JerryMouse.Model.JsonConfig;
using SlotX.Utils;

namespace JerryMouse.Editor
{
    public class LocalConfigEditor
    {
        public static string OutputPath = Application.dataPath + "/Editor/LocalConfig/Output";
        
        [MenuItem("Tools/LocalConfig/Export")]
        public static void Export()
        {
            string jsonData;
            
            DeleteAll();
            
            SavingPotConfig savingPotConfig = LocalConfig.InitSavingPotConfig();
            jsonData = XJsonUtil.Serialize(savingPotConfig);
            File.WriteAllText(OutputPath + "/saving_pot_in_group_b.json", jsonData);
            
            
            
            AssetDatabase.Refresh();
        }

        public static void DeleteAll()
        {
            string[] filePaths = Directory.GetFiles(OutputPath);
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }
        
        
        
        
    }
}
