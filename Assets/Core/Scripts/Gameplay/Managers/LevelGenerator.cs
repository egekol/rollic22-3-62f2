using System.Collections.Generic;
using Core.Scripts.Gameplay.Items;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using UnityEngine;

namespace Core.Scripts.Gameplay.Managers
{
    public class LevelGenerator : Singleton<LevelGenerator>
    {
        [SerializeField] private Transform _centerPoint;
        [SerializeField] private float _sizeOfTile;
        
        [Header("Prefabs")]
        [SerializeField] private FloorTileView _floorTilePrefab;
        [SerializeField] private WallTileView _wallTileView;
        [SerializeField] private SpikeTileView _spikeTileView;
        [SerializeField] private HoleItemView _holeItemView;
        [SerializeField] private MinionView _minionView;

        [Header("Containers")]
        [SerializeField] private Transform _tilesContainer;
        [SerializeField] private Transform _itemsContainer;

        // Tile Lists
        private List<FloorTileView> _floorTiles = new List<FloorTileView>();
        private List<WallTileView> _wallTiles = new List<WallTileView>();
        private List<SpikeTileView> _spikeTiles = new List<SpikeTileView>();
        
        // Item Lists
        private List<HoleItemView> _holeItems = new List<HoleItemView>();
        private List<MinionView> _minions = new List<MinionView>();

        public IReadOnlyList<FloorTileView> FloorTiles => _floorTiles;
        public IReadOnlyList<WallTileView> WallTiles => _wallTiles;
        public IReadOnlyList<SpikeTileView> SpikeTiles => _spikeTiles;
        public IReadOnlyList<HoleItemView> HoleItems => _holeItems;
        public IReadOnlyList<MinionView> Minions => _minions;

        public void GenerateLevel(LevelModel levelModel)
        {
            ClearLevel();
            
            Vector2 gridCenter = CalculateGridCenter(levelModel.GridSize);
            
            foreach (var tileModel in levelModel.Tiles)
            {
                Vector3 worldPosition = CalculateWorldPosition(tileModel.Coordinates, gridCenter);
                SpawnTile(tileModel, worldPosition);
            }
        }

        private Vector2 CalculateGridCenter(Vector2Int gridSize)
        {
            float centerX = (gridSize.x - 1) * 0.5f;
            float centerY = (gridSize.y - 1) * 0.5f;
            return new Vector2(centerX, centerY);
        }

        private Vector3 CalculateWorldPosition(Vector2Int coordinates, Vector2 gridCenter)
        {
            float x = (coordinates.x - gridCenter.x) * _sizeOfTile;
            float z = (gridCenter.y - coordinates.y) * _sizeOfTile; // Y koordinatı ters çevrildi (CSV yukarıdan aşağı)
            return _centerPoint.position + new Vector3(x, 0, z);
        }

        private void SpawnTile(LevelTileModel tileModel, Vector3 position)
        {
            switch (tileModel.Type)
            {
                case TileType.Floor:
                    SpawnFloorTile(tileModel, position);
                    break;
                case TileType.Wall:
                    SpawnWallTile(tileModel, position);
                    break;
                case TileType.Minion:
                    SpawnFloorTile(tileModel, position); // Minion altına floor ekle
                    SpawnMinion(tileModel, position);
                    break;
                case TileType.Hole:
                    SpawnHoleItem(tileModel, position);
                    break;
                case TileType.Spike:
                    SpawnSpikeTile(tileModel, position);
                    break;
                case TileType.Ice:
                    SpawnFloorTile(tileModel, position); // Ice için şimdilik floor kullan
                    break;
                case TileType.Collectable:
                    SpawnFloorTile(tileModel, position); // Collectable altına floor ekle
                    break;
            }
        }

        private void SpawnFloorTile(LevelTileModel tileModel, Vector3 position)
        {
            var floorTile = Instantiate(_floorTilePrefab, position, Quaternion.identity, _tilesContainer);
            floorTile.SetTileModel(tileModel);
            _floorTiles.Add(floorTile);
        }

        private void SpawnWallTile(LevelTileModel tileModel, Vector3 position)
        {
            var wallTile = Instantiate(_wallTileView, position, Quaternion.identity, _tilesContainer);
            wallTile.SetTileModel(tileModel);
            _wallTiles.Add(wallTile);
        }

        private void SpawnSpikeTile(LevelTileModel tileModel, Vector3 position)
        {
            var spikeTile = Instantiate(_spikeTileView, position, Quaternion.identity, _tilesContainer);
            spikeTile.SetTileModel(tileModel);
            _spikeTiles.Add(spikeTile);
        }

        private void SpawnHoleItem(LevelTileModel tileModel, Vector3 position)
        {
            var holeItem = Instantiate(_holeItemView, position, Quaternion.identity, _itemsContainer);
            holeItem.SetTileModel(tileModel);
            _holeItems.Add(holeItem);
        }

        private void SpawnMinion(LevelTileModel tileModel, Vector3 position)
        {
            var minion = Instantiate(_minionView, position, Quaternion.identity, _itemsContainer);
            minion.SetTileModel(tileModel);
            _minions.Add(minion);
        }

        public void ClearLevel()
        {
            ClearList(_floorTiles);
            ClearList(_wallTiles);
            ClearList(_spikeTiles);
            ClearList(_holeItems);
            ClearList(_minions);
        }

        private void ClearList<T>(List<T> list) where T : MonoBehaviour
        {
            foreach (var item in list)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            list.Clear();
        }
    }
}