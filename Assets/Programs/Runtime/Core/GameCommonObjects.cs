using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Contents.Player;
using Game.Core.Extensions;
using Game.Core.Services;
using Game.Core.MessagePipe;
using MessagePipe;
using R3;
using Sample;
using TMPro;
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

        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private TextMeshProUGUI _winText;

        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        private int _count;

        private void Initialize()
        {
            GlobalMessageBroker.GetSubscriber<int, GameObject>()
                .Subscribe(MessageKey.Player.SpawnPlayer, handler: player =>
                {
                    if (player.TryGetComponent<SDUnityChanPlayerController>(out var controller))
                    {
                        controller.SetMainCamera(_mainCamera.transform);
                    }

                    _playerFollowCameraController.SetPlayer(player);
                })
                .AddTo(this);

            GlobalMessageBroker.GetSubscriber<int, int>()
                .Subscribe(MessageKey.Player.AddScore, handler: score =>
                {
                    _count += score;
                    SetCountText();
                })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.Player.EnemyCollied, handler: isCollied =>
                {
                    _winText.gameObject.SetActive(isCollied);
                    if (isCollied) _winText.text = "You Lose...";
                })
                .AddTo(this);

            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameScene.TransitionEnter, handler: _ => { _fadeImage.DOFade(1f, 0.5f); })
                .AddTo(this);
            GlobalMessageBroker.GetSubscriber<int, bool>()
                .Subscribe(MessageKey.GameScene.TransitionFinish, handler: _ => { _fadeImage.DOFade(0f, 1f); })
                .AddTo(this);

            _gameUIController.Initialize();
            _fadeImage.DOFade(1f, 0f);
            SetCountText();
        }

        private void SetCountText()
        {
            _countText.gameObject.SetActive(true);
            _countText.text = "Count: " + _count;

            bool isWin = _count >= 16;
            _winText.gameObject.SetActive(isWin);
            if (isWin) _winText.text = "You Win!!";
        }
    }
}