using Game.Core;
using Game.Core.MessagePipe;
using Game.Core.Services;
using UnityChan;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// SD-Unityちゃん用のプレイヤーコントローラー
    /// </summary>
    public class SDUnityChanController : MonoBehaviour
    {
        [SerializeField] private float _speed = 1f;

        private SDUnityChanInputSystem _inputSystem;
        private SDUnityChanInputSystem.PlayerActions _player;

        // TODO: StateMachineでアニメーション操作
        private Animator _animator;

        private GlobalMessageBroker _globalMessageBroker;

        private void Awake()
        {
            _inputSystem = new SDUnityChanInputSystem();
            _player = _inputSystem.Player;
            //m_Player.Move.performed += OnMove;
            //m_Player.Move.canceled += OnMove;
        }

        private void OnEnable()
        {
            _inputSystem.Enable();
            _globalMessageBroker = GameServiceManager.Instance.GetService<MessageBrokerService>().GlobalMessageBroker;
        }

        private void OnDisable() => _inputSystem.Disable();
        private void OnDestroy() => _inputSystem.Dispose();

        private void Start()
        {
            TryGetComponent<Animator>(out _animator);
        }

        void Update()
        {
            var moveValue = _player.Move.ReadValue<Vector2>();
            var direction = new Vector3(moveValue.x, 0.0f, moveValue.y).normalized;

            // 移動
            transform.Translate(direction * _speed * Time.deltaTime, Space.World);

            // 向き
            if (!Mathf.Approximately(direction.magnitude, 0f))
            {
                Quaternion from = transform.rotation;
                Quaternion to = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(from, to, 720f * Time.deltaTime);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.name.Contains("Enemy"))
            {
                other.gameObject.SetActive(false);

                _globalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.Sample.EnemyCollied, true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Contains("PickUp"))
            {
                other.gameObject.SetActive(false);

                _globalMessageBroker.GetPublisher<int, int>().Publish(MessageKey.Sample.AddScore, 1);
            }
        }
    }
}