using Core.Scripts.Gameplay.Levels;
using UnityEngine;

namespace Core.Scripts.Gameplay.Items
{
    public class HoleItemView : MonoBehaviour, ILocation
    {
        public LevelTileModel LevelTileModel { get; set; }
        public void SetTileModel(LevelTileModel tileModel)
        {
            LevelTileModel = tileModel;
        }
    }
}