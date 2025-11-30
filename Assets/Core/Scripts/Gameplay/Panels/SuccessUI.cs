using Core.Scripts.Gameplay.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.Gameplay.Panels
{
    public class SuccessUI : MonoBehaviour
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private TextMeshProUGUI _energyCountTMP;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveListener(OnRetryButtonClicked);
        }

        private void OnRetryButtonClicked()
        {
            GameSettings.Instance.LoadNextLevel().Forget();
        }

        public void Show()
        {
            _canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, 0.5f).SetLink(gameObject);
        }
    }
}