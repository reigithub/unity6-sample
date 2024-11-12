using UnityEngine;

namespace Game.Core
{
    public static class GameEntryPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var env = GameEnvConfigManager.Load();
            GameManager.Instance.Initialize(env);
            GameManager.Instance.GameStart();
        }
    }
}