using System.Threading.Tasks;
using Game.Core.Extensions;
using Game.Core.MessagePipe;
using Game.Core.Scenes;
using MessagePipe;
using R3;
using Sample;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Game.Contents.Scenes
{
    public class GameStageScene : GamePrefabScene<GameStageScene, GameStageSceneComponent>, IGameSceneModel<GameStageSceneModel>, IGameSceneArg<string>
    {
        protected override string AssetPathOrAddress => "Assets/Prefabs/GameStageScene.prefab";

        public GameStageSceneModel SceneModel { get; set; }

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
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Ready, handler: async (_, _) =>
                {
                    SceneModel.StageState = GameStageState.Ready;
                    //カウントダウンしてスタート
                    await GameCountdownUIDialog.RunAsync();
                    GlobalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.GameStage.Start, true);
                })
                .AddTo(SceneComponent);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Start, handler: _ =>
                {
                    SceneModel.StageState = GameStageState.Start;
                    // UI操作可能になるタイミング
                })
                .AddTo(SceneComponent);
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Retry, handler: async (_, _) =>
                {
                    SceneModel.StageState = GameStageState.Retry;
                    // 現在のステージへ再遷移
                    await SceneService.TransitionAsync<GameStageScene, GameStageSceneModel, string>(_stageName);
                })
                .AddTo(SceneComponent);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Result, handler: _ =>
                {
                    // リザルト画面
                    SceneModel.StageState = GameStageState.Result;
                })
                .AddTo(SceneComponent);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Finish, handler: _ =>
                {
                    // 次のステージへ
                    SceneModel.StageState = GameStageState.Finish;
                })
                .AddTo(SceneComponent);


            // プレイヤー設定
            GlobalMessageBroker.GetSubscriber<int, int>()
                .Subscribe(MessageKey.Player.AddPoint, handler: point =>
                {
                    SceneModel.Point += point;
                    SceneComponent.UpdateView();
                })
                .AddTo(SceneComponent);

            // プレイヤー爆誕の儀
            var playerStart = GameSceneHelper.GetPlayerStart(_stageSceneInstance.Scene);
            await playerStart.LoadPlayerAsync();

            // Memo: ビューがモデルの変更を検知する方法については賛否あると思われるが一旦は愚直に渡す
            // (MessageBroker、MessagePipe.Request/Response、R3.ReactivePropertyとか疎結合化は後ほど検討する → やるにしも現在の規模では仰々しすぎないかということで…)
            await SceneComponent.Initialize(SceneModel);
            await base.Startup();
        }

        public override Task Ready()
        {
            // ゲーム開始準備OKの合図
            GlobalMessageBroker.GetAsyncPublisher<int, bool>().Publish(MessageKey.GameStage.Ready, true);
            return base.Ready();
        }

        public override async Task Terminate()
        {
            await AssetService.UnloadSceneAsync(_stageSceneInstance);
            await base.Terminate();
        }
    }
}