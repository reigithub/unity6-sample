using System;
using Game.Core.Scenes;

namespace Game.Core.Services
{
    public class GameSceneServiceBase<TGameSceneBase> where TGameSceneBase : GameSceneBase
    {
    }

    public partial class GameSceneService : GameSceneServiceBase<GameSceneBase>
    {
        public readonly struct SceneTransitionRequest
        {
            public readonly Type NextSceneType;
            public readonly SceneTransitionType TransitionType;

            public readonly short Token;

            public SceneTransitionRequest(
                Type nextSceneType,
                SceneTransitionType transitionType,
                short token)
            {
                NextSceneType = nextSceneType;
                TransitionType = transitionType;
                Token = token;
            }
        }
    }
}