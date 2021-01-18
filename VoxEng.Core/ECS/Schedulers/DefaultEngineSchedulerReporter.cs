using System.Collections.Generic;

namespace VoxEng.Core.ECS.Schedulers
{
    public interface IEngineSchedulerReporter
    {
        void        RecordTicksSpent(string engine, long delta);
        void        IncrementFps();
        public void Reset();
    }

    public class DefaultEngineSchedulerReporter : IEngineSchedulerReporter
    {
        const int PtSize = 12;

        public void RecordTicksSpent(string engine, long delta)
        {
            if (!_ticksSpent.ContainsKey(engine))
                _ticksSpent[engine] = 0;

            _ticksSpent[engine] += delta;
        }

        public void IncrementFps()
        {
            _fpsAccumulator += 1;
        }
        

        public void Reset()
        {
            _report         = _ticksSpent;
            _ticksSpent     = new Dictionary<string, long>();
            _fps            = _fpsAccumulator;
            _fpsAccumulator = 0;
        }

        Dictionary<string, long> _report     = new Dictionary<string, long>();
        Dictionary<string, long> _ticksSpent = new Dictionary<string, long>();
        uint                     _fpsAccumulator;
        uint                     _fps;
    }
}