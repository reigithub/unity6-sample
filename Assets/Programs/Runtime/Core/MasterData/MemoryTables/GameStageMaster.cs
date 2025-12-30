using MasterMemory;
using MessagePack;

namespace Game.Core.MasterData.MemoryTables
{
    [MemoryTable("GameStageMaster"), MessagePackObject(true)]
    public sealed partial class GameStageMaster
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }

        public int MaxPoint { get; set; }

        public int PlayerMaxHp { get; set; }

        public int? NextStageId { get; set; }
    }
}