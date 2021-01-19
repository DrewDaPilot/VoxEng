
using System.Diagnostics;
using System.Threading.Tasks;
using Svelto.ECS;
using Veldrid.Sdl2;
using VoxEng.Core.ECS;
using VoxEng.Core.ECS.Scheduler;
using VoxEng.Core.ECS.Scheduler.Interfaces;
using VoxEng.Core.Rendering;

namespace VoxEng.Core
{
    
    public class Eng
    {
        private IEngineScheduler _scheduler;
        private Stopwatch _sw;
        
        private const uint TicksPerMillisecond = 1000;
        private const uint TicksPerSecond = 1000 * TicksPerMillisecond;
        private bool _running = true;

        const uint  DefaultPhysicsSimulationsPerSecond = 30;
        const uint  DefaultGraphicsFramesPerSecond     = 60;
        const float DefaultSimulationSpeed             = 1.0f;
        

        /// <summary>
        /// Creates a new instance of the VoxEng.
        /// </summary>
        public Eng()
        {
            _sw = Stopwatch.StartNew();
            var logic = new CompositionRoot();
            this._scheduler = logic.Scheduler;
            
            logic.AddPlane();
            
            for (int i = 0; i < 10000; i++)
            {
                logic.AddCube();
            }
        }

        public async Task Execute()
        {

            var renderTsk = new Task(async () =>
            {
                _scheduler.Execute();
            });

      
            ScheduledTask rend = new ScheduledTask(renderTsk);

            while (_running)
            {
                _scheduler.Execute();
            }
        }

        public void Stop()
        {
            _running = false;
        }
    }
}