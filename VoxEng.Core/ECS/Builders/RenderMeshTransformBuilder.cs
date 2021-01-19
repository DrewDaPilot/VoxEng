using System.Numerics;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Descriptors;

namespace VoxEng.Core.ECS.Builders
{
    /// <summary>
    /// Builds an entity with a Transform and Mesh component for rendering.
    /// </summary>
    internal struct RenderMeshTransformBuilder
    {

        public void Build(IEntityFactory factory, MeshEntityComponent mesh, TransformEntityComponent transform)
        {
            EntityInitializer init = factory.BuildEntity
                <RenderMeshWithTransformDescriptor>
                (EgidFactory.GetNextId(), Groups.RenderMeshWithTransform.BuildGroup);
            
            init.Init(transform);
            
            init.Init(mesh);
        }
    }
}