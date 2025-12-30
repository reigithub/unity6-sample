using System.Linq;
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
                _startButton.onClick.AddListener(() =>
                {
                    var master = MemoryDatabase.GameStageMasterTable.All
                        .OrderBy(x => x.Id)
                        .FirstOrDefault();
                    if (master != null)
                        SceneService.TransitionAsync<GameStageScene, GameStageSceneModel, int>(master.Id).Forget();
                    else
                        SceneService.TransitionAsync<GameStageScene, GameStageSceneModel, int>(1).Forget(); // 本来はエラーメッセージだして落とす

                    GlobalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.Game.Start, true);
                });
            }

            if (_quitButton)
            {
                _quitButton.onClick.AddListener(() => { GlobalMessageBroker.GetPublisher<int, bool>().Publish(MessageKey.Game.Quit, true); });
            }
        }
    }
}