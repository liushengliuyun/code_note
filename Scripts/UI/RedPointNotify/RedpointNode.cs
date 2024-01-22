using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NCat
{

    public class RedpointNode : MonoBehaviour
    {
        public List<ERedPointItem> Subscribe;

        public GameObject RedpointGroup;
        public Text CountText;
        public TMP_Text CountTextPro;

        // Start is called before the first frame update
        void Awake()
        {
            RedPointNotify.OnRedPointChange += OnRedPointChange;
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void OnDestroy()
        {
            RedPointNotify.OnRedPointChange -= OnRedPointChange;
        }

        bool HasSubscribe(ERedPointItem item)
        {
            return Subscribe.Contains(item);
        }

        int TotalCount()
        {
            int total = 0;
            for (int i = 0; i < Subscribe.Count; i++)
            {
                total += RedPointNotify.GetCount(Subscribe[i]);
            }
            return total;
        }

        private void RefreshUI()
        {
            int totalCount = TotalCount();
            string outStr = "!";
            if (totalCount > 1)
            {
                outStr = totalCount.ToString();
            }

            if (CountText != null)
            {
                CountText.text = outStr;
            }

            if (CountTextPro != null)
            {
                CountTextPro.text = outStr;
            }

            if (RedpointGroup != null)
            {
                bool on = totalCount > 0;
                if (RedpointGroup.activeSelf != on)
                {
                    RedpointGroup.SetActive(on);
                }
            }
        }

        private void OnRedPointChange(ERedPointItem item)
        {
            if (HasSubscribe(item))
            {
                RefreshUI();
            }
        }
    }

} // namespace