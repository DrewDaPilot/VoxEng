using System.Collections.Generic;
using System.Diagnostics;
using VoxEng.Core.ECS.Engines;
using VoxEng.Core.ECS.Scheduler.Interfaces;

namespace VoxEng.Core.ECS.Scheduler
{
    internal class DefaultEngineScheduler: IEngineScheduler
    {
        //The stopwatch used for timing mechanisms.
        private Stopwatch _stopwatch;
        
        //A list of scheduled engines for execution.
        private List<IScheduledEngine> _scheduled;

        private SubmissionScheduler _scheduler;
        
        public DefaultEngineScheduler(SubmissionScheduler sched)
        {
            _scheduler = sched;

            //Start the stopwatch.
            _stopwatch = Stopwatch.StartNew();

            _scheduled = new List<IScheduledEngine>();
        }

        public void Execute()
        {
            foreach (var eng in _scheduled)
            {
                var before = _stopwatch.ElapsedTicks;
                
                eng.Tick();
                
            }
        }

        /// <summary>
        /// Registers the scheduled engine for execution.
        /// </summary>
        /// <param name="eng">The engine to register</param>
        public void RegisterEngine(IScheduledEngine eng)
        {
            _scheduled.Add(eng);
        }
    }
}