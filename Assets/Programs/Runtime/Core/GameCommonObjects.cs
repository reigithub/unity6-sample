using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Contents.Player;
using Game.Contents.Scenes;
using Game.Core.Extensions;
using Game.Core.Services;
using Game.Core.MessagePipe;
using MessagePipe;
using R3;
using Sample;
using UnityEngine;
using UnityEngine.UI;


namespace Game.Core
{
    public class GameCommonObjects : MonoBehaviour
    {
        private const string Address = "Assets/Prefabs/GameCommonObjects.prefab";

        public static GameCommonObjects Instance { get; private set; }

        public static async UniTask LoadAssetAsync()
        {
            var assetService = GameServiceManager.Instance.GetService<AddressableAssetService>();
            var prefab = await assetService.LoadAssetAsync<GameObject>(Address);
            if (prefab == null)
            {
                Debug.LogError($"Load Asset Failed. {Address}");
                Debug.Break();
            }

            Instance.SafeDestroy();
            Instance = null;

            var go = Instantiate(prefab);
            if (go.TryGetComponent<GameCommonObjects>(out var commonObjects))
            {
                DontDestroyOnLoad(go);
                commonObjects.Initialize();
                Instance = commonObjects;
            }
        }

        [SerializeField] private GameObject _mainCamera;
        [SerializeField] private PlayerFollowCameraController _playerFollowCameraController;

        [SerializeField] private GameUIController _gameUIController;

        [SerializeField] private Image _fadeImage;

        private GameServiceReference<GameSceneService> _sceneService;
        private GameSceneService SceneService => _sceneService.Reference;

        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        // Memo: どこに持つかは要検討としてマーク
        private bool _gameStart;
        private bool _gameStageStart;
        private int _count;

        private void Initialize()
        {
            _gameUIController.Initialize();
            _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 1f);
            Subscribe();
        }

        private void Subscribe()
        {
            // Memo: あらゆることをここで解決しようとしていて、良くないかもしれない…一旦はまだ小規模なので、、、デカくなりはじめたら見直しが必要
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.System.TimeScale, handler: (status, _) =>
                {
                    Time.timeScale = status ? 1f : 0f;
                    return UniTask.CompletedTask;
                })
                .AddTo(this);
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.System.Cursor, handler: (status, _) =>
                {
                    if (status)
                    {
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    return UniTask.CompletedTask;
                })
                .AddTo(this);

            // Game
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.Game.Start, handler: _ => { _gameStart = true; })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.Game.Quit, handler: _ => { GameManager.Instance.GameQuit(); })
                .AddTo(this);
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.Game.Pause, handler: async (_, _) =>
                {
                    if (!_gameStart || !_gameStageStart) return;
                    await GamePauseUIDialog.RunAsync();
                })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.Game.Resume, handler: _ =>
                {
                    if (!_gameStart || !_gameStageStart) return;
                    SceneService.TerminateAsync<GamePauseUIDialog>().Forget();
                })
                .AddTo(this);
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.Game.Return, handler: async (_, _) =>
                {
                    if (!_gameStart || !_gameStageStart) return;
                    _gameStart = false;
                    _gameStageStart = false;
                    // 現在のシーンを終了させてタイトルに戻る
                    await SceneService.TransitionAsync<GameTitleScene>();
                })
                .AddTo(this);

            // GameScene
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameScene.TransitionEnter, handler: _ => { _fadeImage.DOFade(1f, 0.5f); })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameScene.TransitionFinish, handler: _ => { _fadeImage.DOFade(0f, 1f); })
                .AddTo(this);

            // GameStage
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Ready, handler: (_, _) =>
                {
                    _gameStageStart = false;
                    return UniTask.CompletedTask;
                })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Start, handler: _ => { _gameStageStart = true; })
                .AddTo(this);
            GlobalMessageBroker.GetAsyncSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Retry, handler: (_, _) => UniTask.CompletedTask)
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Result, handler: _ => { })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameStage.Finish, handler: _ => { _gameStageStart = false; })
                .AddTo(this);

            // Player
            GlobalMessageBroker.GetSubscriber<int, GameObject>()
                .Subscribe(MessageKey.Player.SpawnPlayer, handler: player =>
                {
                    // 現在プレイヤーはUnityちゃんしかいない
                    if (player.TryGetComponent<SDUnityChanPlayerController>(out var controller))
                    {
                        controller.SetMainCamera(_mainCamera.transform);
                    }

                    _playerFollowCameraController.SetPlayer(player);
                })
                .AddTo(this);

            // UI
            GlobalMessageBroker.GetSubscriber<int, Vector2>()
                .Subscribe(MessageKey.UI.ScrollWheel, handler: scrollWheel =>
                {
                    if (!_gameStart) return;
                    _playerFollowCameraController.SetCameraRadius(scrollWheel);
                })
                .AddTo(this);
        }
    }
}