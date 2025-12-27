using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Core.Scenes;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Game.Core.Services
{
    public class GameSceneService<TGameScene> : GameService
        where TGameScene : GameScene
    {
        private GameServiceReference<AddressableAssetService> _assetService;
        protected AddressableAssetService AssetService => _assetService.Reference;

        protected internal override bool AllowResidentOnMemory => true;
    }

    /// <summary>
    /// GameSceneの遷移挙動を制御するサービス
    /// </summary>
    public partial class GameSceneService : GameSceneService<GameScene>
    {
        // 1.履歴を持って、一つ前のシーンへ戻れるように
        // 2.現在シーンをスリープさせて、次のシーンを表示する処理
        // 3.ダイアログ用のオーバーレイ表示
        // 4.マルチ解像度対応（いずれどこかで）
        private readonly List<GameScene> _gameScenes = new();
        private readonly List<SceneInstance> _unityScenes = new();

        public async Task TransitionAsync<TScene>()
            where TScene : GameScene
        {
            await TerminateAllAsync();

            var gameScene = GameSceneHelper.CreateInstance(typeof(TScene));
            if (gameScene is not null)
            {
                await TransitionCore(gameScene);
                _gameScenes.Add(gameScene);
            }
        }

        public async Task TransitionAsync<TScene, TArg>(TArg arg)
            where TScene : GameScene, IGameSceneArg<TArg>
        {
            await TerminateAllAsync();

            var gameScene = GameSceneHelper.CreateInstance(typeof(TScene));
            if (gameScene is not null)
            {
                await ((TScene)gameScene).SetArg(arg);

                await TransitionCore(gameScene);
                _gameScenes.Add(gameScene);
            }
        }

        public async Task<TResult> TransitionAsync<TScene, TResult>()
            where TScene : GameScene, IGameSceneResult<TResult>
        {
            var gameScene = GameSceneHelper.CreateInstance(typeof(TScene));
            if (gameScene is not null)
            {
                var tcs = ((TScene)gameScene).ResultTcs = new UniTaskCompletionSource<TResult>();

                await TransitionCore(gameScene);
                _gameScenes.Add(gameScene);

                return await tcs.Task;
            }

            return default;
        }

        public async Task<TResult> TransitionAsync<TScene, TArg, TResult>(TArg arg)
            where TScene : GameScene, IGameSceneArg<TArg>, IGameSceneResult<TResult>
        {
            var gameScene = GameSceneHelper.CreateInstance(typeof(TScene));
            if (gameScene is not null)
            {
                await ((TScene)gameScene).SetArg(arg);
                var tcs = ((TScene)gameScene).ResultTcs = new UniTaskCompletionSource<TResult>();

                await TransitionCore(gameScene);
                _gameScenes.Add(gameScene);

                return await tcs.Task;
            }

            return default;
        }

        private async Task TransitionCore(GameScene gameScene)
        {
            await gameScene.LoadAsset();
            await gameScene.PreInitialize();
            await gameScene.Initialize();
            await gameScene.PostInitialize();
        }

        private async Task TerminateAllAsync()
        {
            foreach (var s in _gameScenes)
            {
                await s.Terminate();
            }

            _gameScenes.Clear();
        }

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
    }
}