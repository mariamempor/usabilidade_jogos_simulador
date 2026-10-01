using System.Collections;
using UnityEngine;

namespace CleanSlate.Core
{
    /// <summary>Minimal code-only pixel sequence. Assign ordered sprites for each routine in the Inspector.</summary>
    public sealed class PixelRoutinePlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] private SpriteRenderer monitorRenderer;
        [SerializeField] private Sprite[] eveningFrames;
        [SerializeField] private Sprite[] morningFrames;
        [SerializeField, Min(0.05f)] private float frameDuration = 0.35f;

        public IEnumerator PlayEvening() => PlayFrames(eveningFrames, false);
        public IEnumerator PlayMorning() => PlayFrames(morningFrames, true);

        private IEnumerator PlayFrames(Sprite[] frames, bool monitorOn)
        {
            monitorRenderer.enabled = monitorOn;
            foreach (Sprite frame in frames)
            {
                characterRenderer.sprite = frame;
                yield return new WaitForSecondsRealtime(frameDuration);
            }
        }
    }
}
