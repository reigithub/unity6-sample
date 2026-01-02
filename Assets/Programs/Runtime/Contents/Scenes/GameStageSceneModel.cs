using System;
using Game.Core.MasterData;
using Game.Core.MasterData.MemoryTables;
using Game.Core.Services;
using UnityEngine;

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

    public struct GameStageResultData
    {
        public GameStageResult StageResult { get; set; }

        public int CurrentTime { get; set; }
        public int TotalTime { get; set; }

        public int CurrentPoint { get; set; }
        public int MaxPoint { get; set; }

        public int PlayerCurrentHp { get; set; }
        public int PlayerMaxHp { get; set; }

        public int? NextStageId { get; set; }
    }

    public class GameStageSceneModel
    {
        private GameServiceReference<MasterDataService> _masterDataService;
        protected MemoryDatabase MemoryDatabase => _masterDataService.Reference.MemoryDatabase;

        public StageMaster StageMaster { get; private set; }
        public PlayerMaster PlayerMaster { get; private set; }

        // Memo: データの持ち方は後日検討するとして、一旦動くものを作成
        public GameStageState StageState { get; set; }
        public GameStageResult StageResult { get; set; }

        public int CurrentTime { get; set; }
        public int TotalTime { get; set; }

        public int CurrentPoint { get; set; }
        public int MaxPoint { get; set; }

        public int PlayerCurrentHp { get; set; }
        public int PlayerMaxHp { get; set; }

        public float PlayerCurrentStamina { get; set; }
        public float PlayerMaxStamina { get; set; }

        public GameStageSceneModel()
        {
            StageState = GameStageState.None;
            StageResult = GameStageResult.None;
        }

        public void Initialize(int stageId)
        {
            var stageMaster = MemoryDatabase.StageMasterTable.FindById(stageId);
            var playerMaster = MemoryDatabase.PlayerMasterTable.FindById(stageMaster.PlayerId ?? 1);
            StageMaster = stageMaster;
            PlayerMaster = playerMaster;

            CurrentTime = stageMaster.TotalTime;
            TotalTime = stageMaster.TotalTime;
            CurrentPoint = 0;
            MaxPoint = stageMaster.MaxPoint;

            // Memo: プレイヤー情報はPlayerMasterを作成するか検討
            PlayerCurrentHp = playerMaster.MaxHp;
            PlayerMaxHp = playerMaster.MaxHp;
            PlayerCurrentStamina = playerMaster.MaxStamina;
            PlayerMaxStamina = playerMaster.MaxStamina;
        }

        public void ProgressTime()
        {
            CurrentTime--;
            CurrentTime = Math.Max(0, CurrentTime);
        }

        public void AddPoint(int point)
        {
            CurrentPoint = Mathf.Clamp(CurrentPoint + point, 0, MaxPoint);
        }

        public void PlayerHpDamaged(int hpDamage)
        {
            PlayerCurrentHp = Mathf.Clamp(PlayerCurrentHp - hpDamage, 0, PlayerMaxHp);
        }

        public bool CanRun()
        {
            return PlayerCurrentStamina > 0f;
        }

        public bool IsTimeUp()
        {
            return CurrentTime <= 0;
        }

        public bool IsClear()
        {
            return CurrentPoint >= MaxPoint;
        }

        public bool IsFailed()
        {
            return PlayerCurrentHp <= 0 || IsTimeUp();
        }

        public bool CanPause()
        {
            return StageState == GameStageState.Start;
        }

        public GameStageResultData CreateStageResult()
        {
            return new GameStageResultData
            {
                StageResult = StageResult,
                CurrentTime = CurrentTime,
                TotalTime = TotalTime,
                CurrentPoint = CurrentPoint,
                MaxPoint = MaxPoint,
                PlayerCurrentHp = PlayerCurrentHp,
                PlayerMaxHp = PlayerMaxHp,
                NextStageId = StageMaster.NextStageId,
            };
        }
    }
}