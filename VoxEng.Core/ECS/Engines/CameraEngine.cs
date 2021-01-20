using System;
using System.Collections.Generic;
using Svelto.ECS;
using Veldrid;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Scheduler;
using VoxEng.Core.Rendering;

namespace VoxEng.Core.ECS.Engines
{
    internal class CameraEngine: IScheduledEngine, IQueryingEntitiesEngine
    {
        private RenderAgent _agent;
        private bool W;
        private bool S;
        private bool A;
        private bool D;

        public CameraEngine(RenderAgent agent)
        {

            _agent = agent;
            _agent.Window.KeyDown += delegate(KeyEvent e)
            {
                if (e.Key == Key.W)
                {
                    W = true;
                }

                if (e.Key == Key.S)
                {
                    S = true;
                }

                if (e.Key == Key.A)
                {
                    A = true;
                }

                if (e.Key == Key.D)
                {
                    D = true;
                }
            }; 
            _agent.Window.KeyUp += delegate(KeyEvent e)
            {
                if (e.Key == Key.W)
                {
                    W = false;
                }

                if (e.Key == Key.S)
                {
                    S = false;
                }

                if (e.Key == Key.A)
                {
                    A = false;
                }

                if (e.Key == Key.D)
                {
                    D = false;
                }
            };
        }
        
        public void Ready()
        {

        }

        public EntitiesDB entitiesDB { get; set; }

        public void Dispose()
        {
            
        }

        [TickRate(60)]
        public void TickFixed()
        {

            ref var cam = ref entitiesDB.QueryUniqueEntity<TransformEntityComponent>(Groups.CameraGroup);

            if (W)
                cam.Position.Z += 0.1f;
            if (S)
                cam.Position.Z -= 0.1f;
            if (A)
                cam.Position.X += 0.1f;
            if (D)
                cam.Position.X -= 0.1f;
        }

        public Guid Id { get; } = Guid.NewGuid();
    }
}