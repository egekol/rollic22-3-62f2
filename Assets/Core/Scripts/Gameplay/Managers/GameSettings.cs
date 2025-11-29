using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Lib.Utility;

namespace Core.Scripts.Gameplay.Managers
{
    public class GameSettings : Singleton<GameSettings>
    {
        private LevelManager _levelManager;
        private LevelExporter _levelProvider;
        
        private void Awake()
        {
            SetInstances();
            InitializeBindings();
        }

        private void SetInstances()
        {
            _levelManager = LevelManager.Instance;
            _levelProvider = LevelExporter.Instance;
        }

        public void InitializeBindings()
        {
            _levelManager.Initialize(_levelProvider);
        }

        public void InitializeGameBindings()
        {
            MovementManager.Instance.Initialize(LevelManager.Instance.LevelModel, LevelGenerator.Instance);

            OnGameSceneInitialized();
        }

        private void OnGameSceneInitialized()
        {
            
        }
    }
}