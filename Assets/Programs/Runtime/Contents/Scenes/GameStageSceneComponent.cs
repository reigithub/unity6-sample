using System.Threading.Tasks;
using Game.Core.Scenes;
using TMPro;
using UnityEngine;

namespace Game.Contents.Scenes
{
    public class GameStageSceneComponent : GameSceneComponent
    {
        [SerializeField] private TextMeshProUGUI _currentPoint;
        [SerializeField] private TextMeshProUGUI _maxPoint;

        private GameStageSceneModel _sceneModel;

        public Task Initialize(GameStageSceneModel sceneModel)
        {
            _sceneModel = sceneModel;
            UpdateView();
            return Task.CompletedTask;
        }

        public void UpdateView()
        {
            _currentPoint.text = _sceneModel.Point.ToString();
            _maxPoint.text = _sceneModel.MaxPoint.ToString();
        }
    }
}