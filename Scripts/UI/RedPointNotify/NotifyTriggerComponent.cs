using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCat
{

    public class NotifyTriggerComponent : MonoBehaviour
    {
        public ERedPointItem ItemType;

        public void _AddOne()
        {
            _Add(1);
        }

        public void _SubtractOne()
        {
            _Add(-1);
        }

        public void _Add(int count)
        {
            RedPointNotify.AddMark(ItemType, count);
        }

        public void _Reset()
        {
            RedPointNotify.ClearMark(ItemType);
        }
    }

} // namespace