using System;
using System.Collections.Generic;
using CleanSlate.Core;
using UnityEngine;

namespace CleanSlate.Finance
{
    /// <summary>Executes daily placement operations and applies the transparent risk formula.</summary>
    public sealed class FinancialManager : MonoBehaviour
    {
        [SerializeField] private List<FacadeCompanySO> companies = new();
        [SerializeField, Range(0f, 1f)] private float activeRiskReduction;

        private readonly Dictionary<FacadeCompanySO, decimal> usedCapacity = new();
        private float suspicionGeneratedToday;

        public event Action<FacadeCompanySO, decimal, decimal> OperationCompleted;

        public bool TryPlace(FacadeCompanySO company, decimal amount)
        {
            if (company == null || amount <= 0m || !GameManager.Instance.CanPerformWork()) return false;
            usedCapacity.TryGetValue(company, out decimal used);
            decimal available = Math.Max(0m, company.maxDailyCapacity - used);
            decimal processed = Math.Min(amount, available);
            if (processed <= 0m || !GameManager.Instance.TrySpendDirtyMoney(processed)) return false;

            usedCapacity[company] = used + processed;
            decimal cleanProfit = processed * (decimal)(1f - company.lossRate);
            suspicionGeneratedToday += (float)processed * company.suspicionFactor;
            GameManager.Instance.AddCleanMoney(cleanProfit);
            OperationCompleted?.Invoke(company, processed, cleanProfit);
            return true;
        }

        public void SetRiskReduction(float reduction) => activeRiskReduction = Mathf.Clamp01(reduction);

        public void ApplyEndOfDayRisk()
        {
            float appliedRisk = suspicionGeneratedToday * (1f - activeRiskReduction);
            GameManager.Instance.ChangeSuspicion(appliedRisk);
            suspicionGeneratedToday = 0f;
            usedCapacity.Clear();
        }
    }
}
