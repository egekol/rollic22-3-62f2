using System;
using Core.Scripts.Gameplay.Events;
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
            GameEvents.OnMoveCountChanged += OnMoveCountChanged;
        }
        
        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(OnRetryButtonClicked);
            GameEvents.OnMoveCountChanged -= OnMoveCountChanged;
        }

        private void OnMoveCountChanged(int remainingMoves)
        {
            _moveCountTMP.text = $"{remainingMoves}";
            PlayMoveCountPunchAnimation();
        }

        private void PlayMoveCountPunchAnimation()
        {
            DOTween.Kill(_moveCountTMP.transform);
            _moveCountTMP.transform.localScale = Vector3.one;
            _moveCountTMP.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f)
                .SetId(_moveCountTMP.transform).SetLink(gameObject);
        }

        public void ShowAnimation()
        {
            ShowAsync().Forget();
        }

        public void HideAnimation()
        {
            HideAsync().Forget();
        }

        public async UniTask ShowAsync()
        {
            DOTween.Kill(transform);
            _canvasGroup.alpha = 0;
            _successUI.gameObject.SetActive(false);
            _failUI.gameObject.SetActive(false);
            _canvasGroup.gameObject.SetActive(true);
            await _canvasGroup.DOFade(1, 0.5f).SetId(transform);
        }

        public async UniTask HideAsync()
        {
            DOTween.Kill(transform);
            await _canvasGroup.DOFade(0, 0.5f).SetId(transform);
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

        public void ShowFailUI()
        {
            _failUI.Show();
        }

        public void ShowSuccessUI()
        {
            _successUI.Show();
        }
    }
}