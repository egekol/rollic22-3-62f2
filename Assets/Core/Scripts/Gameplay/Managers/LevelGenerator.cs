using System.Collections.Generic;
using Core.Scripts.Gameplay.Items;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using Lib.CustomAttributes.Scripts;
using UnityEngine;

namespace Core.Scripts.Gameplay.Managers
{
    public interface ILevelGenerator
    {
        IReadOnlyDictionary<int, FloorTileView> FloorTiles { get; }
        IReadOnlyDictionary<int, WallTileView> WallTiles { get; }
        IReadOnlyDictionary<int, SpikeTileView> SpikeTiles { get; }
        IReadOnlyDictionary<int, HoleItemView> HoleItems { get; }
        IReadOnlyDictionary<int, MinionView> Minions { get; }
        IReadOnlyDictionary<int, ITileItem> AllLocationItems { get; }
        void GenerateLevel(LevelModel levelModel);
    }

    public class LevelGenerator : Singleton<LevelGenerator>, ILevelGenerator
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

        // Tile Dictionaries (key: LevelTileModel.Id)
        private Dictionary<int, FloorTileView> _floorTiles = new();
        private Dictionary<int, WallTileView> _wallTiles = new();
        private Dictionary<int, SpikeTileView> _spikeTiles = new();
        
        // Item Dictionaries (key: LevelTileModel.Id)
        private Dictionary<int, HoleItemView> _holeItems = new();
        private Dictionary<int, MinionView> _minions = new();

        // All location items by ID
        private Dictionary<int, ITileItem> _allLocationItems = new();

        public IReadOnlyDictionary<int, FloorTileView> FloorTiles => _floorTiles;
        public IReadOnlyDictionary<int, WallTileView> WallTiles => _wallTiles;
        public IReadOnlyDictionary<int, SpikeTileView> SpikeTiles => _spikeTiles;
        public IReadOnlyDictionary<int, HoleItemView> HoleItems => _holeItems;
        public IReadOnlyDictionary<int, MinionView> Minions => _minions;
        public IReadOnlyDictionary<int, ITileItem> AllLocationItems => _allLocationItems;

        [ProButton]
        public void TestGenerateLevel(int index)
        {
            LevelManager.Instance.LoadLevel(index);
            GenerateLevel(LevelManager.Instance.LevelModel);
            PlayShowAnimations().Forget();
        }
        
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

        public async UniTask PlayShowAnimations()
        {
            float delayPerDiagonal = 0.03f;

            float delay = 0f;
            foreach (var item in _allLocationItems.Values)
            {
                var coords = item.LevelTileModel.Coordinates;
                // Diagonal distance from top-left corner (0,0)
                int diagonalDistance = coords.x + coords.y;
                delay = diagonalDistance * delayPerDiagonal;
                item.ShowAnimation(delay);
            }

            await UniTask.WaitForSeconds(delay);
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
            float z = (gridCenter.y - coordinates.y) * _sizeOfTile; // Y koordinatı ters çevrildi
            return _centerPoint.position + new Vector3(x, 0, z);
        }

        private void SpawnTile(LevelTileModel tileModel, Vector3 position)
        {
            switch (tileModel.Type)
            {
                case TileType.Floor:
                case TileType.Ice:
                    SpawnFloorTile(tileModel, position);
                    break;
                case TileType.Wall:
                    SpawnWallTile(tileModel, position);
                    break;
                case TileType.Minion:
                    SpawnMinion(tileModel, position);
                    break;
                case TileType.Hole:
                    SpawnHoleItem(tileModel, position);
                    break;
                case TileType.Spike:
                    SpawnSpikeTile(tileModel, position);
                    break;
                case TileType.Collectable:
                    // TODO: Collectable prefab eklendiğinde spawn edilecek
                    break;
            }
        }

        private void SpawnFloorTile(LevelTileModel tileModel, Vector3 position)
        {
            var floorTile = Instantiate(_floorTilePrefab, position, Quaternion.identity, _tilesContainer);
            floorTile.SetTileModel(tileModel);
            _floorTiles[tileModel.Id] = floorTile;
            _allLocationItems[tileModel.Id] = floorTile;
        }

        private void SpawnWallTile(LevelTileModel tileModel, Vector3 position)
        {
            var wallTile = Instantiate(_wallTileView, position, Quaternion.identity, _tilesContainer);
            wallTile.SetTileModel(tileModel);
            _wallTiles[tileModel.Id] = wallTile;
            _allLocationItems[tileModel.Id] = wallTile;
        }

        private void SpawnSpikeTile(LevelTileModel tileModel, Vector3 position)
        {
            var spikeTile = Instantiate(_spikeTileView, position, Quaternion.identity, _tilesContainer);
            spikeTile.SetTileModel(tileModel);
            _spikeTiles[tileModel.Id] = spikeTile;
            _allLocationItems[tileModel.Id] = spikeTile;
        }

        private void SpawnHoleItem(LevelTileModel tileModel, Vector3 position)
        {
            var holeItem = Instantiate(_holeItemView, position, Quaternion.identity, _itemsContainer);
            holeItem.SetTileModel(tileModel);
            _holeItems[tileModel.Id] = holeItem;
            _allLocationItems[tileModel.Id] = holeItem;
        }

        private void SpawnMinion(LevelTileModel tileModel, Vector3 position)
        {
            var minion = Instantiate(_minionView, position, Quaternion.identity, _itemsContainer);
            minion.SetTileModel(tileModel);
            _minions[tileModel.Id] = minion;
            _allLocationItems[tileModel.Id] = minion;
        }

        public ITileItem GetLocationById(int id)
        {
            return _allLocationItems.TryGetValue(id, out var location) ? location : null;
        }

        public void ClearLevel()
        {
            ClearDictionary(_floorTiles);
            ClearDictionary(_wallTiles);
            ClearDictionary(_spikeTiles);
            ClearDictionary(_holeItems);
            ClearDictionary(_minions);
            _allLocationItems.Clear();
        }

        private void ClearDictionary<T>(Dictionary<int, T> dictionary) where T : MonoBehaviour
        {
            foreach (var kvp in dictionary)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            dictionary.Clear();
        }

        public void SetItemsVisible(bool isEnabled)
        {
            foreach (var item in _allLocationItems.Values)
            {
                item.SetEnabled(isEnabled);
            }
        }

        public void DestroyMinion(int minionId)
        {
            if (_minions.Remove(minionId, out var minion))
            {
                _allLocationItems.Remove(minionId);
                
                if (minion != null)
                {
                    minion.Kill();
                }
            }
        }
    }
}