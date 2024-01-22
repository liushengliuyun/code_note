using UnityEngine;
using UnityEngine.UI;

namespace UI.Effect
{
    public class TextWidthController : MonoBehaviour
    {
        public Text text;
        public Image backgroundImage;

        private void Start()
        {
            ResizeBackground();
        }

        private void ResizeBackground()
        {
            RectTransform textRect = text.GetComponent<RectTransform>();
            RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();

            float textWidth = textRect.rect.width;
            float textHeight = textRect.rect.height;

            backgroundRect.sizeDelta = new Vector2(textWidth, textHeight);
        }
    }

}