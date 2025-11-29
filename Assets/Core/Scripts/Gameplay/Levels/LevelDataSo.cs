using UnityEngine;

namespace Core.Scripts.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Core/LevelDataSo", order = 0)]
    public class LevelDataSo : ScriptableObject
    {
        public string LevelName;
        public int LevelIndex;
        public int MoveCount;
        public LevelDifficultyType Difficulty;
        public Vector2Int GridSize;
        public LevelTileData[] Tiles;
    }
}