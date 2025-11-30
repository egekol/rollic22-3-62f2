using System;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Managers;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts.Gameplay.Panels
{
    public class InGameUI : Singleton<InGameUI>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private SuccessUI _successUI;
        [SerializeField] private FailUI _failUI;

        [SerializeField] private TextMeshProUGUI _energyCountTMP;
        [SerializeField] private TextMeshProUGUI _moveCountTMP;
        [SerializeField] private TextMeshProUGUI _levelInfoTMP;

        [SerializeField] private Button _retryButton;

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(OnRetryButtonClicked);
        }

        public void ShowAnimation()
        {
            ShowAsync().Forget();
        }

        public void HideAnimation()
        {
            HideAsync().Forget();
        }

        private async UniTask ShowAsync()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.gameObject.SetActive(true);
            await _canvasGroup.DOFade(1, 0.5f);
        }

        private async UniTask HideAsync()
        {
            await _canvasGroup.DOFade(0, 0.5f);
            _canvasGroup.gameObject.SetActive(false);
        }

        public void SetEnabled(bool isEnabled)
        {
            _canvasGroup.gameObject.SetActive(isEnabled);
        }


        public void InitializeHeader(LevelModel levelModel)
        {
            _levelInfoTMP.text = $"{levelModel.LevelName}";
            _moveCountTMP.text = $"{levelModel.RemainingMoveCount}";
            _energyCountTMP.text = $"0";
        }

        private void OnRetryButtonClicked()
        {
            GameSettings.Instance.RetryButtonClicked();
        }
    }
}