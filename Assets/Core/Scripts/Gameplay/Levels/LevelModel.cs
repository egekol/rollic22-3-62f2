using UnityEngine;

namespace Core.Scripts.Gameplay.Levels
{
    public class LevelModel
    {
        public int LevelIndex { get; private set; }
        public string LevelName { get; private set; }
        public int TotalMoveCount { get; private set; }
        public int TotalMinionCount { get; private set; }
        public LevelDifficultyType Difficulty { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public LevelTileModel[] Tiles { get; private set; }
        public LevelDataSo LevelData { get; private set; }

        public int RemainingMoveCount { get; private set; }
        public int CollectedCoinCount { get; private set; }
        public int CollectedMinionCount { get; private set; }

        public void InitializeLevel(LevelDataSo levelData)
        {
            LevelData = levelData;
            LevelIndex = levelData.LevelIndex;
            LevelName = levelData.LevelName;
            TotalMoveCount = levelData.MoveCount;
            Difficulty = levelData.Difficulty;
            GridSize = levelData.GridSize;
            SetTileModelList(levelData);
            ReloadLevel();
        }

        private void SetTileModelList(LevelDataSo levelData)
        {
            TotalMinionCount = 0;
            Tiles = new LevelTileModel[levelData.Tiles.Length];
            for (int i = 0; i < levelData.Tiles.Length; i++)
            {
                Tiles[i] = new LevelTileModel
                {
                    Type = levelData.Tiles[i].Type,
                    Coordinates = levelData.Tiles[i].Coordinates
                };
                TryIncreaseMinionCount(levelData, i);
            }
        }

        private void TryIncreaseMinionCount(LevelDataSo levelData, int i)
        {
            if (levelData.Tiles[i].Type == TileType.Minion)
            {
                TotalMinionCount++;
            }
        }

        public void ReloadLevel()
        {
            RemainingMoveCount = TotalMoveCount;
            CollectedCoinCount = 0;
            CollectedMinionCount = 0;
            SetTileModelList(LevelData);
        }
    }

    public class LevelTileModel
    {
        public TileType Type;
        public Vector2Int Coordinates;
    }
}