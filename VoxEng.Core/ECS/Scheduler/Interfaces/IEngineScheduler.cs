using Svelto.ECS;
using VoxEng.Core.ECS.Engines;

namespace VoxEng.Core.ECS.Scheduler.Interfaces
{
    public interface IEngineScheduler
    {
        /// <summary>
        /// Registers the specified engine for execution.
        /// </summary>
        /// <param name="eng">The engine to register.</param>
        public abstract void RegisterEngine(IScheduledEngine eng);

        public abstract void RegisterGraphicsEngine(IScheduledEngine eng);

        public abstract void Execute();
    }
}