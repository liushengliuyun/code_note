using System;
using JerryMouse.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JerryMouse
{
    public delegate void ListItemRenderer(int index, GameObject obj);

    public delegate string ListItemProvider(int index);

    public enum ListLayOut
    {
        None,
        Vertical,
        Horizontal
    }

    public class XList
    {
        public ScrollRect ScrollRect;
        
        private Transform Content => ScrollRect.content;

        private RectTransform ContentRect => ScrollRect.content;
        
        private string _defaultItem;

        public ListLayOut ListLayout = ListLayOut.None;

        public string defaultItem
        {
            get { return _defaultItem; }
            set { _defaultItem = value; }
        }

        public ListItemRenderer ItemRenderer { get; set; }
        public ListItemProvider ItemProvider { get; set; }

        public GameObject ItemPrefab;

        private Action<PointerEventData> _onPullDownRelease;

        public float LineGap { get; set; }
        
        int _numItems;

        private bool _firstInit;
        private bool _loop;
        
        public int PaddingBottom;

        public int NumItems_Austen
        {
            get { return NumChildren_Austen; }
            set
            {
                int cnt = NumChildren_Austen;
                if (value > cnt)
                {
                    for (int i = cnt; i < value; i++)
                    {
                        if (ItemPrefab != null)
                        {
                            AddChild(GameObject.Instantiate(ItemPrefab).transform);
                        }
                        else
                        {
                            if (ItemProvider == null)
                                AddItemFromPool_Wolfgang();
                            else
                                AddItemFromPool_Wolfgang(ItemProvider(i));
                        }
                    }
                }
                else
                {
                    RemoveChildrenToPool_Fenimore(value, cnt);
                }

                float height = 0f;
                if (ItemRenderer != null)
                {
                    for (int i = 0; i < value; i++)
                    {
                        var obj = GetChildAt(i);
                        ItemRenderer(i, obj);
                        height += obj.GetComponent<RectTransform>().rect.height + LineGap;
                    }
                }

                height -= LineGap;
                height += PaddingBottom;
                if (ListLayout == ListLayOut.Vertical)
                {
                    ContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                }

                if (!_firstInit)
                {
                    _firstInit = true;
                    if (ListLayout == ListLayOut.Vertical)
                    {
                        ContentRect.anchoredPosition = new Vector2(ContentRect.anchoredPosition.x, -height / 2);
                    }
                }
            }
        }

        /// <summary>
        /// gameObject的数量
        /// </summary>
        public int NumChildren_Austen => Content.childCount;
        
        public GameObject GetChildAt(int index)
        {
            if (index >= 0 && index < NumChildren_Austen)
                return Content.GetChild(index).gameObject;
            else
                return null;
        }

        private void RemoveChildrenToPool_Fenimore(int beginIndex, int endIndex)
        {
            if (endIndex < 0 || endIndex >= NumChildren_Austen)
                endIndex = NumChildren_Austen - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildToPoolAt(beginIndex);
        }

        private void RemoveChildToPoolAt(int index)
        {
             GetChildAt(index).Destroy();
        }

        private void AddItemFromPool_Wolfgang(string url = null)
        {
            GameObject obj = GetFromPool(url);

            AddChild(obj.transform);
        }

        private GameObject GetFromPool(string url)
        {
            if (url == null)
            {
                url = defaultItem;
            }

            return ResourceController.Instance.GetObjSync(url, cache: true);
        }

        public void AddChild(Transform child)
        {
            child.SetParent(Content);
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
        }
        
        public void ScrollToIndex(int index)
        {
            var child = GetChildAt(index);
            if (child == null)
            {
                return;
            }
            
            ScrollRect.ScrollToObject(child.GetComponent<RectTransform>(), true);
        }
    }
}