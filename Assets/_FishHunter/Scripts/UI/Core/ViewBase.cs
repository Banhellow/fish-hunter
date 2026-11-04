using UnityEngine;

namespace FishHunter.UI.Core
{
    /// <summary>
    /// Non-generic base for every screen. Handles visibility through a CanvasGroup so the
    /// <see cref="UIScreenService"/> can manage screens without knowing their ViewModel types.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            SetVisible(true);
            OnShow();
        }

        public void Hide()
        {
            SetVisible(false);
            OnHide();
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            IsVisible = visible;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }
    }
}
