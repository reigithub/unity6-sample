using System.Threading.Tasks;
using Game.Contents.Player;
using Game.Core.Scenes;
using UnityEngine;

namespace Game.Contents.Scenes
{
    public class GameStageSceneComponent : GameSceneComponent
    {
        [SerializeField] private PlayerStart _playerStart;

        public async Task Initialize(PlayerStart playerStart)
        {
            _playerStart = playerStart;
            await _playerStart.LoadPlayerAsync();
        }
    }
}