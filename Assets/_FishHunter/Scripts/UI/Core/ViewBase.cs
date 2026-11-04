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
        [SerializeField] private CanvasGroup _canvasGroup;

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
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
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            IsVisible = visible;
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }
    }
}
