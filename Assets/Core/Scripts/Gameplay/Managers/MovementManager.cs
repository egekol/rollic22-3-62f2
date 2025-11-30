using System.Collections.Generic;
using Core.Scripts.Gameplay.Events;
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
        Stopped
    }

    public class MovementManager : Singleton<MovementManager>
    {
        [SerializeField] private Transform _centerPoint;
        [SerializeField] private float _sizeOfTile = 1f;
        [SerializeField] private float _movementDuration = 0.05f;
        [SerializeField] private Ease _ease = Ease.InSine;

        private LevelModel _levelModel;
        private ILevelGenerator _levelGenerator;

        public MovementState MovementState { get; private set; } = MovementState.Idle;
        
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

            SetMovementState(MovementState.Swiping);

            var moveTasks = new List<UniTask>();
            var directionVector = GetDirectionVector(direction);

            // Hole'ların hedef pozisyonlarını hesapla
            var holeTargets = CalculateHoleTargets(directionVector);
            
            // Hole'ların final pozisyonlarını set olarak tut
            var finalHolePositions = new HashSet<Vector2Int>();
            foreach (var kvp in holeTargets)
            {
                finalHolePositions.Add(kvp.Value.targetCoord);
            }
            
            // Hole'ların geçtiği TÜM yolları hesapla (başlangıçtan hedefe kadar)
            var holePathPositions = new HashSet<Vector2Int>();
            foreach (var kvp in holeTargets)
            {
                var holeId = kvp.Key;
                var (targetCoord, distance) = kvp.Value;
                
                if (_levelGenerator.HoleItems.TryGetValue(holeId, out var hole))
                {
                    var startPos = hole.LevelTileModel.Coordinates;
                    var currentPos = startPos;
                    
                    // Başlangıç pozisyonunu ekle (arkadan gelen minion buraya ulaşabilir)
                    holePathPositions.Add(startPos);
                    
                    // Hole'un geçtiği her pozisyonu ekle
                    for (int i = 0; i < distance; i++)
                    {
                        currentPos += directionVector;
                        holePathPositions.Add(currentPos);
                    }
                    
                    // Final pozisyonu da ekle (distance = 0 durumu için güvenlik)
                    holePathPositions.Add(targetCoord);
                }
            }

            // Minion'ların hedef pozisyonlarını hesapla
            var minionTargets = CalculateMinionTargets(directionVector);
            
            // Hole'a düşecek minion sayısını hesapla ve önceden collect et
            // Minion'un final pozisyonu hole'un YOLUNDAKİ herhangi bir noktada ise, yakalanacak
            int minionsToFallInHole = 0;
            foreach (var kvp in minionTargets)
            {
                var (targetCoord, _) = kvp.Value;
                if (holePathPositions.Contains(targetCoord))
                {
                    minionsToFallInHole++;
                }
            }
            
            if (minionsToFallInHole > 0)
            {
                LevelManager.Instance.PreCollectMinions(minionsToFallInHole);
            }

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
                    var completionSource = hole.Move(targetWorldPos, distance, direction);
                    moveTasks.Add(completionSource.Task);
                }
            }

            if (moveTasks.Count>0)
            {
                LevelManager.Instance.DecreaseMoveCount();
                GameEvents.InvokeMoveCountChanged(LevelManager.Instance.LevelModel.RemainingMoveCount);
            }

            // Tüm animasyonların bitmesini bekle
            await UniTask.WhenAll(moveTasks);
            LogHelper.Log($"Movement Complete - Tasks Count: {moveTasks.Count}");
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
            
            // Önce hole'ların hareket sonrası pozisyonlarını hesapla
            var holeTargets = CalculateHoleTargets(directionVector);
            var finalHolePositions = new HashSet<Vector2Int>();

            // Hole'ların geçtiği TÜM yolları hesapla (başlangıçtan hedefe kadar)
            var holePathPositions = new HashSet<Vector2Int>();
            
            // Hole'ların orijinal pozisyonlarını da tut (final pozisyon -> orijinal pozisyon)
            var holeOriginalPositions = new Dictionary<Vector2Int, Vector2Int>();
            foreach (var kvp in holeTargets)
            {
                var holeId = kvp.Key;
                var finalPos = kvp.Value.targetCoord;
                finalHolePositions.Add(finalPos);
                
                // Orijinal pozisyonu ve path'ini bul
                if (_levelGenerator.HoleItems.TryGetValue(holeId, out var hole))
                {
                    var startPos = hole.LevelTileModel.Coordinates;
                    holeOriginalPositions[finalPos] = startPos;

                    // Hole'un path'ini ekle (başlangıçtan hedefe kadar tüm koordinatlar)
                    var currentPos = startPos;
                    holePathPositions.Add(startPos);

                    var distance = kvp.Value.distance;
                    for (int i = 0; i < distance; i++)
                    {
                        currentPos += directionVector;
                        holePathPositions.Add(currentPos);
                    }

                    // Güvenlik için final pozisyonu da ekle
                    holePathPositions.Add(finalPos);
                }
            }
            
            // Hareket yönüne göre minion'ları sırala (önce en uzaktakiler hareket etsin)
            var sortedMinions = new List<(int id, MinionView minion)>();
            foreach (var kvp in minions)
            {
                sortedMinions.Add((kvp.Key, kvp.Value));
            }

            // Hareket yönüne göre sıralama
            // Not: Projeksiyon (dot product) kullanarak, hareket yönünde ÖNDE olan minion'lar önce hesaplanır.
            sortedMinions.Sort((a, b) =>
            {
                var coordA = a.minion.LevelTileModel.Coordinates;
                var coordB = b.minion.LevelTileModel.Coordinates;

                int projA = coordA.x * directionVector.x + coordA.y * directionVector.y;
                int projB = coordB.x * directionVector.x + coordB.y * directionVector.y;

                // projB > projA ise b, a'dan daha öndedir → önce b gelmeli
                return projB.CompareTo(projA);
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

                    // Diğer minion kontrolü
                    if (occupiedByMinions.Contains(nextCoord))
                    {
                        // Hole önümüzde ise (hareket yönünde) stack'e izin ver
                        // Hole arkadan geliyorsa stack yapma
                        bool canPassThrough = false;
                        if (finalHolePositions.Contains(nextCoord) && 
                            holeOriginalPositions.TryGetValue(nextCoord, out var holeOriginalPos))
                        {
                            canPassThrough = IsPositionInFront(currentCoord, holeOriginalPos, directionVector);
                        }
                        
                        if (!canPassThrough)
                            break;
                    }

                    // Spike ve Collectable içinden geçebilir
                    targetCoord = nextCoord;
                    distance++;
                }

                targets[id] = (targetCoord, distance);
                
                // Hole pozisyonunda stacking kontrolü:
                // Sadece hole önümüzdeyse (orijinal pozisyon hareket yönünde) stack'e izin ver
                bool isOnHolePath = holePathPositions.Contains(targetCoord);
                bool shouldAllowStacking = false;
                if (finalHolePositions.Contains(targetCoord) && 
                    holeOriginalPositions.TryGetValue(targetCoord, out var originalPos))
                {
                    shouldAllowStacking = IsPositionInFront(currentCoord, originalPos, directionVector);
                }
                
                // Hole path'indeki ve hareket etmeyen (distance == 0) minion'lar
                // arkadan gelenleri bloklamasın; diğerleri normal şekilde bloklasın.
                if (!shouldAllowStacking && !(isOnHolePath && distance == 0))
                {
                    occupiedByMinions.Add(targetCoord);
                }
            }

            return targets;
        }
        
        /// <summary>
        /// Verilen pozisyonun, referans noktasına göre hareket yönünde (önde) olup olmadığını kontrol eder.
        /// </summary>
        private bool IsPositionInFront(Vector2Int referencePos, Vector2Int checkPos, Vector2Int direction)
        {
            if (direction.x != 0)
            {
                return direction.x > 0 ? checkPos.x > referencePos.x : checkPos.x < referencePos.x;
            }
            if (direction.y != 0)
            {
                return direction.y > 0 ? checkPos.y > referencePos.y : checkPos.y < referencePos.y;
            }
            return false;
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

        public void SetMovementState(MovementState state)
        {
            MovementState = state;
            LogHelper.Log($"Movement State set to: {state}");
        }
    }
}