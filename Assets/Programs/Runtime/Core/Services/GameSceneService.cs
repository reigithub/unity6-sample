using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Core.Scenes;
using UnityEngine;
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
        private readonly List<(Type type, IGameScene gameScene)> _gameScenes = new();
        private readonly List<SceneInstance> _unityScenes = new();

        public async Task TransitionAsync<TScene>()
            where TScene : IGameScene, new()
        {
            await TerminateAllAsync();

            var gameScene = new TScene();
            await TransitionCore(gameScene);
            _gameScenes.Add((typeof(TScene), gameScene));
        }

        public async Task TransitionAsync<TScene, TArg>(TArg arg)
            where TScene : IGameScene, IGameSceneArg<TArg>, new()
        {
            await TerminateAllAsync();

            var gameScene = new TScene();
            await gameScene.PreInitialize(arg);
            await TransitionCore(gameScene);
            _gameScenes.Add((typeof(TScene), gameScene));
        }

        public async Task<TResult> TransitionAsync<TScene, TResult>()
            where TScene : IGameScene, IGameSceneResult<TResult>, new()
        {
            var gameScene = new TScene();
            var tcs = gameScene.ResultTcs = new UniTaskCompletionSource<TResult>();

            await TransitionCore(gameScene);
            _gameScenes.Add((typeof(TScene), gameScene));

            return await tcs.Task;
        }

        public async Task<TResult> TransitionAsync<TScene, TArg, TResult>(TArg arg)
            where TScene : IGameScene, IGameSceneArg<TArg>, IGameSceneResult<TResult>, new()
        {
            var gameScene = new TScene();
            await gameScene.PreInitialize(arg);
            var tcs = gameScene.ResultTcs = new UniTaskCompletionSource<TResult>();

            await TransitionCore(gameScene);
            _gameScenes.Add((typeof(TScene), gameScene));

            return await tcs.Task;
        }


        public async Task<TResult> TransitionDialogAsync<TScene, TComponent, TResult>(Func<TScene, TComponent, Task> initializer = null)
            where TScene : GameScene<TScene, TComponent>, IGameSceneInitializer<TScene, TComponent>, IGameSceneResult<TResult>, new()
            where TComponent : GameSceneComponent
        {
            if (await TerminateAsync(typeof(TScene)))
                return default;

            // WARN: MonoBehaviourをnewしない方向で
            var gameScene = new TScene();
            gameScene.Scene = gameScene;
            gameScene.Initializer = initializer;
            var tcs = gameScene.ResultTcs = new UniTaskCompletionSource<TResult>();
            await TransitionCore(gameScene);
            _gameScenes.Add((typeof(TScene), gameScene));

            return await tcs.Task;
        }

        private async Task TransitionCore(IGameScene gameScene)
        {
            await gameScene.LoadAsset();
            await gameScene.PreInitialize();
            await gameScene.Startup();
            await gameScene.OnReady();
        }

        public async Task<bool> TerminateAsync(Type sceneType)
        {
            var target = _gameScenes.LastOrDefault(x => x.type == sceneType);
            if (target.type != null)
            {
                await target.gameScene.Terminate();
                _gameScenes.Remove(target);
                return true;
            }

            return false;
        }

        private async Task TerminateAllAsync()
        {
            foreach (var (_, s) in _gameScenes)
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