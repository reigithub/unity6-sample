using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class GamePauseUI : MonoBehaviour
    {
        [SerializeField]
        private Button _resumeButton;

        [SerializeField]
        private Button _retryButton;

        [SerializeField]
        private Button _returnButton;

        [SerializeField]
        private Button _quitButton;

        public void Initialize()
        {
            _resumeButton.onClick.AddListener(() =>
            {
                GameManager.Instance.GameResume();
                SetActive(false);
            });
            _retryButton.onClick.AddListener(() => { });
            _returnButton.onClick.AddListener(() => { });
            _quitButton.onClick.AddListener(() => { GameManager.Instance.GameQuit(); });
        }

        public void SetActive(bool active)
        {
            Time.timeScale = active ? 0f : 1f;
            gameObject.SetActive(active);
        }
    }
}