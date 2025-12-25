using System.Threading.Tasks;
using Game.Core.Extensions;
using Game.Core.Scenes;
using Game.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class GameTitleSceneComponent : GameSceneComponent
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private GameServiceReference<AddressableAssetService> _assetService;
        private GameServiceReference<GameSceneService> _sceneService;

        public override Task Initialize()
        {
            if (_startButton)
            {
                _startButton.onClick.AddListener(() =>
                {
                    _sceneService.Reference.TransitionAsync<GameStageScene, string>("Assets/Scenes/UnityScenes/Stage00/Stage00.unity").Forget();
                });
            }

            if (_quitButton)
            {
                _quitButton.onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.ExitPlaymode();
#else
                    Application.Quit();
#endif
                });
            }

            return Task.CompletedTask;
        }
    }
}