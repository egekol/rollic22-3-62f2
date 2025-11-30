using Core.Scripts.Gameplay.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.Gameplay.Panels
{
    public class FailUI : MonoBehaviour
    {
        [SerializeField] private Button RetryButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private void OnEnable()
        {
            RetryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        private void OnDisable()
        {
            RetryButton.onClick.RemoveListener(OnRetryButtonClicked);
        }

        private void OnRetryButtonClicked()
        {
            GameSettings.Instance.RestartLevel().Forget();
        }

        public void Show()
        {
            _canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, 0.5f).SetLink(gameObject);
        }
    }
}