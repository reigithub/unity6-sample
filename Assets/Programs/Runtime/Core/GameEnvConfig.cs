using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "GameEnvConfig", menuName = "Project/GameEnvConfig")]
    public class GameEnvConfig : ScriptableObject
    {
        [SerializeField] private GameEnv env;

        public GameEnv Env => env;
    }
}