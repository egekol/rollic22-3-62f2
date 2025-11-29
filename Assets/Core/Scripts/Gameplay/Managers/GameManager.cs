using Core.Scripts.Lib.Utility;
using Cysharp.Threading.Tasks;
using Lib.SceneLoad;
using UnityEngine;

namespace Core.Scripts.Gameplay.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private SceneSettingsSo _sceneSettings;
        private SceneController _sceneController;


        
        private void Awake()
        {
            _sceneController = new SceneController();
            _sceneController.Init(_sceneSettings);
        }

        private void Start()
        {
            LoadScenes().Forget();
        }

        private async UniTask LoadScenes()
        { 
            await _sceneController.LoadSceneAsync(GameplaySceneName);
            await _sceneController.LoadSceneAsync(MainSceneName);
        }

        private const string GameplaySceneName = "GameScene";
        private const string MainSceneName = "MainScene";
    }
}