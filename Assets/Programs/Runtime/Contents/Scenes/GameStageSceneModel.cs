namespace Game.Contents.Scenes
{
    public enum GameStageState
    {
        None,
        Ready,
        Start,
        Retry,
        Result,
        Finish
    }

    public enum GameStageResult
    {
        None,
        Clear,
        Failed,
    }

    public class GameStageSceneModel
    {
        // Memo: データの持ち方は後日検討するとして、一旦動くものを作成
        public GameStageState StageState { get; set; }
        public GameStageResult StageResult { get; set; }
        public int Point { get; set; }
        public int MaxPoint { get; set; }

        public int PlayerHp { get; set; }
        public int PlayerMaxHp { get; set; }
        public float PlayerStamina { get; set; }
        public float PlayerMaxStamina { get; set; }

        public GameStageSceneModel()
        {
            StageState = GameStageState.None;
            StageResult = GameStageResult.None;
            Point = 0;
            MaxPoint = 5;

            PlayerHp = 5;
            PlayerMaxHp = 5;
            PlayerStamina = 100f;
            PlayerMaxStamina = 100f;
        }
    }
}