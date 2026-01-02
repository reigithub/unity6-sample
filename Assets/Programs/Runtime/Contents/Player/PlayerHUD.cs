using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core.MessagePipe;
using Game.Core.Services;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Contents.Player
{
    /// <summary>
    /// 簡易的なプレイヤーHUD
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _uiCanvasGroup;

        [SerializeField] private Slider _hpGauge;

        [SerializeField] private TextMeshProUGUI _currentHp;
        [SerializeField] private TextMeshProUGUI _maxHp;

        [SerializeField] private Slider _staminaGauge;

        [SerializeField] private TextMeshProUGUI _currentStamina;
        [SerializeField] private TextMeshProUGUI _maxStamina;

        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        public readonly ReactiveProperty<int> CurrentHp = new();
        public int _maxHpValue = 100;

        public readonly ReactiveProperty<float> CurrentStamina = new();
        public float _maxStaminaValue = 100f;

        public float _staminaDepleteRate = 10f;
        public float _staminaRegenRate = 5f;
        public bool _isRunning;

        private void Awake()
        {
            _uiCanvasGroup.alpha = 0f;
        }

        public void Initialize()
        {
            CurrentHp.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    _currentHp.text = x.ToString();
                    _hpGauge.value = x / 100f;
                }).AddTo(this);
            CurrentStamina.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    _currentStamina.text = x.ToString("0");
                    _staminaGauge.value = x / 100f;
                }).AddTo(this);

            CurrentHp.Value = _maxHpValue;
            CurrentStamina.Value = _maxStaminaValue;

            _maxHp.text = _maxHpValue.ToString();
            _maxStamina.text = _maxStaminaValue.ToString("0");
        }

        private　void Update()
        {
            if (_isRunning)
            {
                var nextStamina = CurrentStamina.Value - _staminaDepleteRate * Time.deltaTime;
                CurrentStamina.Value = Mathf.Clamp(nextStamina, 0f, _maxStaminaValue);
            }
            else
            {
                var nextStamina = CurrentStamina.Value + _staminaRegenRate * Time.deltaTime;
                CurrentStamina.Value = Mathf.Clamp(nextStamina, 0f, _maxStaminaValue);
            }
        }

        public void SetRunInput(bool isRunning)
        {
            _isRunning = isRunning;
        }


        public void DoFadeIn()
        {
            _uiCanvasGroup.DOFade(1f, 0.25f);
        }

        public void DoFadeOut()
        {
            _uiCanvasGroup.DOFade(0f, 0.25f);
        }
    }
}