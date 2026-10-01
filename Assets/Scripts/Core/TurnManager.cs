using System.Collections;
using CleanSlate.Finance;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CleanSlate.Core
{
    /// <summary>Owns the non-interactive end-of-day sequence; pixel animation is driven by sprite steps, not Animator.</summary>
    public sealed class TurnManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CanvasGroup workCanvas;
        [SerializeField] private GameObject pixelRoomRoot;
        [SerializeField] private CanvasGroup blackOverlay;
        [SerializeField] private Text dayLabel;
        [SerializeField] private PixelRoutinePlayer pixelRoutine;
        [SerializeField] private FinancialManager financialManager;
        [Header("Timing")]
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.45f;
        [SerializeField, Min(0.1f)] private float dayCardDuration = 1.5f;

        public void EndDayButtonPressed()
        {
            if (!GameManager.Instance.CanPerformWork()) return;
            StartCoroutine(PlayDayTransition());
        }

        private IEnumerator PlayDayTransition()
        {
            GameManager.Instance.BeginDayTransition();
            financialManager.ApplyEndOfDayRisk();
            if (GameManager.Instance.State == GameManager.GameState.GameOver) yield break;

            workCanvas.DOFade(0f, fadeDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(fadeDuration);
            workCanvas.interactable = false;
            workCanvas.blocksRaycasts = false;
            pixelRoomRoot.SetActive(true);

            yield return pixelRoutine.PlayEvening();
            blackOverlay.DOFade(1f, fadeDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(fadeDuration);

            int nextDay = GameManager.Instance.CurrentDay + 1;
            dayLabel.text = $"DIA {nextDay} - 07:00 AM";
            yield return new WaitForSecondsRealtime(dayCardDuration);
            yield return pixelRoutine.PlayMorning();

            blackOverlay.DOFade(0f, fadeDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(fadeDuration);
            pixelRoomRoot.SetActive(false);
            workCanvas.DOFade(1f, fadeDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(fadeDuration);
            workCanvas.interactable = true;
            workCanvas.blocksRaycasts = true;
            GameManager.Instance.StartNextDay();
        }
    }
}
