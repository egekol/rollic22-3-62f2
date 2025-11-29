using AssetKits.ParticleImage;
using Core.Scripts.Lib.Utility;
using DG.Tweening;
using Lib.CustomAttributes.Scripts;
using UnityEngine;

namespace Core.Scripts.Gameplay.Panels
{
    public class BackgroundUI : Singleton<BackgroundUI>
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private ParticleImage _particleImage;
        
        [ProButton]
        public void PlayParticleFaster()
        {
            DOTween.Kill(transform);
            DOTween.To(() => _particleImage.trailRatio, x => _particleImage.trailRatio = x, 1, 0.5f).SetId(transform);
            SetSpeedOverLifetimeDotween(10);
            SetRatePerSecondDotween(20);
        }
        
        [ProButton]
        public void PlayParticleSlower()
        {
            DOTween.Kill(transform);
            DOTween.To(() => _particleImage.trailRatio, x => _particleImage.trailRatio = x, 0, 0.5f).SetId(transform);
            SetSpeedOverLifetimeDotween(1);
            SetRatePerSecondDotween(4);
        }

        private void SetSpeedOverLifetimeDotween(float f)
        {
            DOTween.To(() => _particleImage.speedOverLifetime.constant, (x) =>
                {
                    var main = _particleImage.main;
                    main.speedOverLifetime = x;
                }, f, 0.5f)
                .SetId(transform);
        }
        private void SetRatePerSecondDotween(float f)
        {
            DOTween.To(() => _particleImage.rateOverTime, (x) =>
                {
                    var main = _particleImage.main;
                    main.rateOverTime = x;
                }, f, 0.5f)
                .SetId(transform);
        }
    }
}