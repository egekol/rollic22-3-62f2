using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Scripts.Gameplay.Items;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private CollectableItemView _collectableItemView;

        [Header("Containers")]
        [SerializeField] private Transform _tilesContainer;
        [SerializeField] private Transform _itemsContainer;

        [Header("Hole Animation Settings")]
        [SerializeField] private float _holeAnimShakeDuration = 0.3f;
        [SerializeField] private float _holeAnimMoveDuration = 0.5f;
        [SerializeField] private float _holeAnimJumpHeight = 0.5f;
        [SerializeField] private float _holeAnimDelayPerItem = 0.04f;
        [SerializeField] private float _holeAnimScaleDownTime = 0.2f;

        // Tile Dictionaries (key: LevelTileModel.Id)
        private Dictionary<int, FloorTileView> _floorTiles = new();
        private Dictionary<int, WallTileView> _wallTiles = new();
        private Dictionary<int, SpikeTileView> _spikeTiles = new();
        
        // Item Dictionaries (key: LevelTileModel.Id)
        private Dictionary<int, HoleItemView> _holeItems = new();
        private Dictionary<int, MinionView> _minions = new();
        private Dictionary<int, CollectableItemView> _collectableItems = new();

        // All location items by ID
        private Dictionary<int, ITileItem> _allLocationItems = new();

        public IReadOnlyDictionary<int, FloorTileView> FloorTiles => _floorTiles;
        public IReadOnlyDictionary<int, WallTileView> WallTiles => _wallTiles;
        public IReadOnlyDictionary<int, SpikeTileView> SpikeTiles => _spikeTiles;
        public IReadOnlyDictionary<int, HoleItemView> HoleItems => _holeItems;
        public IReadOnlyDictionary<int, MinionView> Minions => _minions;
        public IReadOnlyDictionary<int, ITileItem> AllLocationItems => _allLocationItems;
        public IReadOnlyDictionary<int, CollectableItemView> CollectableItems => _collectableItems;

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
        public async UniTask PlayHideAnimations()
        {
            float delayPerDiagonal = 0.03f;

            float delay = 0f;
            foreach (var item in _allLocationItems.Values)
            {
                var coords = item.LevelTileModel.Coordinates;
                // Diagonal distance from top-left corner (0,0)
                int diagonalDistance = coords.x + coords.y;
                delay = diagonalDistance * delayPerDiagonal;
                item.HideAnimation(delay);
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
                    SpawnCollectable(tileModel, position);
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

        private void SpawnCollectable(LevelTileModel tileModel, Vector3 position)
        {
            var collectable = Instantiate(_collectableItemView, position, Quaternion.identity, _itemsContainer);
            collectable.SetTileModel(tileModel);
            _collectableItems[tileModel.Id] = collectable;
            _allLocationItems[tileModel.Id] = collectable;
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
            ClearDictionary(_collectableItems);
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

        public void DestroyCollectable(int collectableId)
        {
            if (_collectableItems.Remove(collectableId, out var collectable))
            {
                _allLocationItems.Remove(collectableId);
                
                if (collectable != null)
                {
                    collectable.Collect();
                }
            }
        }

        public async UniTask MoveBlocksToHoleAnimation()
        {
            // Hole bulunamazsa çık
            if (_holeItems.Count == 0) return;
            
            // İlk hole'un pozisyonunu al
            HoleItemView hole = null;
            Vector3 holePosition = Vector3.zero;
            foreach (var holeItem in _holeItems.Values)
            {
                hole = holeItem;
                holePosition = holeItem.transform.position;
                break;
            }
            
            if (hole == null) return;
            
            // Hole hariç tüm item'ları topla ve hole'a uzaklığa göre sırala
            var itemsToAnimate = new List<(ITileItem item, float distance, Transform transform)>();
            foreach (var item in _allLocationItems.Values)
            {
                if (item is HoleItemView) continue;
                
                var itemTransform = (item as MonoBehaviour)?.transform;
                if (itemTransform == null) continue;
                
                float distance = Vector3.Distance(itemTransform.position, holePosition);
                itemsToAnimate.Add((item, distance, itemTransform));
            }
            
            // Uzaklığa göre sırala (en yakın en önce)
            itemsToAnimate.Sort((a, b) => a.distance.CompareTo(b.distance));
            
            // Tüm item'ları shake et
            foreach (var (item, distance, itemTransform) in itemsToAnimate)
            {
                itemTransform.DOShakePosition(_holeAnimShakeDuration, 0.1f, 20, 90, false, true)
                    .SetLink(itemTransform.gameObject);
            }
            
            await UniTask.WaitForSeconds(_holeAnimShakeDuration);
            
            // Uzaklık sırasına göre hole'a doğru hareket ettir
            float maxDelay = 0f;
            for (int i = 0; i < itemsToAnimate.Count; i++)
            {
                var (item, distance, itemTransform) = itemsToAnimate[i];
                float delay = i * _holeAnimDelayPerItem;
                maxDelay = delay;
                
                // Jump ile hole'a git
                var sequence = DOTween.Sequence();
                sequence.SetDelay(delay);
                sequence.Append(itemTransform.DOJump(holePosition, _holeAnimJumpHeight, 1, _holeAnimMoveDuration)
                    .SetEase(Ease.InQuad));
                
                // Son 0.2 saniye kala scale down
                sequence.Insert(delay + _holeAnimMoveDuration - _holeAnimScaleDownTime, 
                    itemTransform.DOScale(Vector3.zero, _holeAnimScaleDownTime).SetEase(Ease.InBack));
                
                // Animasyon bitince yok et
                sequence.OnComplete(() =>
                {
                    var go = itemTransform?.gameObject;
                    if (go != null)
                    {
                        Destroy(go);
                    }
                });
                
                sequence.SetLink(itemTransform.gameObject);
            }
            
            // Tüm animasyonların bitmesini bekle
            await UniTask.WaitForSeconds(maxDelay + _holeAnimMoveDuration + 0.1f);
            
            // Hole'u yok et
            if (hole != null)
            {
                hole.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        if (hole != null)
                        {
                            Destroy(hole.gameObject);
                        }
                    });
            }
            
            await UniTask.WaitForSeconds(0.4f);
        }
    }
}