using System;
using UnityEngine;

namespace Game.Core
{
    public partial class GameManager
    {
        public static readonly GameManager Instance = new();

        private GameEnvConfig _gameEnvConfig;

        private readonly GameServiceReference<SampleGameService> _sampleGameService;
        public SampleGameService SampleGameService => _sampleGameService;

        private GameManager()
        {
        }

        public void Initialize(GameEnvConfig config)
        {
            _gameEnvConfig = config;
        }

        public void GameStart()
        {
            ThrowExceptionIfNotInitialize();
            AppQuitIfJailbreak();
            GameServiceManager.Instance.StartUp();
        }

        private void ThrowExceptionIfNotInitialize()
        {
            if (_gameEnvConfig is null)
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