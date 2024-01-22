using System;
using Core.Services.ResourceService.Internal.UniPooling;
using Core.Services.UserInterfaceService.UIExtensions.Scripts.Utilities;
using Jing.TurbochargedScrollList;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Services.UserInterfaceService.Internal
{
    public delegate void ListItemRenderer(int index, GameObject obj);

    public delegate string ListItemProvider(int index);

    public enum ListLayOut
    {
        Vertical,
        Horizontal,
        Grid
    }

    public class MyList
    {
        public ScrollRect ScrollRect;

        public int TopOffset;
        
        private Transform content => ScrollRect.content;

        private RectTransform content_rect => ScrollRect.content;

        private IScrollList VirtualList { get; set; }

        private MyPool pool = new MyPool();

        private string _defaultItem;

        public ListLayOut ListLayout = ListLayOut.Vertical;

        public string defaultItem
        {
            get { return _defaultItem; }
            set { _defaultItem = value; }
        }

        public ListItemRenderer itemRenderer;
        public ListItemProvider itemProvider;


        private Action<PointerEventData> onPullDownRelease;

        /// <summary>
        /// 向下拉然后释放的事件
        /// </summary>
        public Action<PointerEventData> OnPullDownRelease
        {
            set
            {
                if (onPullDownRelease == null)
                {
                    ScrollRect.OnEndDragAsObservable().Subscribe(data =>
                    {
                        if (ScrollRect.verticalNormalizedPosition >= 1)
                        {
                            onPullDownRelease?.Invoke(data);
                        }
                    });
                }

                onPullDownRelease = value;
            }
        }

        public Func<int, Vector2> itemSizeProvider;

        public float lineGap;

        bool _virtual;
        int numItems;

        private bool firstInit;
        private bool _loop;

        public int PaddingTop;
        
        public int PaddingBottom;

        public int NumItems
        {
            get
            {
                if (_virtual)
                    return numItems;
                else
                    return numChildren;
            }
            set
            {
                if (_virtual)
                {
                    numItems = value;
                    VirtualList.NumItems = value;
                }
                else
                {
                    int cnt = numChildren;
                    if (value > cnt)
                    {
                        for (int i = cnt; i < value; i++)
                        {
                            if (itemProvider == null)
                                AddItemFromPool();
                            else
                                AddItemFromPool(itemProvider(i));
                        }
                    }
                    else
                    {
                        RemoveChildrenToPool(value, cnt);
                    }

                    float height = 0f;
                    if (itemRenderer != null)
                    {
                        for (int i = 0; i < value; i++)
                        {
                            var obj = GetChildAt(i);
                            itemRenderer(i, obj);
                            height += obj.GetComponent<RectTransform>().rect.height + lineGap;
                        }
                    }

                    height -= lineGap;
                    height += PaddingBottom;
                    if (ListLayout == ListLayOut.Vertical)
                    {
                        content_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                    }

                    if (!firstInit)
                    {
                        firstInit = true;
                        if (ListLayout == ListLayOut.Vertical)
                        {
                            content_rect.anchoredPosition = new Vector2(content_rect.anchoredPosition.x, -height / 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// gameObject的数量
        /// </summary>
        private int numChildren => content.childCount;

        /// <summary>
        /// 需要有defaultItem 或者ItemProvider ，转化为虚拟列表时， 注意content的Pivot是否是（0，1）
        /// </summary>
        public void SetVirtual()
        {
            switch (ListLayout)
            {
                case ListLayOut.Vertical:
                    VirtualList =
                        new VerticalScrollList(ScrollRect, GetFromPoolByIndex, itemSizeProvider, itemProvider,
                            new VerticalLayoutSettings(PaddingTop, PaddingBottom, lineGap), TopOffset);
                    break;
                case ListLayOut.Horizontal:
                    //todo
                    break;
                case ListLayOut.Grid:
                    break;
            }

            VirtualList.onRenderItem += (ScrollListItem item, object data, bool isFresh) =>
            {
                itemRenderer(item.index, item.gameObject);
            };

            _virtual = true;
        }

        public int ItemIndexToChildIndex(int itemIndex)
        {
            if (_virtual)
            {
                for (int i = 0; i < numChildren; i++)
                {
                    var gameObject = GetChildAt(i);
                    if (!gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    var scrollListItem = gameObject.GetComponent<ScrollListItem>();
                    if (scrollListItem.index == itemIndex)
                    {
                        return i;
                    }
                }

                return -1;
            }

            return itemIndex;
        }
        
        public GameObject GetChildAt(int index)
        {
            if (index >= 0 && index < numChildren)
                return content.GetChild(index).gameObject;
            else
                return null;
        }

        private void RemoveChildrenToPool(int beginIndex, int endIndex)
        {
            if (endIndex < 0 || endIndex >= numChildren)
                endIndex = numChildren - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildToPoolAt(beginIndex);
        }

        private void RemoveChildToPoolAt(int index)
        {
            GetChildAt(index).Restore();
        }

        private void AddItemFromPool(string url = null)
        {
            GameObject obj = GetFromPool(url);

            AddChild(obj.transform);
        }

        private GameObject GetFromPoolByIndex(int index)
        {
            var url = itemProvider != null ? itemProvider(index) : defaultItem;
            return GetFromPool(url);
        }

        private GameObject GetFromPool(string url)
        {
            if (url == null)
            {
                url = defaultItem;
            }

            return pool.Spawner.SpawnSync(url.ToLower()).GameObj;
        }

        public void AddChild(Transform child)
        {
            child.SetParent(content);
            child.localScale = Vector3.one;
        }

        public void Clear()
        {
            if (_virtual)
            {
                VirtualList.Clear();
            }
            else
            {
                RemoveChildrenToPool(0, numChildren - 1);
            }
        }

        /// <summary>
        /// 滑动到指定位置
        /// </summary>
        /// <param name="index"></param>
        /// <param name="anim"></param>
        /// <param name="setFirst"></param>
        public void ScrollToIndex(int index, bool anim = false, bool setFirst = false)
        {
            
        }

        public void ScrollToIndex(int index)
        {
            if (_virtual)
            {
                VirtualList.ScrollToItem(index);
            }
            else
            {
                var child = GetChildAt(index);
                if (child == null)
                {
                    return;
                }
                ScrollRect.ScrollToObject(child.GetComponent<RectTransform>(), true);
            }
        }
        
        public void ScrollToTop()
        {
            if (_virtual)
            {
                VirtualList.ScrollToItem(0);
            }
            else
            {
                ScrollRect.verticalNormalizedPosition = 1;
            }
        }
        
    }
}