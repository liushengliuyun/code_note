using System.Collections;
using System.Collections.Generic;
using System.Text;
using AppSettings;
using Core.Extensions;
using Core.Services.ResourceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using UnityEngine;

namespace DataAccess.Model
{
    /// <summary>
    /// 游戏物品的抽象
    /// </summary>
    public class Item
    {
        public int id;
        public float Count;
        public string name;

        public Item()
        {
        }

        public Item(int id, float count = 0)
        {
            this.id = id;
            Count = count;
            name = ItemSettings.Get(id)?.Name;
        }
        
        public Item(string name, float count = 0)
        {
            var row = GetItemIdByName(name);
            this.name = name;
            if (row != null)
            {
                id = row.Id;
            }
            else
            {
                id = -1;
            }
            Count = count;
        }
        
        public static ItemSetting GetItemIdByName(string name)
        {
            foreach (var row in ItemSettings.GetAll())
            {
                if (row is ItemSetting itemSetting)
                {
                    if (itemSetting.Name == name)
                    {
                        return itemSetting;
                    }
                }
            }

            return null;
        }

        public static string GetItemsName(IEnumerable<Item> items)
        {
            if (items == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in items)
            {
                stringBuilder.Append($" {item.Count} * {item.GetName()}");
            }

            return stringBuilder.ToString();
        }

        public string GetName()
        {
            return I18N.Get($"item_name_{id}");
        }

        public Sprite GetIcon()
        {
            if (name == "MuseumPoint")
            {
                return MediatorBingo.Instance.GetSpriteByUrl("common/museum_point");
            }

            if (name == "piggyBonus")
            {
                return MediatorBingo.Instance.GetSpriteByUrl("common/piggy bank_icon");
            }
            
            if (name == "MagicBallPoint")
            {
                return MediatorBingo.Instance.GetSpriteByUrl("common/magic_point_icon");
            }
            
            var itemSetting = ItemSettings.Get(id);
            if (itemSetting == null)
            {
                YZLog.LogColor("没有找到对应道具的 配置 id =" + id, "red");
                return null;     
            }
                                 
            return MediatorBingo.Instance.GetSpriteByUrl(itemSetting.Icon);
        }
    }
}