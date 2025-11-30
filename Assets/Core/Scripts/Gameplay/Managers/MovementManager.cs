using System.Collections.Generic;
using Core.Scripts.Gameplay.Inputs;
using Core.Scripts.Gameplay.Items;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lib.CustomAttributes.Scripts;
using Lib.Debugger;
using UnityEngine;

namespace Core.Scripts.Gameplay.Managers
{
    public enum MovementState
    {
        None,
        Swiping,
        Idle,
    }

    public class MovementManager : Singleton<MovementManager>
    {
        [SerializeField] private Transform _centerPoint;
        [SerializeField] private float _sizeOfTile = 1f;
        [SerializeField] private float _movementDuration = 0.05f;
        [SerializeField] private Ease _ease = Ease.InSine;

        private LevelModel _levelModel;
        private ILevelGenerator _levelGenerator;

        public MovementState MovementState { get; set; } = MovementState.Idle;
        
        public Ease Ease => _ease;

        public float MovementDuration => _movementDuration;

        public void Initialize(LevelModel levelModel, ILevelGenerator levelGenerator)
        {
            _levelModel = levelModel;
            _levelGenerator = levelGenerator;
        }

        [ProButton]
        public void MoveTest(InputDirection direction)
        {
            Move(direction).Forget();
        }
        
        public async UniTask Move(InputDirection direction)
        {
            if (MovementState == MovementState.Swiping)
                return;

            MovementState = MovementState.Swiping;

            var moveTasks = new List<UniTask>();
            var directionVector = GetDirectionVector(direction);

            // Minion'ların hedef pozisyonlarını hesapla
            var minionTargets = CalculateMinionTargets(directionVector);
            
            // Hole'ların hedef pozisyonlarını hesapla
            var holeTargets = CalculateHoleTargets(directionVector);

            // Minion hareketlerini başlat
            foreach (var kvp in minionTargets)
            {
                var minionId = kvp.Key;
                var (targetCoord, distance) = kvp.Value;
                
                if (distance > 0 && _levelGenerator.Minions.TryGetValue(minionId, out var minion))
                {
                    var targetWorldPos = CalculateWorldPosition(targetCoord);
                    minion.LevelTileModel.Coordinates = targetCoord;
                    var completionSource = minion.Move(targetWorldPos, distance, direction);
                    moveTasks.Add(completionSource.Task);
                }
            }

            // Hole hareketlerini başlat
            foreach (var kvp in holeTargets)
            {
                var holeId = kvp.Key;
                var (targetCoord, distance) = kvp.Value;
                
                if (distance > 0 && _levelGenerator.HoleItems.TryGetValue(holeId, out var hole))
                {
                    var targetWorldPos = CalculateWorldPosition(targetCoord);
                    hole.LevelTileModel.Coordinates = targetCoord;
                    var completionSource = hole.Move(targetWorldPos, distance);
                    moveTasks.Add(completionSource.Task);
                }
            }

            // Tüm animasyonların bitmesini bekle
            await UniTask.WhenAll(moveTasks);
            LogHelper.Log($"Movement Complete: {moveTasks.Count}");
            MovementState = MovementState.Idle;
        }

        private Vector2Int GetDirectionVector(InputDirection direction)
        {
            return direction switch
            {
                InputDirection.Up => new Vector2Int(0, -1),    // Grid'de yukarı = Y azalır
                InputDirection.Down => new Vector2Int(0, 1),  // Grid'de aşağı = Y artar
                InputDirection.Left => new Vector2Int(-1, 0),
                InputDirection.Right => new Vector2Int(1, 0),
                _ => Vector2Int.zero
            };
        }

        private Dictionary<int, (Vector2Int targetCoord, int distance)> CalculateMinionTargets(Vector2Int directionVector)
        {
            var targets = new Dictionary<int, (Vector2Int, int)>();
            var minions = _levelGenerator.Minions;
            
            // Hareket yönüne göre minion'ları sırala (önce en uzaktakiler hareket etsin)
            var sortedMinions = new List<(int id, MinionView minion)>();
            foreach (var kvp in minions)
            {
                sortedMinions.Add((kvp.Key, kvp.Value));
            }
            
            // Hareket yönüne göre sıralama (arkadakiler önce hesaplanır)
            sortedMinions.Sort((a, b) =>
            {
                var coordA = a.minion.LevelTileModel.Coordinates;
                var coordB = b.minion.LevelTileModel.Coordinates;
                
                if (directionVector.x != 0)
                    return directionVector.x > 0 ? coordB.x.CompareTo(coordA.x) : coordA.x.CompareTo(coordB.x);
                if (directionVector.y != 0)
                    return directionVector.y > 0 ? coordB.y.CompareTo(coordA.y) : coordA.y.CompareTo(coordB.y);
                return 0;
            });

            // Her minion için hedef pozisyon hesapla
            var occupiedByMinions = new HashSet<Vector2Int>();
            foreach (var (id, minion) in sortedMinions)
            {
                var currentCoord = minion.LevelTileModel.Coordinates;
                var targetCoord = currentCoord;
                var distance = 0;

                // Adım adım ilerle
                while (true)
                {
                    var nextCoord = targetCoord + directionVector;
                    
                    // Grid sınırları kontrolü
                    if (!IsWithinGrid(nextCoord))
                        break;

                    var tileAtNext = _levelModel.TilesGrid[nextCoord.x, nextCoord.y];
                    
                    // Duvar veya Ice kontrolü
                    // TODO: Ice'a çarpıldığında Ice yok edilecek
                    if (tileAtNext != null && (tileAtNext.Type == TileType.Wall || tileAtNext.Type == TileType.Ice))
                        break;

                    // Diğer minion kontrolü (minion'lar birbirinin içinden geçemez)
                    if (occupiedByMinions.Contains(nextCoord))
                        break;

                    // Spike ve Collectable içinden geçebilir
                    targetCoord = nextCoord;
                    distance++;
                }

                targets[id] = (targetCoord, distance);
                occupiedByMinions.Add(targetCoord);
            }

            return targets;
        }

        private Dictionary<int, (Vector2Int targetCoord, int distance)> CalculateHoleTargets(Vector2Int directionVector)
        {
            var targets = new Dictionary<int, (Vector2Int, int)>();
            var holes = _levelGenerator.HoleItems;

            foreach (var kvp in holes)
            {
                var holeId = kvp.Key;
                var hole = kvp.Value;
                var currentCoord = hole.LevelTileModel.Coordinates;
                var targetCoord = currentCoord;
                var distance = 0;

                // Adım adım ilerle
                while (true)
                {
                    var nextCoord = targetCoord + directionVector;
                    
                    // Grid sınırları kontrolü
                    if (!IsWithinGrid(nextCoord))
                        break;

                    var tileAtNext = _levelModel.TilesGrid[nextCoord.x, nextCoord.y];
                    
                    // Duvar veya Ice kontrolü
                    // TODO: Ice'a çarpıldığında Ice yok edilecek
                    if (tileAtNext != null && (tileAtNext.Type == TileType.Wall || tileAtNext.Type == TileType.Ice))
                        break;

                    // Hole, minion'ların içinden geçebilir (aynı koordinatta durabilirler)
                    // Spike ve Collectable içinden de geçebilir
                    targetCoord = nextCoord;
                    distance++;
                }

                targets[holeId] = (targetCoord, distance);
            }

            return targets;
        }

        private bool IsWithinGrid(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < _levelModel.GridSize.x &&
                   coord.y >= 0 && coord.y < _levelModel.GridSize.y;
        }

        private Vector3 CalculateWorldPosition(Vector2Int coordinates)
        {
            var gridCenter = new Vector2(
                (_levelModel.GridSize.x - 1) * 0.5f,
                (_levelModel.GridSize.y - 1) * 0.5f
            );
            
            float x = (coordinates.x - gridCenter.x) * _sizeOfTile;
            float z = (gridCenter.y - coordinates.y) * _sizeOfTile;
            return _centerPoint.position + new Vector3(x, 0, z);
        }

    }
}