using Core.Scripts.Gameplay.Inputs;
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
            transform.DOScale(Vector3.one, .3f).SetEase(Ease.OutBack).SetDelay(delay).SetLink(gameObject);
        }

        public void HideAnimation(float delay = 0)
        {
            transform.DOScale(Vector3.zero, .2f).SetEase(Ease.InBack).SetDelay(delay)
                .OnComplete(() =>
                {
                    SetEnabled(false);
                }).SetLink(gameObject);
        }

        public UniTaskCompletionSource Move(Vector3 targetPosition, int distance)
        {
            return Move(targetPosition, distance, InputDirection.None);
        }

        public UniTaskCompletionSource Move(Vector3 targetPosition, int distance, InputDirection direction)
        {
            _moveCompletionSource = new UniTaskCompletionSource();
            
            float duration = distance * MovementManager.Instance.MovementDuration;
            float stretchDuration = 0.08f;
            float unstretchDuration = 0.1f;
            
            // Stretch scale based on direction
            Vector3 stretchScale = GetStretchScale(direction);
            
            // Create sequence: stretch -> move -> unstretch
            Sequence sequence = DOTween.Sequence();
            
            // Stretch before movement starts
            sequence.Append(transform.DOScale(stretchScale, stretchDuration).SetEase(Ease.OutQuad));
            
            // Move while stretched
            sequence.Join(transform.DOMove(targetPosition, duration + stretchDuration)
                .SetEase(MovementManager.Instance.Ease));
            
            // Unstretch at the end
            sequence.Append(transform.DOScale(Vector3.one, unstretchDuration).SetEase(Ease.OutBack));
            
            sequence.OnComplete(() => _moveCompletionSource.TrySetResult());
            sequence.SetLink(gameObject);
            
            return _moveCompletionSource;
        }

        private Vector3 GetStretchScale(InputDirection direction)
        {
            float stretchAmount = 1.3f;
            float compressAmount = 0.85f;
            
            return direction switch
            {
                // Up/Down: stretch on Z axis (forward/back in world), compress X
                InputDirection.Up => new Vector3(compressAmount, 1f, stretchAmount),
                InputDirection.Down => new Vector3(compressAmount, 1f, stretchAmount),
                // Left/Right: stretch on X axis, compress Z
                InputDirection.Left => new Vector3(stretchAmount, 1f, compressAmount),
                InputDirection.Right => new Vector3(stretchAmount, 1f, compressAmount),
                _ => Vector3.one
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<MinionView>(out var minion))
            {
                HandleMinionEntered(minion);
                return;
            }

            if (other.TryGetComponent<CollectableItemView>(out var collectable))
            {
                HandleCollectableEntered(collectable);
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

        private void HandleCollectableEntered(CollectableItemView collectable)
        {
            LevelManager.Instance.CollectCollectable(collectable);
        }

        private void OnDestroy()
        {
            // Item yok edildiğinde completion source'u tamamla
            _moveCompletionSource?.TrySetResult();
        }
    }
}