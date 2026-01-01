using Cysharp.Threading.Tasks;
using Game.Core.MasterData;
using Game.Core.Services;
using UnityEngine;

namespace Game.Contents.Enemy
{
    /// <summary>
    /// エネミー生成地点
    /// </summary>
    public class EnemyStart : MonoBehaviour
    {
        private GameServiceReference<AddressableAssetService> _assetService;
        private AddressableAssetService AssetService => _assetService.Reference;

        private GameServiceReference<MasterDataService> _masterDataService;
        private MemoryDatabase MemoryDatabase => _masterDataService.Reference.MemoryDatabase;

        public async UniTask LoadEnemyAsync(GameObject player, int stageId)
        {
            var spawnMasters = MemoryDatabase.EnemySpawnMasterTable.FindByStageId(stageId);

            foreach (var spawnMaster in spawnMasters)
            {
                var enemyMaster = MemoryDatabase.EnemyMasterTable.FindById(spawnMaster.EnemyId);
                var enemy = await AssetService.InstantiateAsync(enemyMaster.AssetName, transform);
                if (enemy.TryGetComponent<EnemyController>(out var enemyController))
                {
                    enemyController.Initialize(player, enemyMaster, spawnMaster);
                }
            }
        }
    }
}