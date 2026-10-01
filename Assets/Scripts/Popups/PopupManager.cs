using System.Collections.Generic;
using CleanSlate.Core;
using DG.Tweening;
using UnityEngine;

namespace CleanSlate.Popups
{
    /// <summary>Selects event assets by current risk and instantiates their presentation prefabs.</summary>
    public sealed class PopupManager : MonoBehaviour
    {
        [SerializeField] private Transform popupLayer;
        [SerializeField] private NewsPopupView newsPopupPrefab;
        [SerializeField] private OfficialPopupView officialPopupPrefab;
        [SerializeField] private List<NewsEventSO> newsEvents = new();
        [SerializeField] private List<PoliceEventSO> officialEvents = new();
        [SerializeField, Range(0f, 1f)] private float dailyNewsChance = .35f;
        [SerializeField, Range(0f, 1f)] private float dailyOfficialChance = .25f;

        private void OnEnable() => GameManager.StateChanged += OnStateChanged;
        private void OnDisable() => GameManager.StateChanged -= OnStateChanged;

        private void OnStateChanged(GameManager.GameState state)
        {
            if (state != GameManager.GameState.Work || GameManager.Instance.CurrentDay == 1) return;
            TryShowDailyEvent();
        }

        private void TryShowDailyEvent()
        {
            float suspicion = GameManager.Instance.Suspicion;
            if (Random.value < dailyOfficialChance && suspicion > 0f)
            {
                PoliceEventSO official = officialEvents.Find(e => e != null && suspicion >= e.minimumSuspicion);
                if (official != null) { ShowOfficial(official); return; }
            }
            if (Random.value < dailyNewsChance)
            {
                NewsEventSO news = newsEvents.Find(e => e != null && suspicion >= e.minimumSuspicion && suspicion <= e.maximumSuspicion);
                if (news != null) ShowNews(news);
            }
        }

        public void ShowNews(NewsEventSO data)
        {
            NewsPopupView view = Instantiate(newsPopupPrefab, popupLayer);
            view.Bind(data);
            view.transform.localScale = Vector3.zero;
            view.transform.DOScale(1f, .25f).SetEase(Ease.OutBack).SetUpdate(true);
            GameManager.Instance.ChangeSuspicion(data.suspicionImpact);
        }

        public void ShowOfficial(PoliceEventSO data)
        {
            OfficialPopupView view = Instantiate(officialPopupPrefab, popupLayer);
            view.Bind(data, choice => ResolveOfficialChoice(data, choice));
            view.transform.localScale = Vector3.zero;
            view.transform.DOScale(1f, .25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void ResolveOfficialChoice(PoliceEventSO data, OfficialChoice choice)
        {
            switch (choice)
            {
                case OfficialChoice.Bribe:
                    GameManager.Instance.AddCleanMoney(-(decimal)data.fineAmount);
                    GameManager.Instance.ChangeSuspicion(-data.bribeRiskReduction);
                    break;
                case OfficialChoice.HideEvidence:
                    GameManager.Instance.ChangeSuspicion(-data.evidenceRiskReduction);
                    break;
                case OfficialChoice.AcceptFine:
                    GameManager.Instance.AddCleanMoney(-(decimal)data.fineAmount);
                    break;
            }
        }
    }
}
