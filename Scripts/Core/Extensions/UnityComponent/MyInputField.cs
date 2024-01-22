using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Extensions.UnityComponent
{
    public class MyInputField : InputField
    {
        public UnityEvent onSelect;

        public UnityEvent onDisSelect;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            onSelect?.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            onDisSelect?.Invoke();
        }
    }
}