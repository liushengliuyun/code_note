using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NCat
{

    public static class RedPointNotify
    {
        public static Action<ERedPointItem> OnRedPointChange;

        static Dictionary<ERedPointItem, int> RedPointCounter = new Dictionary<ERedPointItem, int>();

        public static void Init(ERedPointItem item)
        {
            if (RedPointCounter.ContainsKey(item))
            {
                RedPointCounter.Remove(item);
            }

            OnRedPointChange?.Invoke(item);
        }

        public static void AddMark(ERedPointItem item, int count)
        {
            if (RedPointCounter.ContainsKey(item))
            {
                RedPointCounter[item] = Mathf.Max(0, RedPointCounter[item] + count);
            }
            else
            {
                RedPointCounter[item] = Mathf.Max(0, count);
            }

            OnRedPointChange?.Invoke(item);
        }

        public static void SetMark(ERedPointItem item, int count)
        {
            RedPointCounter[item] = count;

            OnRedPointChange?.Invoke(item);
        }

        public static void ClearMark(ERedPointItem item)
        {
            RedPointCounter[item] = 0;

            OnRedPointChange?.Invoke(item);
        }

        public static int GetCount(ERedPointItem item)
        {
            if (RedPointCounter.ContainsKey(item))
            {
                return RedPointCounter[item];
            }

            return 0;
        }
    }

} // namespace
