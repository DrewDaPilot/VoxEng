using System.Collections.Generic;
using System.Diagnostics;
using FixedMaths;
using VoxEng.Core.ECS.Engines;

namespace VoxEng.Core.ECS.Schedulers
{
    public class DefaultEngineScheduler
    {
        //A list of engines to schedule for execution.
        private List<IScheduledEngine> _engines;
        private IEngineSchedulerReporter _reporter;

        private Stopwatch _sw;

        public DefaultEngineScheduler()
        {
            _engines = new List<IScheduledEngine>();
            _sw = Stopwatch.StartNew();
            _reporter = new DefaultEngineSchedulerReporter();
        }

        /// <summary>
        /// Executes all scheduled systems immediately.
        /// </summary>
        public void Execute()
        {
            foreach (var eng in _engines)
            {
                var before = _sw.ElapsedTicks;

                eng.Run();
                
                _reporter.RecordTicksSpent(eng.Name, _sw.ElapsedTicks - before);
            }
        }

        public void RegisterEngine(IScheduledEngine eng)
        {
            _engines.Add(eng);
        }
    }
}