using Cysharp.Threading.Tasks;
using Game.Core.MessagePipe;
using Game.Core.Services;
using UnityEngine;

namespace Game.Contents.Enemy
{
    /// <summary>
    /// エネミー生成地点
    /// </summary>
    public class EnemyStart : MonoBehaviour
    {
        private GameServiceReference<MessageBrokerService> _messageBrokerService;
        private GlobalMessageBroker GlobalMessageBroker => _messageBrokerService.Reference.GlobalMessageBroker;

        public async UniTask LoadEnemyAsync(int stageId)
        {
            var assetService = GameServiceManager.Instance.GetService<AddressableAssetService>();
            var player = await assetService.InstantiateAsync("Assets/Prefabs/Player_SDUnityChan.prefab", transform);

            // TODO: マスターデータで指定した位置にスポーンして、プレイヤーをセット
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.TryGetComponent<EnemyController>(out var enemyMovement))
                {
                    enemyMovement.SetPlayer(player);
                }
            }

            GlobalMessageBroker.GetPublisher<int, GameObject>()
                .Publish(MessageKey.Player.SpawnPlayer, player);
        }
    }
}