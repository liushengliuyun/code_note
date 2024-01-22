using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace UI.Effect
{
    public class AddHeightCanvas : MonoBehaviour
    {
        [SerializeField]
        private Canvas parent;

        private int addSortingOrder = 1;

        public int sortingOrder
        {
            get
            {
                var component = GetComponent<Canvas>();
                if (component == null)
                {
                    return 0;
                }
                return component.sortingOrder;
            }
        }

        public void SetAddSortingOrder(int value)
        {
            addSortingOrder = value;
            if (TryGetParent()) return;
            var canvas = TryGetCanvas();
            SetSortingOrder(canvas);
        }
        
        private void OnEnable()
        {
            SetNewSorting();
        }

        public void SetNewSorting()
        {
            if (TryGetParent()) return;

            var canvas = TryGetCanvas();

            TryGetComponent<GraphicRaycaster>(out var GraphicRaycaster);

            canvas.overrideSorting = true;
            if (!canvas.overrideSorting)
            {
                this.AttachTimer(0.1f, () =>
                {
                    canvas.overrideSorting = true;
                });
            }
            
            //Everything
            canvas.additionalShaderChannels = (AdditionalCanvasShaderChannels)31;

            SetSortingOrder(canvas);

            if (GraphicRaycaster == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private Canvas TryGetCanvas()
        {
            TryGetComponent<Canvas>(out var canvas);
            canvas ??= gameObject.AddComponent<Canvas>();
            return canvas;
        }

        private void SetSortingOrder(Canvas canvas)
        {
            canvas.sortingOrder = parent.sortingOrder + addSortingOrder;
            if (canvas.sortingOrder != parent.sortingOrder + addSortingOrder)
            {
                this.AttachTimer(0.1f, () =>
                {
                    canvas.sortingOrder = parent.sortingOrder + addSortingOrder;
                });
            }
        }

        private bool TryGetParent()
        {
            parent ??= GetComponentInParent<Canvas>();

            if (parent == null)
            {
                return true;
            }

            return false;
        }

        private void OnDisable()
        {
            DelayedDestroy();
        }

        async UniTask DelayedDestroy()
        {
            TryGetComponent<Canvas>(out var canvas);
            TryGetComponent<GraphicRaycaster>(out var GraphicRaycaster);

            Destroy(GraphicRaycaster);

            await UniTask.WaitUntil(() => GraphicRaycaster == null);

            Destroy(canvas);
        }
    }
}