using UnityEngine;
using UnityEngine.UI;

namespace CleanSlate.Popups
{
    public sealed class NewsPopupView : MonoBehaviour
    {
        [SerializeField] private Text headlineText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Image articleImage;

        public void Bind(NewsEventSO data)
        {
            headlineText.text = data.headline;
            bodyText.text = data.body;
            articleImage.sprite = data.articleImage;
            articleImage.enabled = data.articleImage != null;
        }

        public void Close() => Destroy(gameObject);
    }
}
