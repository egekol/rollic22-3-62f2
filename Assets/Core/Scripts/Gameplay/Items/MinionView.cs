using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class MinionView : MonoBehaviour, ITileItem
    {
        private UniTaskCompletionSource _moveCompletionSource;
        
        public LevelTileModel LevelTileModel { get; set; }
        public UniTaskCompletionSource MoveCompletionSource => _moveCompletionSource;
        
        public void SetTileModel(LevelTileModel tileModel)
        {
            LevelTileModel = tileModel;
        }

        public void SetEnabled(bool isEnabled)
        {
            gameObject.SetActive(isEnabled);
        }
        
        public void ShowAnimation(float delay = 0)
        {
            transform.localScale = Vector3.zero;
            SetEnabled(true);
            transform.DOScale(Vector3.one, .3f).SetEase(Ease.OutBack).SetDelay(delay);
        }

        public void HideAnimation(float delay = 0)
        {
            transform.DOScale(Vector3.zero, .2f).SetEase(Ease.InBack).SetDelay(delay)
                .OnComplete(() =>
            {
                SetEnabled(false);
            });
        }

        public UniTaskCompletionSource Move(Vector3 targetPosition, int distance)
        {
            _moveCompletionSource = new UniTaskCompletionSource();
            
            float duration = distance * MovementManager.Instance.MovementDuration; // Her birim için 0.1 saniye
            transform.DOMove(targetPosition, duration)
                .SetEase(MovementManager.Instance.Ease)
                .OnComplete(() => _moveCompletionSource.TrySetResult());
            
            return _moveCompletionSource;
        }

        private void OnDestroy()
        {
            // Item yok edildiğinde completion source'u tamamla
            _moveCompletionSource?.TrySetResult();
        }
    }
}