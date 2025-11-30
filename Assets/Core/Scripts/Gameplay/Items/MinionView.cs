using Core.Scripts.Gameplay.Inputs;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lib.Animation.Animator;
using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class MinionView : MonoBehaviour, ITileItem
    {
        [SerializeField] private Animator _animator;
        
        private AnimatorPlayer _animatorPlayer;
        private UniTaskCompletionSource _moveCompletionSource;
        
        public LevelTileModel LevelTileModel { get; set; }
        public UniTaskCompletionSource MoveCompletionSource => _moveCompletionSource;

        private void Start()
        {
            _animatorPlayer = new AnimatorPlayer(_animator, new MinionAnimationLibrary());
            PlayIdle();
        }

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
            transform.DOScale(Vector3.zero, .1f).SetEase(Ease.InBack).SetDelay(delay)
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
            
            PlayMovementAnimation(direction, duration);
            
            transform.DOMove(targetPosition, duration)
                .SetEase(MovementManager.Instance.Ease)
                .OnComplete(() =>
                {
                    SetResultWithDelay().Forget();
                    PlayIdleAfterDelay(0).Forget();
                });
            
            return _moveCompletionSource;
        }

        private async UniTask SetResultWithDelay()
        {
            await UniTask.WaitForSeconds(.1f);
            _moveCompletionSource.TrySetResult();
        }

        private void PlayMovementAnimation(InputDirection direction, float duration)
        {
            string animationType = direction switch
            {
                InputDirection.Up => MinionAnimationType.SlidingBack,
                InputDirection.Down => MinionAnimationType.SlidingFront,
                InputDirection.Left => MinionAnimationType.SlidingSideLeft,
                InputDirection.Right => MinionAnimationType.SlidingSideRight,
                _ => MinionAnimationType.Idle
            };
            
            _animatorPlayer.PlayAnimationByType(animationType, false);
        }

        private async UniTaskVoid PlayIdleAfterDelay(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            PlayIdle();
        }

        public void PlayIdle()
        {
            _animatorPlayer.PlayAnimationByType(MinionAnimationType.Idle, false);
        }

        private void OnDestroy()
        {
            // Item yok edildiğinde completion source'u tamamla
            _moveCompletionSource?.TrySetResult();
        }

        public void Kill()
        {
            HideAnimation();
            Destroy(gameObject, 1f);
        }
    }
}