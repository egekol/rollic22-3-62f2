using Core.Scripts.Lib.Utility;
using Lib.Debugger;
using UnityEngine;

namespace Core.Scripts.Gameplay.Managers
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _cameraPivotPoint;
        [SerializeField] private float _ratioValue;

        private const float BaseGridWidth = 8;
        private Vector3 _initialCameraPosition;
        private bool _isInitialized;
        
        private void Awake()
        {
            _initialCameraPosition = _camera.transform.position;
            _isInitialized = true;
        }

        public void AdjustCameraForGridWidth(int gridWidth)
        {
            if (!_isInitialized)
            {
                _initialCameraPosition = _camera.transform.position;
                _isInitialized = true;
            }
            
            LogHelper.Log($"level : {LevelManager.Instance.CurrentLevel}");
            if (LevelManager.Instance.CurrentLevel== 1)
            { 
                _camera.transform.localPosition = new Vector3(0, 47.7400017f, -7.13000011f);
                return;
            }
            else
            {
                _camera.transform.position = _cameraPivotPoint.transform.position;
            }
            
            // var ratio =  Mathf.Max(gridWidth - BaseGridWidth,0);
            
            
            // Grid width 8 ise değişiklik yapma
            if (gridWidth == BaseGridWidth)
            {
                LogHelper.Log($"CameraManager: GridWidth={gridWidth}, BaseWidth={BaseGridWidth}, No adjustment needed");
                return;
            }
            
            float rawRatio = (float)gridWidth / BaseGridWidth;
            // Ratio değişimini %50 azalt
            float ratio = 1f + (rawRatio - 1f) * 0.8f;
            // Minimum 0.9 ratio (çok yaklaşmasın)
            ratio = Mathf.Max(ratio, 0.9f);
            
            LogHelper.Log($"CameraManager: GridWidth={gridWidth}, RawRatio={rawRatio}, FinalRatio={ratio}, InitialPos={_initialCameraPosition}");

            _camera.transform.position = _initialCameraPosition * ratio;
        }
    }
}