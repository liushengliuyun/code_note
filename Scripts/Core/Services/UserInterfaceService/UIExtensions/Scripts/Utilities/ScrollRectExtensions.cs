/// Credit Feaver1968 
/// Sourced from - http://forum.unity3d.com/threads/scroll-to-the-bottom-of-a-scrollrect-in-code.310919/

using UnityEngine;
using UnityEngine.UI;

namespace Core.Services.UserInterfaceService.UIExtensions.Scripts.Utilities
{
    public static class ScrollRectExtensions
    {
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }

        public static void ScrollToObject(this ScrollRect scrollRect, RectTransform target, bool top = false)
        {
            scrollRect.normalizedPosition = CenterPoint(scrollRect, target, top);
        }

        private static Vector2 CenterPoint(ScrollRect worldMap, RectTransform target, bool top)
        {
            var content = worldMap.content.GetComponent<RectTransform>();
            var viewport = worldMap.viewport;
            Vector3 targetPosition =
                worldMap.GetComponent<RectTransform>().InverseTransformPoint(Clear_Pivot_Offset(target));
            Vector3 viewportPosition = worldMap.GetComponent<RectTransform>()
                .InverseTransformPoint(Clear_Pivot_Offset(viewport));
            Vector3 distance_vec = viewportPosition - targetPosition;
            if (top)
            {
                distance_vec = new Vector3(distance_vec.x, distance_vec.y + viewport.rect.height / 2 - target.rect.height / 2, distance_vec.z);
            }

            var height_Delta = content.rect.height - viewport.rect.height;
            var width_Delta = content.rect.width - viewport.rect.width;

            content.TryGetComponent<VerticalLayoutGroup>(out var verticalLayoutGroup);

            float y_offset = verticalLayoutGroup != null ? verticalLayoutGroup.padding.top : 0;
            
            var ratio_x = distance_vec.x / width_Delta;
            var ratio_y = (distance_vec.y - y_offset) / height_Delta;
            var ratioDistance = new Vector2(ratio_x, ratio_y);
            var newPosition = worldMap.normalizedPosition - ratioDistance;
            return new Vector2(Mathf.Clamp01(newPosition.x), Mathf.Clamp01(newPosition.y));
        }

        private static Vector3 Clear_Pivot_Offset(RectTransform rec)
        {
            var offset = new Vector3((0.5f - rec.pivot.x) * rec.rect.width,
                (0.5f - rec.pivot.y) * rec.rect.height, 0.0f);
            var newPosition = rec.localPosition + offset;
            return rec.parent.TransformPoint(newPosition);
        }
    }
}