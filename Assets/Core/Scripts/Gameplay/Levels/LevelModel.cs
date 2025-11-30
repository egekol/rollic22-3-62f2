using System.Collections.Generic;
using Lib.Debugger;
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
        public LevelTileModel[,] TilesGrid { get; private set; }
        public LevelDataSo LevelData { get; private set; }

        public int RemainingMoveCount { get; private set; }
        public int CollectedCoinCount { get; private set; }
        public int CollectedMinionCount { get; private set; }
        public bool HasMinionDied { get; set; }

        // Dictionary to access tiles by their unique ID
        private Dictionary<int, LevelTileModel> _tilesById = new Dictionary<int, LevelTileModel>();
        public IReadOnlyDictionary<int, LevelTileModel> TilesById => _tilesById;

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
            _tilesById.Clear();
            TotalMinionCount = 0;
            
            // Initialize TilesGrid with GridSize dimensions
            TilesGrid = new LevelTileModel[GridSize.x, GridSize.y];
            
            var tileList = new List<LevelTileModel>();
            var idCounter = 0;
            
            for (int i = 0; i < levelData.Tiles.Length; i++)
            {
                var tileData = levelData.Tiles[i];
                
                // Hole, Minion, Collectable, Spike altına floor ekle
                if (RequiresFloorUnderneath(tileData.Type))
                {
                    var floorTile = new LevelTileModel(idCounter++, TileType.Floor, tileData.Coordinates);
                    tileList.Add(floorTile);
                    _tilesById[floorTile.Id] = floorTile;
                    TilesGrid[tileData.Coordinates.x, tileData.Coordinates.y] = floorTile;
                }
                
                var tileModel = new LevelTileModel(idCounter++, tileData.Type, tileData.Coordinates);
                tileList.Add(tileModel);
                _tilesById[tileModel.Id] = tileModel;
                // Üstteki tile her zaman grid'de görünür (floor üstüne yazılır)
                TilesGrid[tileData.Coordinates.x, tileData.Coordinates.y] = tileModel;
                
                if (tileData.Type == TileType.Minion)
                {
                    TotalMinionCount++;
                }
            }
            
            Tiles = tileList.ToArray();
        }

        private bool RequiresFloorUnderneath(TileType type)
        {
            return type == TileType.Hole || 
                   type == TileType.Minion || 
                   type == TileType.Collectable || 
                   type == TileType.Spike;
        }

        public LevelTileModel GetTileById(int id)
        {
            return _tilesById.TryGetValue(id, out var tile) ? tile : null;
        }

        public void RemoveTile(int id)
        {
            if (_tilesById.TryGetValue(id, out var tile))
            {
                _tilesById.Remove(id);
                
                // TilesGrid'den de kaldır (eğer bu tile grid'de ise)
                var coords = tile.Coordinates;
                if (coords.x >= 0 && coords.x < GridSize.x && 
                    coords.y >= 0 && coords.y < GridSize.y &&
                    TilesGrid[coords.x, coords.y] == tile)
                {
                    TilesGrid[coords.x, coords.y] = null;
                }
            }
        }

        public void ReloadLevel()
        {
            RemainingMoveCount = TotalMoveCount;
            CollectedCoinCount = 0;
            CollectedMinionCount = 0;
            SetTileModelList(LevelData);
        }

        public void DecreaseMoveCount()
        {
            RemainingMoveCount = Mathf.Max(0, RemainingMoveCount - 1);
            LogHelper.Log($"Remaining Moves: {RemainingMoveCount}");
        }
    }
}