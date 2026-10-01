using CleanSlate.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CleanSlate.UI
{
    /// <summary>One-way binding from game state to dashboard labels.</summary>
    public sealed class DashboardPresenter : MonoBehaviour
    {
        [SerializeField] private Text dirtyBalanceText;
        [SerializeField] private Text cleanBalanceText;
        [SerializeField] private Text suspicionText;
        [SerializeField] private Text daysText;

        private void OnEnable()
        {
            GameManager.Instance.StateValuesChanged += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.StateValuesChanged -= Refresh;
        }
        private void Refresh()
        {
            dirtyBalanceText.text = $"R$ {GameManager.Instance.DirtyBalance:N0}";
            cleanBalanceText.text = $"R$ {GameManager.Instance.CleanBalance:N0}";
            suspicionText.text = $"{GameManager.Instance.Suspicion:0}%";
            daysText.text = $"{GameManager.Instance.DaysRemaining} DIAS";
        }
    }
}
