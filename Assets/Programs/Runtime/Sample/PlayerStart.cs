using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Core.Services;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// プレイヤー生成地点
    /// </summary>
    public class PlayerStart : MonoBehaviour
    {
        private void OnEnable()
        {
            LoadPlayerAsync().Forget();
        }

        private async UniTask LoadPlayerAsync()
        {
            var assetService = GameServiceManager.Instance.GetService<AddressableAssetService>();
            var playerPrefab = await assetService.LoadAssetAsync<GameObject>("Assets/Prefabs/Player_UnityChan.prefab");
            var player = Instantiate(playerPrefab, transform);

            // Memo: この辺、もう少しキレイにかけるはず...
            await GameManager.Instance.LoadCommonObjectsTask;
            GameCommonObjects.Instance.SetPlayer(player);

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.TryGetComponent<EnemyMovement>(out var enemyMovement))
                {
                    enemyMovement.SetPlayer(player);
                }
            }

            // var messageBrokerService = GameServiceManager.Instance.GetService<MessageBrokerService>();
            // var globalMessageBroker = messageBrokerService.GlobalMessageBroker;
            // var publisher = globalMessageBroker.GetPublisher<int, GameObject>();
            // publisher.Publish(MessageKey.Player.SpawnPlayer, player);
        }
    }
}