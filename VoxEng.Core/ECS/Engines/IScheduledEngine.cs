using System;

namespace VoxEng.Core.ECS.Engines
{
    public interface IScheduledEngine
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
        /// A method that is invoked as frequently as possible.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// A method that is invoked at a fixed interval.
        /// The default interval is 60tps, but this can be changed using the <see cref="TickRate"/> attribute.
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