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

        public void LoadLevel(int levelIndex)
        {
            LevelDataSo levelData = _levelProvider.GetLevelData(levelIndex);
            if (levelData is null)
            {
                LogHelper.LogError($"Level {levelIndex} not found");
                return;
            }
            LevelModel.InitializeLevel(levelData);
        }

        public void Initialize(ILevelProvider levelProvider)
        {
            _levelProvider = levelProvider;
            LevelModel = new LevelModel();
        }
    }
}