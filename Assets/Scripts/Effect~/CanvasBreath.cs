using UnityEngine;

namespace SlotX.Effect
{
    public class CanvasBreath : MonoBehaviour
    {
        public CanvasGroup targetCanvasGroup;
        public float fadeOutTime = 0.2f; // 淡出时间，alpha 从 1 到 0
        public float fadeInTime = 0.2f;  // 淡入时间，alpha 从 0 到 1
        public float fullAlphaDuration = 2f; // 保持显示的时间，alpha 为 1
        public float hideDuration = 10f; // 消失的时间，alpha 为 0

        private float timer;
        private enum FadeState
        {
            FadingIn,
            FullAlpha,
            FadingOut,
            Hidden
        }
        private FadeState currentState = FadeState.FadingIn;

        void Start()
        {
            if (targetCanvasGroup == null)
            {
                targetCanvasGroup = GetComponent<CanvasGroup>();
                
                if (targetCanvasGroup == null)
                {
                    Debug.LogError("Target CanvasGroup is not assigned!");
                    enabled = false;
                    return;
                }                
                
            }
            targetCanvasGroup.alpha = 0;
        }

        void Update()
        {
            timer += Time.deltaTime;

            switch (currentState)
            {
                case FadeState.FadingIn:
                    targetCanvasGroup.alpha = Mathf.Clamp01(timer / fadeInTime);
                    if (timer >= fadeInTime)
                    {
                        targetCanvasGroup.alpha = 1;
                        timer = 0;
                        currentState = FadeState.FullAlpha;
                    }
                    break;
                case FadeState.FullAlpha:
                    if (timer >= fullAlphaDuration)
                    {
                        timer = 0;
                        currentState = FadeState.FadingOut;
                    }
                    break;
                case FadeState.FadingOut:
                    targetCanvasGroup.alpha = 1 - Mathf.Clamp01(timer / fadeOutTime);
                    if (timer >= fadeOutTime)
                    {
                        targetCanvasGroup.alpha = 0;
                        timer = 0;
                        currentState = FadeState.Hidden;
                    }
                    break;
                case FadeState.Hidden:
                    if (timer >= hideDuration)
                    {
                        timer = 0;
                        currentState = FadeState.FadingIn;
                    }
                    break;
            }
        }
    }
}