using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Panels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;

namespace Core.Scripts.Gameplay.Managers
{
    public class GameSettings : Singleton<GameSettings>
    {
        private LevelManager _levelManager;
        private LevelExporter _levelProvider;
        private MovementManager _movementManager;
        private BackgroundUI _backgroundUI;
        private LevelGenerator _levelGenerator;

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
            _movementManager = MovementManager.Instance;
            _levelGenerator = LevelGenerator.Instance;
            _movementManager.Initialize(LevelManager.Instance.LevelModel, _levelGenerator);
            _backgroundUI = BackgroundUI.Instance;
            OnGameSceneInitialized();
        }

        private void OnGameSceneInitialized()
        {
            StartNextLevel().Forget();
        }

        private async UniTask StartNextLevel()
        {
            _backgroundUI.BlockViewWithCanvas();
            _backgroundUI.PlayParticleFaster();
            await UniTask.WaitForSeconds(1.8f);
            _backgroundUI.PlayParticleSlower();
            await UniTask.WaitForSeconds(.5f);
            _levelManager.LoadCurrentLevel();
            _levelGenerator.GenerateLevel(LevelManager.Instance.LevelModel);
            _levelGenerator.SetItemsVisible(true);
            await UniTask.WaitForSeconds(.1f);
            _levelGenerator.SetItemsVisible(false);
            _backgroundUI.UnblockViewWithCanvas();
            _levelGenerator.PlayShowAnimations();
        }
    }
}