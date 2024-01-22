using Core.Extensions.UnityComponent;
using UnityEngine;
using Utils;

namespace UI.UIChargeFlow
{
    public class ChargeDiscountMono : MonoBehaviour
    {
        [SerializeField] private MyText offText;
        [SerializeField] private MyText newPriceText;

        public void Init(int off, float newPrice)
        {
            offText.text = off + "%\noff";
            newPriceText.text = "$" + YZNumberUtil.FormatYZMoney(newPrice.ToString());
        }
    }
}