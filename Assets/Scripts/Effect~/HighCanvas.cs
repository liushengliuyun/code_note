using System.Threading.Tasks;
using JerryMouse.Extensions;
using SlotX.Utils;
using UnityEngine;
using UnityEngine.UI;


namespace SlotX.Effect
{
    public class HighCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        public int addedSortingOrder = 1;

        // public int sortingOrder
        // {
        //     get
        //     {
        //         var component = GetComponent<Canvas>();
        //         if (component == null)
        //         {
        //             return 0;
        //         }
        //         return component.sortingOrder;
        //     }
        // }

        public void SetAddSortingOrder(int value)
        {
            addedSortingOrder = value;
            if (CheckCanvas()) return;
            SetSortingOrder(AddCanvas());
        }
        
        private void OnEnable()
        {
            SetNewSorting();
        }

        public void SetNewSorting()
        {
            if (CheckCanvas()) return;

            var highCanvas = AddCanvas();

            TryGetComponent<GraphicRaycaster>(out var graphicRaycaster);

            highCanvas.overrideSorting = true;
            if (!highCanvas.overrideSorting)
            {
                this.AttachTimer(0.1f, () =>
                {
                    //这些属性立即赋值并不会立即生效
                    highCanvas.overrideSorting = true;
                });
            }
            
            //Everything
            highCanvas.additionalShaderChannels = (AdditionalCanvasShaderChannels)31;

            SetSortingOrder(highCanvas);

            if (graphicRaycaster == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private Canvas AddCanvas()
        {
            var highCanvas = gameObject.TryAddComponent<Canvas>();
            return highCanvas;
        }

        private void SetSortingOrder(Canvas childCanvas)
        {
            var childSortingOrder = canvas.sortingOrder + addedSortingOrder;
            childCanvas.sortingOrder = childSortingOrder;
            childCanvas.sortingLayerID = canvas.sortingLayerID;
            childCanvas.sortingLayerName = canvas.sortingLayerName;
            if (childCanvas.sortingOrder != childSortingOrder)
            {
                this.AttachTimer(0.1f, () =>
                {
                    if (childCanvas != null)
                    {
                        childCanvas.sortingOrder = canvas.sortingOrder + addedSortingOrder;
                        childCanvas.sortingLayerID = canvas.sortingLayerID;
                        childCanvas.sortingLayerName = canvas.sortingLayerName;
                    }
                });
            }
        }

        private bool CheckCanvas()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (canvas == null)
            {
                canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            }
            
            if (canvas == null)
            {
                return true;
            }

            return false;
        }

        private void OnDisable()
        {
            DelayedDestroy();
        }

        async Task DelayedDestroy()
        {
            TryGetComponent<Canvas>(out var canvas);
            TryGetComponent<GraphicRaycaster>(out var graphicRaycaster);

            Destroy(graphicRaycaster);

            await MyTask.WaitUntil(() => graphicRaycaster == null);

            Destroy(canvas);
        }
    }
}