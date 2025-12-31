using System.Linq;
using System.Threading.Tasks;
using Game.Core;
using Game.Core.Extensions;
using Game.Core.MessagePipe;
using Game.Core.Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Contents.Scenes
{
    public class GameTitleSceneComponent : GameSceneComponent
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        public void Initialize()
        {
            if (_startButton)
            {
                _startButton.onClick.AddListener(() => { StartStageAsync().Forget(); });
            }

            if (_quitButton)
            {
                _quitButton.onClick.AddListener(() => { GlobalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.Game.Quit, true); });
            }
        }

        private async Task StartStageAsync()
        {
            var master = MemoryDatabase.GameStageMasterTable.All
                .OrderBy(x => x.Id)
                .FirstOrDefault();
            var stageId = master?.Id ?? 1; // 本来はエラーメッセージだして落とす
            await SceneService.TransitionAsync<GameStageScene, GameStageSceneModel, int>(stageId);

            GlobalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.Game.Start, true);
        }
    }
}