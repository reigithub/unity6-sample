namespace Game.Core.MessagePipe
{
    public partial struct MessageKey
    {
        private struct Offset
        {
            public const int System = 0;
            public const int Stat = 100;
        }

        public struct Stat
        {
            public const int AddScore = Offset.Stat + 0;
            public const int EnemyCollied = Offset.Stat + 1;
        }
    }
}