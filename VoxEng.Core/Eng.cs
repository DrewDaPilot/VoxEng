
using System.Diagnostics;
using Svelto.ECS;
using Veldrid.Sdl2;
using VoxEng.Core.ECS.Schedulers;
using VoxEng.Core.Rendering;

namespace VoxEng.Core
{
    
    public class Eng
    {
        private const uint TicksPerMillisecond = 1000;
        private const uint TicksPerSecond = 1000 * TicksPerMillisecond;
        private bool _running = false;

        const uint  DefaultPhysicsSimulationsPerSecond = 30;
        const uint  DefaultGraphicsFramesPerSecond     = 60;
        const float DefaultSimulationSpeed             = 1.0f;
        
        //The manager used for rendering objects.
        internal RenderManager _manager;

        /// <summary>
        /// Creates a new instance of the VoxEng.
        /// </summary>
        public Eng()
        {
            _manager = new RenderManager();
            
            var logic = new CompositionRoot()
        }

        public void Execute()
        {
            var renderAction = new ScheduledAction(() => 
                _scheduler.Execute(), 60, false);
            
            var clearFpsCounter = new ScheduledAction(() => 
                _schedulerReporter.Report())

            var clock = Stopwatch.StartNew();
            
            while (_running)
            {
                renderAction.Tick((ulong) clock.ElapsedTicks);
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private DefaultEngineScheduler _scheduler;
        private IEngineSchedulerReporter _scheduleReporter;
        
        public Sdl2Window Window => _manager._window;
    }
}