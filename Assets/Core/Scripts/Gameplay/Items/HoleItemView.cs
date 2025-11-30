using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class HoleItemView : MonoBehaviour, ITileItem
    {
        [SerializeField] private ParticleSystem _splashParticle;
        
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
            
            float duration = distance * MovementManager.Instance.MovementDuration;
            transform.DOMove(targetPosition, duration)
                .SetEase(MovementManager.Instance.Ease)
                .OnComplete(() => _moveCompletionSource.TrySetResult());
            
            return _moveCompletionSource;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<MinionView>(out var minion))
            {
                HandleMinionEntered(minion);
            }
        }

        private void HandleMinionEntered(MinionView minion)
        {
            // Minion'un hareket completion source'unu hemen tamamla (diğer hareketleri engellememesi için)
            minion.MoveCompletionSource?.TrySetResult();
            
            // LevelManager üzerinden minion'u kaldır
            LevelManager.Instance.RemoveMinion(minion);
            _splashParticle.Play();
        }

        private void OnDestroy()
        {
            // Item yok edildiğinde completion source'u tamamla
            _moveCompletionSource?.TrySetResult();
        }
    }
}