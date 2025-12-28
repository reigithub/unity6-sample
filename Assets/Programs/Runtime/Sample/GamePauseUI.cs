using System.Threading.Tasks;
using Game.Core;
using Game.Core.Scenes;
using Game.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class GamePauseUIDialog : GameDialogScene<GamePauseUIDialog, GamePauseUI, bool>
    {
        protected override string AssetPathOrAddress => "Assets/Prefabs/UI/GamePauseUI.prefab";

        public static Task<bool> RunAsync()
        {
            var sceneService = GameServiceManager.Instance.GetService<GameSceneService>();
            return sceneService.TransitionDialogAsync<GamePauseUIDialog, GamePauseUI, bool>(initializer: (dialog, component) =>
            {
                component.Initialize(dialog);
                return Task.CompletedTask;
            });
        }

        public override Task Startup()
        {
            Time.timeScale = 0f;
            return base.Startup();
        }

        public override Task Terminate()
        {
            Time.timeScale = 1f;
            return base.Terminate();
        }
    }

    public class GamePauseUI : GameSceneComponent
    {
        [SerializeField]
        private Button _resumeButton;

        [SerializeField]
        private Button _retryButton;

        [SerializeField]
        private Button _returnButton;

        [SerializeField]
        private Button _quitButton;

        public void Initialize(GamePauseUIDialog dialog)
        {
            _resumeButton.onClick.AddListener(() =>
            {
                GameManager.Instance.GameResume();
                dialog.Terminate();
            });
            _retryButton.onClick.AddListener(() => { });
            _returnButton.onClick.AddListener(() => { });
            _quitButton.onClick.AddListener(() => { GameManager.Instance.GameQuit(); });
        }
    }
}