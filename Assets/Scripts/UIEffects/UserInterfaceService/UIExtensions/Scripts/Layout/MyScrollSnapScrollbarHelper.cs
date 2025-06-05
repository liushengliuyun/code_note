/// Credit Anonymous donation
/// Sourced from - https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/issues/120/horizontal-scroll-snap-scroll-bar-fix
/// Updated by simonDarksideJ - Made extension support all types of scroll snap

using UnityEngine;
using UnityEngine.EventSystems;

namespace JerryMouse.UI
{
    [DisallowMultipleComponent]
    public class MyScrollSnapScrollbarHelper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        internal IScrollPage ScrollPage;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnScrollBarDown();
        }

        public void OnDrag(PointerEventData eventData)
        {
            ScrollPage.GetCurrentPageId();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnScrollBarUp();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnScrollBarDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnScrollBarUp();
        }

        void OnScrollBarDown()
        {
            if (ScrollPage != null)
            {
                ScrollPage.Thales_SetLerp_Socrates(false);
                ScrollPage.StartScreenChange();
            }
        }

        void OnScrollBarUp()
        {
            ScrollPage.Thales_SetLerp_Socrates(true);
            ScrollPage.Ludwig_ToPage_Wittgenstein(ScrollPage.GetCurrentPageId());
        }
    }
}