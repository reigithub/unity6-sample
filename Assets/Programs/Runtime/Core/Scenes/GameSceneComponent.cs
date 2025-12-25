using System.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Scenes
{
    public abstract class GameSceneComponent : MonoBehaviour
    {
        public virtual Task Initialize() => Task.CompletedTask;
    }
}