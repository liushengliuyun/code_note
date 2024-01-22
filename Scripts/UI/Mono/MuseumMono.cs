
using System;
using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace UI.Mono
{
    public class MuseumMono : MonoBehaviour
    {
        public Transform[] ItemRow;
        
        public Transform[] DollarsShow;

        public Text LessTimeText;

        public MyButton CollectAllBtn;

        private Timer IntervalTimer;
        
        private void Awake()
        {
            Init();
        }
        
        public void Init()
        {
            int index = 0;
            
            if (Root.Instance.MuseumItems == null)
            {
                return;
            }

            if (CollectAllBtn != null)
            {
                CollectAllBtn.SetActive(Root.Instance.MuseumInfo.RewardCount > 1);
                CollectAllBtn.SetClick(OnCollectAllBtnClick);
            }

            if (LessTimeText != null)
            {
                IntervalTimer?.Cancel();
                IntervalTimer = this.AttachTimer(1f,
                    () => LessTimeText.text =
                        TimeUtils.Instance.ToDayHourMinuteSecond(Math.Max(0, Root.Instance.MuseumInfo.LessTime))
                    , isLooped: true);
            }
            
            foreach (var row in ItemRow)
            {
                for (int i = 0; i < row.childCount; i++)
                {
                    var data = Root.Instance.MuseumItems[index];
                    if (data == null)
                    {
                        break;
                    }
                    var museumItemMono = row.GetChild(i).GetComponent<MuseumItemMono>();
                    museumItemMono.Order = data.order;
                    index++;
                }    
            }

            for (int i = 0; i < DollarsShow.Length; i++)
            {
                var obj = DollarsShow[i].gameObject;
                
                obj.FindChild<Text>("Text").text = I18N.Get("key_money_count", Root.Instance.MuseumInfo?.GetRowAllDollars(i + 1));
            }
        }


        void OnCollectAllBtnClick()
        {
            if (Root.Instance.MuseumItems is not { Count: > 0})
            {
                return;
            }

            var indexes = new List<int>();
            foreach (var museumItem in Root.Instance.MuseumItems)
            {
                if (museumItem.State == 1)
                {
                    indexes.Add(museumItem.order - 1);
                }
            }
            MediatorRequest.Instance.ClaimMuseumReward(indexes);
        }
        
    }
}