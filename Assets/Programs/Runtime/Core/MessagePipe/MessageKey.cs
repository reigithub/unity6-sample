namespace Game.Core.MessagePipe
{
    public partial struct MessageKey
    {
        private struct Offset
        {
            public const int System = 0;
            public const int Game = 100;
            public const int GameScene = 200;
            public const int Player = 500;

            public const int Sample = 90000;
        }

        public struct Sample
        {
            public const int AddScore = Offset.Sample + 0;
            public const int EnemyCollied = Offset.Sample + 1;
        }

        public struct Game
        {
            public const int Start = Offset.Game + 0;
            public const int Quit = Offset.Game + 1;
            public const int Pause = Offset.Game + 2;
            public const int Resume = Offset.Game + 3;
            public const int Restart = Offset.Game + 4;
        }

        public struct GameScene
        {
            public const int TransitionEnter = Offset.GameScene + 0;
            public const int TransitionFinish = Offset.GameScene + 1;
        }

        public struct Player
        {
            public const int SpawnPlayer = Offset.Player + 0;
        }
    }
}