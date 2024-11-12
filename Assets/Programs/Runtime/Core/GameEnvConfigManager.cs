using UnityEngine;

namespace Game.Core
{
    public static class GameEnvConfigManager
    {
        private const string AssetPath = "GameEnvConfigs";
        private const string PlayerPrefsKey = "GameEnvConfig";

        public static GameEnvConfig Load()
        {
            var environment = Get();
            var assetPath = AssetPath + "/GameEnvConfig." + environment;
            return Resources.Load<GameEnvConfig>(assetPath);
        }

        public static GameEnvConfig[] LoadAll()
        {
            return Resources.LoadAll<GameEnvConfig>(AssetPath);
        }

        public static GameEnv Get()
        {
#if RELEASE
            return GameEnvironment.Release;
#endif

            return (GameEnv)PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        }

        public static void Set(GameEnv env)
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, (int)env);
            PlayerPrefs.Save();
        }
    }
}