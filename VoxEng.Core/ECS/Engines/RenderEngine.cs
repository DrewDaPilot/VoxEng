using FixedMaths;
using Svelto.ECS;
using VoxEng.Core.ECS.Components;

namespace VoxEng.Core.ECS.Engines
{
    public class RenderEngine: IScheduledEngine, IQueryingEntitiesEngine
    {
        public void Ready()
        {
            
        }

        public EntitiesDB entitiesDB { get; set; }
        
        
        [Freq(60)]
        public void Run(in FixedPoint delta)
        {
            foreach (var ((transforms, meshes, count), _) in entitiesDB
                .QueryEntities<TransformEntityComponent, MeshEntityComponent>(
                    Groups.RenderMeshWithTransform.Groups))
            {
                //For each group of components,
                for (int i = 0; i < count; i++)
                {
                    ref var transformComp   = ref transforms[i];
                    ref var meshComp = ref meshes[i];
                }
            }
        }

        public string Name { get; } = "RenderEngine";
    }
}