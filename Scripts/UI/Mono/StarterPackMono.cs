using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    public class StarterPackMono : MonoBehaviour
    {
        public Text TimeText;

        public void Init()
        {
            var level = Root.Instance.StarterPackInfo.starter_pack_level;

            Root.Instance.StarterPackConfig.TryGetValue(level, out var listData);

            if (listData == null)
            {
                return;
            }

            var data = listData[^1];
            gameObject.FindChild<MyButton>("Buy").SetClick(() =>
            {
                Buy(data);
            });

            GetComponent<MyButton>().SetClick(() =>
            {
                Buy(data);
            });
            
            gameObject.FindChild<Text>("Buy/BuyText").text = I18N.Get("key_starterpacker_price", data.amount);
            gameObject.FindChild<Text>("data/cash group/cash text").text = I18N.Get("key_money_count", data.amount);
            gameObject.FindChild<Text>("data/bonus group/bonusText").text = I18N.Get("key_money_count", data.ShowBonus);
            gameObject.FindChild<Text>("data/coin group/coinText").text = data.ShowGems.ToString();
            
            gameObject.FindChild<Text>("mark group/Text mark").text = data.MoreValue + "%";
        }

        void Buy(ChargeGoodInfo data)
        {
            data.position = "StarterPack";
            MediatorRequest.Instance.Charge(data, ActivityType.StartPacker);
        }
    }
}