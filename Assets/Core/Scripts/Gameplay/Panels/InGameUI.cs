using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Scripts.Gameplay.Panels
{
    public class InGameUI : Singleton<InGameUI>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        
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
            
        }
    }
}