using System.Threading.Tasks;
using Game.Core.Scenes;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Game.Contents.Scenes
{
    public class GameStageScene : GamePrefabScene<GameStageScene, GameStageSceneComponent>, IGameSceneArg<string>
    {
        protected override string AssetPathOrAddress => "Assets/Prefabs/GameStageScene.prefab";

        private string _stageName;
        private SceneInstance _stageSceneInstance;

        public Task PreInitialize(string stageName)
        {
            _stageName = stageName;
            return Task.CompletedTask;
        }

        public override async Task<GameObject> LoadAsset()
        {
            var instance = await base.LoadAsset();
            _stageSceneInstance = await AssetService.LoadSceneAsync(_stageName);
            return instance;
        }

        public override async Task Startup()
        {
            var playerStart = GameSceneHelper.GetPlayerStart(_stageSceneInstance.Scene);
            await SceneComponent.Initialize(playerStart);
        }

        public override async Task Terminate()
        {
            await base.Terminate();
            await AssetService.UnloadSceneAsync(_stageSceneInstance);
        }
    }
}