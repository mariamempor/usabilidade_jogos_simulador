using UnityEngine;

namespace CleanSlate.Finance
{
    [CreateAssetMenu(menuName = "Clean Slate/Finance/Facade Company", fileName = "Company_")]
    public sealed class FacadeCompanySO : ScriptableObject
    {
        [Tooltip("Name presented in the dashboard.")]
        public string companyName = "Lavanderia Aurora";
        public Sprite icon;
        [Min(1)] public int maxDailyCapacity = 10000;
        [Range(0f, 1f)] public float lossRate = 0.15f;
        [Tooltip("Suspicion points per currency unit processed.")]
        [Min(0f)] public float suspicionFactor = 0.0005f;
    }
}
