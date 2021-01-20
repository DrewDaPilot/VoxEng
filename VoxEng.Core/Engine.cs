
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
    
    public class Engine
    {
        private CompositionRoot _root;
        
        /// <summary>
        /// Creates a new instance of the VoxEng.
        /// </summary>
        public Engine()
        {
            _root = new CompositionRoot();
            _root.AddCube();
        }

        public void Execute()
        {
            while (true)
            {
                _root.Scheduler.Execute();
            }
        }
    }
}