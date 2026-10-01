using UnityEngine;

namespace CleanSlate.Popups
{
    [CreateAssetMenu(menuName = "Clean Slate/Events/Official Notification", fileName = "Official_")]
    public sealed class PoliceEventSO : ScriptableObject
    {
        public string institution = "RECEITA FEDERAL";
        public string protocol = "OFÍCIO Nº 000/2026";
        [TextArea] public string subject;
        [TextArea] public string formalBody;
        [Range(0f, 100f)] public float minimumSuspicion = 50f;
        public float fineAmount = 2500f;
        public float bribeRiskReduction = 10f;
        public float evidenceRiskReduction = 18f;
    }
}
