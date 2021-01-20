using System;
using System.Numerics;
using Svelto.ECS;
using VoxEng.Core.ECS.Builders;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Engines;
using VoxEng.Core.ECS.Scheduler;
using VoxEng.Core.ECS.Scheduler.Interfaces;
using VoxEng.Core.Rendering;

namespace VoxEng.Core.ECS
{
    public class CompositionRoot
    {
        /// <summary>
        /// The scheduler used to schedule the execution of all systems.
        /// </summary>
        readonly IEngineScheduler _scheduler;
        
        /// <summary>
        /// The reporter used to record (for the frame debugger and otherwise) how long
        /// executions take.
        /// </summary>
        readonly IEngineReporter _reporter;
        
        /// <summary>
        /// The submitter used for submitting component changes
        /// back into the component pool.
        /// </summary>
        readonly SubmissionScheduler _submitter;

        readonly RenderAgent _renderAgent;


        public IEngineScheduler Scheduler => this._scheduler;

        public IEntityFactory fac;

        private EnginesRoot _root;
        
        public CompositionRoot()
        {
            _submitter = new SubmissionScheduler();
            _scheduler = new DefaultEngineScheduler(_submitter);
            
            var enginesRoot = new EnginesRoot(_submitter);

            _root = enginesRoot;
            
            var entityFactory = enginesRoot.GenerateEntityFactory();

            //The render agent used for managing managed graphics resources.
            _renderAgent = new RenderAgent(new AgentConfig
            {
                Width = 960,
                Height = 540,
                Fov = 90f,
                VSync = false
            });

            //The render-engine (system) used for rendering entities with the mesh/transform
            //components.
            var eng = new RenderEngine(_renderAgent);

            var cam = new CameraEngine(_renderAgent);
            
            //Schedule the render-engine for execution.
            enginesRoot.AddEngine(eng);
            enginesRoot.AddEngine(cam);
            
            _scheduler.RegisterGraphicsEngine(eng);
            _scheduler.RegisterEngine(cam);


            TransformEntityComponent camTrans = new TransformEntityComponent()
            {
                Position = new Vector3(0f, 0f, -10f),
                Rotation = Quaternion.Identity,
                Scale = new Vector3(Single.NaN)
            };
            new CameraBuilder().Build(entityFactory, camTrans);

            fac = entityFactory;
        }

        public void AddEngine(IScheduledEngine eng)
        {
            _root.AddEngine(eng);
            _scheduler.RegisterEngine(eng);
        }
        
        public void AddCube()
        {
            MeshEntityComponent comp = MeshEntityComponent.Cube();
            comp.BufferIndex = _renderAgent.AllocBuffer(comp);
            
            TransformEntityComponent trans = new TransformEntityComponent()
            {
                Position = new Vector3(3f, 1f, 1f),
                Scale = Vector3.One,
                Rotation = Quaternion.CreateFromAxisAngle(new Vector3(1,0,0), (float) Math.PI / 4),
            };
            
            new RenderMeshTransformBuilder().Build(fac, comp, trans);

            _submitter.SubmitEntities();
        }
        
    }
}