using System.Collections;
using UnityEngine;

namespace CleanSlate.Core
{
    /// <summary>Native Unity UI transitions that use unscaled time and require no tweening package.</summary>
    public static class UiTransitionUtility
    {
        public static IEnumerator Fade(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            if (duration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        public static IEnumerator ScaleIn(Transform target, float duration, float overshoot = 1.08f)
        {
            if (target == null) yield break;

            if (duration <= 0f)
            {
                target.localScale = Vector3.one;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.LerpUnclamped(0f, 1f, EaseOutBack(normalizedTime, overshoot));
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        private static float EaseOutBack(float value, float overshoot)
        {
            float shifted = value - 1f;
            return 1f + (overshoot + 1f) * shifted * shifted * shifted + overshoot * shifted * shifted;
        }
    }
}
