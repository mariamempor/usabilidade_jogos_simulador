using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleanSlate.Popups
{
    public sealed class OfficialPopupView : MonoBehaviour
    {
        [SerializeField] private Text institutionText;
        [SerializeField] private Text protocolText;
        [SerializeField] private Text subjectText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button bribeButton;
        [SerializeField] private Button hideEvidenceButton;
        [SerializeField] private Button acceptFineButton;
        private Action<OfficialChoice> onChoice;

        public void Bind(PoliceEventSO data, Action<OfficialChoice> choiceCallback)
        {
            institutionText.text = data.institution;
            protocolText.text = data.protocol;
            subjectText.text = data.subject;
            bodyText.text = data.formalBody;
            onChoice = choiceCallback;
            bribeButton.onClick.AddListener(() => Choose(OfficialChoice.Bribe));
            hideEvidenceButton.onClick.AddListener(() => Choose(OfficialChoice.HideEvidence));
            acceptFineButton.onClick.AddListener(() => Choose(OfficialChoice.AcceptFine));
        }

        private void Choose(OfficialChoice choice) { onChoice?.Invoke(choice); Destroy(gameObject); }
    }

    public enum OfficialChoice { Bribe, HideEvidence, AcceptFine }
}
