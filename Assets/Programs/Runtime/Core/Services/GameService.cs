namespace Game.Core.Services
{
    public interface IGameService
    {
        public void Startup();
        public void Shutdown();
    }

    public abstract class GameService : IGameService
    {
        public virtual void Startup()
        {
        }

        public virtual void Shutdown()
        {
        }
    }
}