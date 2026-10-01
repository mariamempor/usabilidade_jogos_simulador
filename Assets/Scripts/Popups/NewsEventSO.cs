using UnityEngine;

namespace CleanSlate.Popups
{
    [CreateAssetMenu(menuName = "Clean Slate/Events/News Event", fileName = "News_")]
    public sealed class NewsEventSO : ScriptableObject
    {
        [TextArea] public string headline;
        [TextArea] public string body;
        public Sprite articleImage;
        [Range(0f, 100f)] public float minimumSuspicion;
        [Range(0f, 100f)] public float maximumSuspicion = 100f;
        [Tooltip("Direct suspicion change when this headline is published.")]
        public float suspicionImpact;
    }
}
