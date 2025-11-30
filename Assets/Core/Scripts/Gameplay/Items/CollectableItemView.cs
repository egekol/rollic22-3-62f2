using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class CollectableItemView : TileView
    {
        [SerializeField] private ParticleSystem _collectParticle;
        [SerializeField] private float _destroyDelay = 0.3f;

        public void Collect()
        {
            if (_collectParticle != null)
            {
                _collectParticle.Play();
            }

            HideAnimation();
            Destroy(gameObject, _destroyDelay);
        }
    }
}

