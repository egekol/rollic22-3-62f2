using Core.Scripts.Gameplay.Levels;
using DG.Tweening;
using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class MinionView : MonoBehaviour, ILocation, ITileItem
    {
        public LevelTileModel LevelTileModel { get; set; }
        public void SetTileModel(LevelTileModel tileModel)
        {
            LevelTileModel = tileModel;
        }
        private void SetEnabled(bool isEnabled)
        {
            gameObject.SetActive(isEnabled);
        }
        public void ShowAnimation(float delay = 0)
        {
            transform.localScale = Vector3.one*.2f;
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
    }
}