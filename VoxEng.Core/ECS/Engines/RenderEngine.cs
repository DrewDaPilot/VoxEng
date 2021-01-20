using System;
using Svelto.ECS;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Scheduler;
using VoxEng.Core.Rendering;

namespace VoxEng.Core.ECS.Engines
{
    /// <summary>
    /// A scheduled engine which renders all entities which are flagged as visible and contain the necessary components.
    /// </summary>
    internal class RenderEngine: IScheduledEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        /// <summary>
        /// The unique id of this engine which is useful for tracking tick timings.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        
        private RenderAgent _agent;

        /// <summary>
        /// Called when rendering is ready.
        /// Used for initializing render resources.
        /// </summary>
        public void Ready()
        {

        }
        
        /// <summary>
        /// Called when this rendering instance is to be disposed of.
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Invoked on each frame, this should render all entities that contain transforms and meshes, and are tagged for rendering.
        /// No tick rate is specified because we want graphics to run as fast as possible.
        /// </summary>
        public void Tick()
        {
            var cam = entitiesDB.QueryUniqueEntity<TransformEntityComponent>(Groups.CameraGroup);
            
            foreach (var ((transforms, meshes, count), _) in entitiesDB
                .QueryEntities<TransformEntityComponent, MeshEntityComponent>(
                    Groups.RenderMeshWithTransform.Groups))
            {
                _agent.Window.PumpEvents();
                _agent.PreDraw(cam.Position);

                //For each group of components,
                for (int i = 0; i < count; i++)
                {
                    ref var transformComp   = ref transforms[i];
                    ref var meshComp = ref meshes[i];
                    
                    _agent.DrawEntity(transformComp, meshComp);
                }
                
                _agent.PostDraw();
            }
        }

        [TickRate(120)]
        public void TickFixed()
        {
            
        }

        public RenderEngine(RenderAgent agent)
        {
            _agent = agent;
        }
    }
}