using System;
using System.Threading.Tasks;
using Game.Core.MasterData;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace Game.Core.Services
{
    public class MasterDataService : GameService
    {
        private GameServiceReference<AddressableAssetService> _assetService;
        protected AddressableAssetService AssetService => _assetService.Reference;

        public MemoryDatabase MemoryDatabase { get; private set; }

        protected internal override void Startup()
        {
            StaticCompositeResolver.Instance.Register(new[]
            {
                MasterMemoryResolver.Instance, // set MasterMemory generated resolver
                StandardResolver.Instance      // set default MessagePack resolver
            });

            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }

        protected internal override bool AllowResidentOnMemory => true;

        public async Task LoadMasterDataAsync()
        {
            var asset = await AssetService.LoadAssetAsync<TextAsset>("MasterDataBinary");
            var binary = asset.bytes;
            MemoryDatabase = new MemoryDatabase(binary, maxDegreeOfParallelism: Environment.ProcessorCount);
        }
    }
}