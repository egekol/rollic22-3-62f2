using System.Threading.Tasks;
using Core.Scripts.Gameplay.Inputs;
using Core.Scripts.Gameplay.Levels;
using Core.Scripts.Gameplay.Panels;
using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using Lib.Debugger;

namespace Core.Scripts.Gameplay.Managers
{
    public class GameSettings : Singleton<GameSettings>
    {
        private LevelManager _levelManager;
        private LevelExporter _levelProvider;
        private MovementManager _movementManager;
        private BackgroundUI _backgroundUI;
        private InGameUI _inGameUI;
        private LevelGenerator _levelGenerator;
        private InputManager _inputManager;

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
            _inputManager = InputManager.Instance;
            _inGameUI = InGameUI.Instance;
            OnGameSceneInitialized();
        }

        private void OnGameSceneInitialized()
        {
            PlayNextLevel().Forget();
        }

        private async UniTask PlayNextLevel()
        {
            _inGameUI.SetEnabled(false);
            _backgroundUI.BlockViewWithCanvas();
            _backgroundUI.PlayParticleFaster();
            await UniTask.WaitForSeconds(1.4f);
            _backgroundUI.PlayParticleSlower();
            await UniTask.WaitForSeconds(.9f);

            _levelManager.LoadCurrentLevel();
            _inGameUI.InitializeHeader(_levelManager.LevelModel);
            _inputManager.SetInputState(InputState.Disabled);
            _levelGenerator.GenerateLevel(LevelManager.Instance.LevelModel);
            _levelGenerator.SetItemsVisible(true);
            await UniTask.WaitForSeconds(.1f);
            _levelGenerator.SetItemsVisible(false);

            _backgroundUI.UnblockViewWithCanvas();
            _inGameUI.ShowAnimation(); 
            await _levelGenerator.PlayShowAnimations();
            OnLevelReadyToPlay();
        }

        private void OnLevelReadyToPlay()
        {
            _inputManager.SetInputState(InputState.Enabled);
        }

        public void OnInputUpdated(InputState inputState, InputDirection inputDirection)
        {
            LogHelper.Log($"Input State: {inputState} - Direction: {inputDirection}");
            if (inputState != InputState.Scrolling)
            {
                return;
            }

            if (inputDirection == InputDirection.None)
            {
                return;
            }
            
            ProcessInputMovement(inputDirection).Forget();
            
        }

        private async UniTask ProcessInputMovement(InputDirection inputDirection)
        {
            
            if (_movementManager.MovementState == MovementState.Swiping)
            {
                return;
            }
            LogHelper.Log($"Input Direction: {inputDirection} - Move");
            await _movementManager.Move(inputDirection);

            if (!CheckIfAnyConditionMet())
            {
                // _inputManager.SetInputState(InputState.Enabled);
            }
        }

        private bool CheckIfAnyConditionMet()
        {
            return false;
        }
    }
}