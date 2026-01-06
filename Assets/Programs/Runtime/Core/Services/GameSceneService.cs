using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Contents.Scenes;
using Game.Core.Constants;
using Game.Core.Enums;
using Game.Core.MessagePipe;
using Game.Core.Scenes;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Game.Core.Services
{
    /// <summary>
    /// GameSceneの遷移挙動を制御するサービス
    /// </summary>
    public partial class GameSceneService : GameService
    {
        private GameServiceReference<AddressableAssetService> _assetService;
        private AddressableAssetService AssetService => _assetService.Reference;

        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        protected internal override bool AllowResidentOnMemory => true;

        // 1.履歴を持って、一つ前のシーンへ戻れるように（→完了）
        // 2.現在シーンをスリープさせて、次のシーンを表示する処理（→完了）
        // 3.ダイアログ用のオーバーレイ表示（→完了）
        // 4.マルチ解像度対応（いずれどこかで…）
        private readonly LinkedList<(Type type, IGameScene gameScene)> _gameScenes = new();

        private const GameSceneOperations DefaultOperations = GameSceneConstants.DefaultOperations;

        public async Task TransitionAsync<TScene>(GameSceneOperations operations = DefaultOperations)
            where TScene : IGameScene, new()
        {
            await OperationAsync(operations);

            var gameScene = new TScene();
            _gameScenes.AddLast((typeof(TScene), gameScene));
            await TransitionCore(gameScene);
        }

        // 引数つきの画面遷移
        public async Task TransitionAsync<TScene, TArg>(TArg arg, GameSceneOperations operations = DefaultOperations)
            where TScene : IGameScene, new()
        {
            await OperationAsync(operations);

            var gameScene = new TScene();
            CreateArgHandler(gameScene, arg);
            _gameScenes.AddLast((typeof(TScene), gameScene));
            await TransitionCore(gameScene);
        }

        // リザルトつきの画面遷移
        public async Task<TResult> TransitionAsync<TScene, TResult>(GameSceneOperations operations = DefaultOperations)
            where TScene : IGameScene, new()
        {
            await OperationAsync(operations);

            var type = typeof(TScene);
            var gameScene = new TScene();
            var tcs = CreateResultTcs<TResult>(gameScene);
            _gameScenes.AddLast((type, gameScene));
            await TransitionCore(gameScene);
            return await ResultCore(type, tcs);
        }

        // 引数とリザルトつきの画面遷移
        public async Task<TResult> TransitionAsync<TScene, TArg, TResult>(TArg arg, GameSceneOperations operations = DefaultOperations)
            where TScene : IGameScene, new()
        {
            await OperationAsync(operations);

            var type = typeof(TScene);
            var gameScene = new TScene();
            // await gameScene.ArgHandle(arg);
            CreateArgHandler(gameScene, arg);
            var tcs = CreateResultTcs<TResult>(gameScene);
            _gameScenes.AddLast((type, gameScene));
            await TransitionCore(gameScene);
            return await ResultCore(type, tcs);
        }

        // 現在のシーンから見て、前のシーンへ戻る
        public async Task TransitionPrevAsync()
        {
            var prevNode = _gameScenes.Last.Previous;
            if (prevNode != null)
            {
                var target = prevNode.Value;
                var state = target.gameScene.State;
                if (state is GameSceneState.Terminate)
                {
                    // 現在のシーンを閉じて履歴を消す
                    await TerminateLastAsync(clearHistory: true);
                    // 履歴から遷移する
                    await TransitionCore(target.gameScene);
                }
                else if (state is GameSceneState.Sleep)
                {
                    // 現在のシーンを閉じて履歴を消す
                    await TerminateLastAsync(clearHistory: true);
                    // スリープ復帰
                    await RestartAsync();
                }
                else if (state is GameSceneState.Processing)
                {
                    await TerminateLastAsync(clearHistory: true);
                }
            }
            else
            {
                // なければブラックアウトッ!しないために、ホームやタイトルへ戻る
                // 今はホームがないのでタイトルです
                await TransitionAsync<GameTitleScene>();
            }
        }

        public async Task<TResult> TransitionDialogAsync<TScene, TComponent, TResult>(Func<TScene, TComponent, Task> initializer = null)
            where TScene : GameDialogScene<TScene, TComponent, TResult>, new()
            where TComponent : GameSceneComponent
        {
            // ダイアログは複数開く事ができる
            // Memo: ダイアログはプロセス中に再度要求されたら閉じる挙動とする(ここは後でダイアログ毎に変えられるようにするかもしれない)
            var type = typeof(TScene);
            if (IsProcessing(type))
            {
                await TerminateAsync(type, clearHistory: true);
                return default;
            }

            // WARN: MonoBehaviourをnewしない方向で実装する必要がある…
            var gameScene = new TScene();
            gameScene.Scene = gameScene; // WARN: コンポーネント側からダイアログ操作などを可能にするために、具象化クラスをベースクラスへ入れる（後で改善する）
            gameScene.StartupHandler = initializer;
            var tcs = CreateResultTcs<TResult>(gameScene);
            _gameScenes.AddLast((type, gameScene));
            await TransitionCore(gameScene, isDialog: true);
            return await ResultCore(type, tcs);
        }

        // 主に遷移前に現在のシーンに対して何かする
        private async Task OperationAsync(GameSceneOperations operations = DefaultOperations)
        {
            // シーン遷移が起こる時はダイアログはすべて閉じる
            await TerminateAllDialogAsync();

            if (operations.HasFlag(GameSceneOperations.CurrentSceneSleep))
            {
                await SleepAsync();
            }
            else if (operations.HasFlag(GameSceneOperations.CurrentSceneRestart))
            {
                await RestartAsync();
            }
            else if (operations.HasFlag(GameSceneOperations.CurrentSceneTerminate))
            {
                bool clearHistory = operations.HasFlag(GameSceneOperations.CurrentSceneClearHistory);
                await TerminateLastAsync(clearHistory);
            }

            // 連続で同じシーンに遷移するリクエストは禁止する
        }

        private void CreateArgHandler<TArg>(IGameScene gameScene, TArg arg)
        {
            if (gameScene is IGameSceneArgHandler handler)
            {
                handler.ArgHandler = scene =>
                {
                    if (scene is IGameSceneArg<TArg> gameSceneArg)
                        return gameSceneArg.ArgHandle(arg);

                    return Task.CompletedTask;
                };
            }
        }

        private UniTaskCompletionSource<TResult> CreateResultTcs<TResult>(IGameScene gameScene)
        {
            if (gameScene is IGameSceneResult<TResult> result)
            {
                return result.ResultTcs = new UniTaskCompletionSource<TResult>();
            }

            return null;
        }

        /// <summary>
        /// シーンを起動させる共通処理
        /// </summary>
        private async Task TransitionCore(IGameScene gameScene, bool isDialog = false)
        {
            gameScene.State = GameSceneState.Processing;

            if (gameScene.ArgHandler != null)
                await gameScene.ArgHandler.Invoke(gameScene);

            // Memo: ダイアログかではなく、遷移タイプがOverlayかで判断したい
            if (!isDialog) await GlobalMessageBroker.GetAsyncPublisher<int, bool>().PublishAsync(MessageKey.GameScene.TransitionEnter, true);
            await gameScene.PreInitialize();
            await gameScene.LoadAsset();
            await gameScene.Startup();
            if (!isDialog) await GlobalMessageBroker.GetAsyncPublisher<int, bool>().PublishAsync(MessageKey.GameScene.TransitionFinish, true);
            await gameScene.Ready();
        }

        private async Task<TResult> ResultCore<TResult>(Type type, UniTaskCompletionSource<TResult> tcs)
        {
            if (tcs == null) return default;

            try
            {
                var result = await tcs.Task;
                await TerminateAsync(type, clearHistory: true); // リザルトがセットされ、プロセスが終わったら閉じる, 遷移履歴も消す
                return result;
            }
            catch (OperationCanceledException)
            {
                // キャンセルされたら閉じるようにしておく
                tcs.TrySetCanceled();
            }

            return default;
        }

        public bool IsProcessing(Type type)
        {
            var currentNode = _gameScenes.Last;
            if (currentNode != null)
            {
                var target = currentNode.Value;
                return target.type == type && target.gameScene.State is GameSceneState.Processing;
            }

            return false;
        }

        private Task SleepAsync()
        {
            var currentNode = _gameScenes.Last;
            if (currentNode != null)
            {
                var target = currentNode.Value;
                if (target.gameScene != null)
                {
                    target.gameScene.State = GameSceneState.Sleep;
                    return target.gameScene.Sleep();
                }
            }

            return Task.CompletedTask;
        }

        private Task RestartAsync()
        {
            var currentNode = _gameScenes.Last;
            if (currentNode != null)
            {
                var target = currentNode.Value;
                if (target.gameScene != null)
                {
                    target.gameScene.State = GameSceneState.Processing;
                    return target.gameScene.Restart();
                }
            }

            return Task.CompletedTask;
        }

        public async Task TerminateAsync(Type type, bool clearHistory = false)
        {
            var target = _gameScenes.LastOrDefault(x => x.type == type);
            if (target.gameScene != null)
            {
                await TerminateCore(target.gameScene);

                if (clearHistory) _gameScenes.Remove(target);
            }
        }

        // 最後に開いたものを閉じる
        public async Task TerminateLastAsync(bool clearHistory = false)
        {
            var currentNode = _gameScenes.Last;
            if (currentNode != null)
            {
                var target = currentNode.Value;
                if (target.gameScene != null)
                {
                    await TerminateAsync(target.type, clearHistory);
                }
            }
        }

        private async Task TerminateAllDialogAsync()
        {
            foreach (var (type, gameScene) in _gameScenes.Reverse())
            {
                if (gameScene is IGameDialogScene)
                {
                    await TerminateAsync(type, clearHistory: true);
                }
            }
        }

        private async Task TerminateAllAsync()
        {
            foreach (var (_, gameScene) in _gameScenes.Reverse())
            {
                await TerminateCore(gameScene);
            }

            _gameScenes.Clear();
        }

        private async Task TerminateCore(IGameScene gameScene)
        {
            if (gameScene != null)
            {
                gameScene.State = GameSceneState.Terminate;
                await gameScene.Terminate();
            }
        }

        #region UnityScene

        private readonly List<SceneInstance> _unityScenes = new();

        public async Task<SceneInstance> LoadUnitySceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool activateOnLoad = true)
        {
            var sceneInstance = await AssetService.LoadSceneAsync(sceneName, loadSceneMode, activateOnLoad);
            if (sceneInstance.Scene.IsValid())
            {
                _unityScenes.Add(sceneInstance);
            }

            return sceneInstance;
        }

        public async Task UnloadUnitySceneAsync(SceneInstance sceneInstance)
        {
            await AssetService.UnloadSceneAsync(sceneInstance);

            if (_unityScenes.Contains(sceneInstance))
                _unityScenes.Remove(sceneInstance);
        }

        public async Task UnloadUnitySceneAllAsync()
        {
            foreach (var sceneInstance in _unityScenes)
            {
                await AssetService.UnloadSceneAsync(sceneInstance);
            }

            _unityScenes.Clear();
        }

        #endregion
    }
}