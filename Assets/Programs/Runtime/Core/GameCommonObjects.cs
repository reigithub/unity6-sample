using Cysharp.Threading.Tasks;
using Game.Core.Extensions;
using Game.Core.Services;
using Game.Core.MessagePipe;
using MessagePipe;
using R3;
using Sample;
using TMPro;
using UnityEngine;

public class GameCommonObjects : MonoBehaviour
{
    private const string Address = "Assets/Prefabs/GameCommonObjects.prefab";

    public static GameCommonObjects Instance { get; private set; }

    public static async UniTask LoadAssetAsync(MessageBroker messageBroker)
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
            commonObjects.Initialize(messageBroker);
            Instance = commonObjects;
        }
    }

    [SerializeField] private CameraController _cameraController;

    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _winText;

    private MessageBroker _messageBroker;
    private int _count;

    private void Initialize(MessageBroker messageBroker)
    {
        _cameraController.enabled = false;

        _count = 0;
        SetCountText();

        _messageBroker = messageBroker;
        _messageBroker.AddMessageBroker<int, int>();
        _messageBroker.AddMessageBroker<int, bool>();
        _messageBroker.Build();
        _messageBroker.GetSubscriber<int, int>()
            .Subscribe(MessageKey.Stat.AddScore, handler: score =>
            {
                _count += score;
                SetCountText();
            })
            .AddTo(this);
        _messageBroker.GetSubscriber<int, bool>()
            .Subscribe(MessageKey.Stat.EnemyCollied, handler: isCollied =>
            {
                _winText.gameObject.SetActive(isCollied);
                if (isCollied) _winText.text = "You Lose...";
            })
            .AddTo(this);
    }

    private void SetCountText()
    {
        _countText.text = "Count: " + _count;

        bool isWin = _count >= 16;
        _winText.gameObject.SetActive(isWin);
        if (isWin) _winText.text = "You Win!!";
    }
}