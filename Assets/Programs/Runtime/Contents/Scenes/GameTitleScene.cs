using System.Threading.Tasks;
using Game.Core.Scenes;

namespace Game.Contents.Scenes
{
    public class GameTitleScene : GamePrefabScene<GameTitleScene, GameTitleSceneComponent>
    {
        protected override string AssetPathOrAddress => "Assets/Prefabs/GameTitleScene.prefab";

        public override Task Startup()
        {
            SceneComponent.Initialize();

            return base.Startup();
        }
    }
}