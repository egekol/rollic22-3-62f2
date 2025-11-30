using Core.Scripts.Gameplay.Events;
using Core.Scripts.Gameplay.Items;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;
using Lib.Debugger;

namespace Core.Scripts.Gameplay.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        private ILevelProvider _levelProvider;

        public LevelModel LevelModel { get; private set; }
        public int CurrentLevel { get; set; }
        public bool IsLevelPlaying { get; set; }
        public int CollectedCollectableCount { get; private set; }

        public void LoadLevel(int levelIndex)
        {
            LevelDataSo levelData = _levelProvider.GetLevelData(levelIndex);
            if (levelData is null)
            {
                LogHelper.LogError($"Level {levelIndex} not found");
                return;
            }
            LevelModel.InitializeLevel(levelData);
            ResetCollectableCount();
        }

        public void Initialize(ILevelProvider levelProvider)
        {
            _levelProvider = levelProvider;
            LevelModel = new LevelModel();
        }

        public void LoadCurrentLevel()
        {
            LoadLevel(CurrentLevel);
        }
        
        

        public void RemoveMinion(MinionView minion)
        {
            if (minion == null || minion.LevelTileModel == null)
                return;

            int minionId = minion.LevelTileModel.Id;
            
            LevelModel.RemoveTile(minionId);
            
            LevelGenerator.Instance.DestroyMinion(minionId);
            // Not: CollectMinionCount artık PreCollectMinions ile hareket hesaplaması sırasında çağrılıyor
            // Bu sayede level complete kontrolü animasyon bitmeden önce doğru çalışıyor
            
            LogHelper.Log($"Minion removed: {minionId}");
        }

        private void ResetCollectableCount()
        {
            CollectedCollectableCount = 0;
            GameEvents.InvokeCollectableCountChanged(CollectedCollectableCount);
        }

        public void CollectCollectable(CollectableItemView collectable)
        {
            if (collectable == null || collectable.LevelTileModel == null)
                return;

            int collectableId = collectable.LevelTileModel.Id;

            LevelModel.RemoveTile(collectableId);
            LevelGenerator.Instance.DestroyCollectable(collectableId);

            CollectedCollectableCount++;
            GameEvents.InvokeCollectableCountChanged(CollectedCollectableCount);
            LogHelper.Log($"Collectable collected: {collectableId} (Total: {CollectedCollectableCount})");
        }

        private void CollectMinionCount()
        {
            LevelModel.CollectMinion();
        }

        /// <summary>
        /// Animasyon başlamadan önce hole'a düşecek minion'ları önceden sayar.
        /// Bu, level complete kontrolünün doğru çalışması için gereklidir.
        /// </summary>
        public void PreCollectMinions(int count)
        {
            for (int i = 0; i < count; i++)
            {
                LevelModel.CollectMinion();
            }
            LogHelper.Log($"Pre-collected {count} minions");
        }

        public void DecreaseMoveCount()
        {
            LevelModel.DecreaseMoveCount();
        }

        public void IncreaseLevel()
        {
            CurrentLevel++;
        }
    }
}