using System;
using Core.Extensions.UnityComponent;
using DataAccess.Utils;
using UnityEngine;

namespace UI
{
    public class UIShopComingSoon : MonoBehaviour
    {
        [SerializeField] private MyText timeText;

        private void Update()
        {
            timeText.text = TimeUtils.Instance.FormatTimeToTomorrow();
        }
    }
}