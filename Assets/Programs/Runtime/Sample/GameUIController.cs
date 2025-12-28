using Game.Core.MessagePipe;
using Game.Core.Services;
using UnityChan;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// ゲームUIコントローラー
    /// </summary>
    public class GameUIController : MonoBehaviour //, SDUnityChanInputSystem.IUIActions
    {
        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        private SDUnityChanInputSystem _inputSystem;
        private SDUnityChanInputSystem.UIActions _ui;

        private bool _pause;

        public void Initialize()
        {
            // _gamePauseUI.Initialize();
        }

        private void Awake()
        {
            _inputSystem = new SDUnityChanInputSystem();
            _ui = _inputSystem.UI;
        }

        private void OnEnable()
        {
            _inputSystem.Enable();
            _ui.Enable();
        }

        private void OnDisable()
        {
            _inputSystem.Disable();
            _ui.Disable();
        }

        private void OnDestroy()
        {
            _inputSystem.Dispose();
        }

        private void Start()
        {
        }

        private void Update()
        {
            // _ui.{InputAction}.IsPressed() //押す～離す間
            // _ui.{InputAction}.WasPressedThisFrame() //押した瞬間
            // _ui.{InputAction}.WasReleasedThisFrame() //離した瞬間

            if (_ui.Pause.WasPressedThisFrame())
            {
                GamePauseUIDialog.RunAsync();
            }
        }

        private void FixedUpdate()
        {
        }
    }
}