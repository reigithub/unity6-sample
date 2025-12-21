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

    /// <summary>
    /// リストに存在するシーンのステータスを表します
    /// </summary>
    public enum GameSceneState
    {
        /// <summary>
        /// シーンの開始準備が完了しました
        /// </summary>
        Ready,

        /// <summary>
        /// シーンは稼働中です
        /// </summary>
        Running,

        /// <summary>
        /// シーンは一時停止します
        /// </summary>
        Sleeping,

        /// <summary>
        /// シーンは一時停止しています
        /// </summary>
        Sleeped,

        /// <summary>
        /// シーンが一時停止から稼動します
        /// </summary>
        Wakeup,

        /// <summary>
        /// シーンはトップシーンの状態を取得しました
        /// </summary>
        GotTopSceneFocus,

        /// <summary>
        /// シーンは解放される事のマークをされています
        /// </summary>
        Destroy,

        /// <summary>
        /// シーンは動作開始準備完了状態だったが、解放される対象としてマークされました
        /// </summary>
        ReadyButDestroy,
    }

    /// <summary>
    /// シーンの管理状態を保持するコンテキストクラスです
    /// </summary>
    public class GameSceneContext
    {
        public GameSceneBase Scene { get; set; }
        public GameSceneState State { get; set; }

        public bool IsReady => State == GameSceneState.Ready;
        public bool IsRunning => State == GameSceneState.Running || State == GameSceneState.GotTopSceneFocus;
        public bool IsWakeup => State == GameSceneState.Wakeup;
        public bool IsSleep => State == GameSceneState.Sleeped || State == GameSceneState.Sleeping;
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

        // 正確には遷移ではないけど
        Clean,
        ClearHistory,
    }
}