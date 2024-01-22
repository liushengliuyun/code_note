using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Effect
{
    [RequireComponent(typeof(Dropdown))]
    public class YZDroplistExt : MonoBehaviour, IPointerClickHandler
    {
        private Dropdown droplist;
        private int itemCountPerPage;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (droplist != null)
            {
                var intValue = droplist.value;
                //
                if (intValue > 4)
                {
                    var totalOps = droplist.options.Count - itemCountPerPage;
                    if (totalOps <= 0)
                        return;
                    //
                    var scroll = transform.parent.Find("Dropdown List").GetComponent<ScrollRect>();
                    if (scroll != null)
                    {
                        if (intValue >= totalOps)
                        {
                            scroll.verticalNormalizedPosition = 0;
                        }
                        else
                            scroll.verticalNormalizedPosition = 1 - (float)(intValue) / (float)totalOps;
                    }
                }
            }
        }

        private void Awake()
        {
            droplist = transform.GetComponent<Dropdown>();
            //
            var totalY = droplist.template.rect.size.y;
            var itemY = ((RectTransform)droplist.template.Find("Viewport/Content/Item")).rect.size.y;
            itemCountPerPage = Mathf.CeilToInt(totalY / itemY);
        }
    }
}