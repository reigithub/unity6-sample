namespace Game.Core.Scenes
{
    public abstract class GameSceneBase
    {
        protected internal virtual void Initialize()
        {
        }

        protected internal virtual void Terminate()
        {
        }

        protected internal virtual void Update()
        {
        }

        protected internal virtual void Sleep()
        {
        }

        protected internal virtual void Restart()
        {
        }
    }

    public enum GameSceneState
    {
        Ready,
        Running,
        Sleeping,
        Slept,
        Wakeup,
        GotTopSceneFocus,
        Destroy,
        ReadyButDestroy,
    }

    public class GameSceneContext
    {
        public GameSceneBase Scene { get; set; }
        public GameSceneState State { get; set; }

        public bool IsReady => State == GameSceneState.Ready;
        public bool IsRunning => State == GameSceneState.Running || State == GameSceneState.GotTopSceneFocus;
        public bool IsWakeup => State == GameSceneState.Wakeup;
        public bool IsSleep => State == GameSceneState.Slept || State == GameSceneState.Sleeping;
        public bool IsDestroy => State == GameSceneState.Destroy || State == GameSceneState.ReadyButDestroy;
    }

    public enum SceneTransitionType
    {
        None,
        Normal,
        Sleep,
        Cross,
        CrossSleep,
        Over,
        OverClose,
        OverCloseAll,
        Previous,
        PreviousCross,
        BackTo,
        Clean,
        ClearHistory,
    }
}