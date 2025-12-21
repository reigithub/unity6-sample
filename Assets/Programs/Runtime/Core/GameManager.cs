using System;
using System.Threading.Tasks;
using Game.Core.Extensions;
using UnityEngine;
using Game.Core.Services;

namespace Game.Core
{
    public partial class GameManager
    {
        public static readonly GameManager Instance = new();

        private GameConfig _gameConfig;

        public Task LoadCommonObjectsTask { get; private set; }

        private GameManager()
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EntryPoint()
        {
            Instance.GameStart();
        }

        private void GameStart()
        {
            LoadConfig();
            AppQuitIfJailbreak();
            GameServiceManager.Instance.StartUp();
            GameServiceManager.Instance.AddService<MessageBrokerService>();
            LoadCommonObjectsTask = LoadCommonObjectsAsync();
            LoadCommonObjectsTask.Forget();
        }

        private void LoadConfig()
        {
            _gameConfig ??= GameConfigManager.Load();
            if (_gameConfig is null)
            {
                throw new InvalidOperationException("ゲーム環境設定が読み込まれていない為、スタート処理に失敗しました");
            }
        }

        /// <summary>
        /// アプリ脱獄チェックして、黒なら強制終了
        /// </summary>
        private void AppQuitIfJailbreak()
        {
            if (!Application.genuineCheckAvailable)
                return;

            if (!Application.genuine)
            {
                Application.Quit(-1);
            }
        }

        private async Task LoadCommonObjectsAsync()
        {
            await GameCommonObjects.LoadAssetAsync();
        }

        public void GameReStart()
        {
        }

        public void GameSuspend()
        {
        }

        public void GameResume()
        {
        }

        public void GameExit()
        {
            GameServiceManager.Instance.Shutdown();
        }
    }
}