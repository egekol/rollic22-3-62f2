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

        public void BlockViewWithCanvas()
        {
            _canvas.planeDistance = 10;
        }

        public void UnblockViewWithCanvas()
        {
            _canvas.planeDistance = 100;
        }

        [ProButton]
        public void PlayParticleFaster()
        {
            DOTween.Kill(transform);
            DOTween.To(() => _particleImage.trailRatio, x => _particleImage.trailRatio = x, 1, 0.8f).SetId(transform);
            SetSpeedOverLifetimeDotween(10f, 0.8f);
            SetRatePerSecondDotween(20f, .8f);
        }

        [ProButton]
        public void PlayParticleSlower()
        {
            DOTween.Kill(transform);
            _particleImage.trailRatio = 0;
            SetSpeedOverLifetimeDotween(1);
            SetRatePerSecondDotween(2);
        }

        private void SetSpeedOverLifetimeDotween(float f, float duration = 0.5f)
        {
            DOTween.To(() => _particleImage.speedOverLifetime.constant, (x) =>
                {
                    var main = _particleImage.main;
                    main.speedOverLifetime = x;
                }, f, duration)
                .SetId(transform);
        }

        private void SetRatePerSecondDotween(float f, float duration = 0.5f)
        {
            DOTween.To(() => _particleImage.rateOverTime, (x) =>
                {
                    var main = _particleImage.main;
                    main.rateOverTime = x;
                }, f, duration)
                .SetId(transform);
        }
    }
}