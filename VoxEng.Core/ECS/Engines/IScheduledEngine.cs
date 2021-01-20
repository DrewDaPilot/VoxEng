using System;
using System.Threading.Tasks;
using Svelto.ECS;

namespace VoxEng.Core.ECS.Engines
{
    public interface IScheduledEngine: IEngine
    {

        
        /// <summary>
        /// Invoked when the scheduled engine is ready for execution.
        /// </summary>
        public abstract void Ready();

        /// <summary>
        /// Called when this scheduled engine is no longer being scheduled,
        /// and is being disposed of.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// A method that is invoked as frequently as possible. Only use for logic that needs to be run every frame, otherwise use <see cref="TickFixed"/>.
        /// </summary>
        public virtual void Tick()
        {
            
        }
        
        /// <summary>
        /// A method that is invoked at a fixed interval.
        /// The default interval is 60tps, but this can be changed using <see cref="VoxEng.Core.ECS.Scheduler.TickRateAttribute"/>.
        /// </summary>
        public virtual void TickFixed()
        {
            
        }

        /// <summary>
        /// The id of this scheduled engine. Used so that the tick-rate can be tracked on
        /// a per-engine basis.
        /// </summary>
        public Guid Id { get; }
    }
}