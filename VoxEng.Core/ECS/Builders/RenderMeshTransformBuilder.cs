using System.Numerics;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Descriptors;

namespace VoxEng.Core.ECS.Builders
{
    internal struct RenderMeshTransformBuilder
    {

        public void Build(IEntityFactory factory, int bufferIdx)
        {
            EntityInitializer init = factory.BuildEntity
                <RenderMeshWithTransformDescriptor>
                (EgidFactory.GetNextId(), Groups.RenderMeshWithTransform.BuildGroup);
            
            init.Init(new TransformEntityComponent()
            {
                Position = Vector3.Zero,
                Scale = Vector3.One,
                Rotation = Quaternion.Identity,
            });
            
            init.Init(new MeshEntityComponent()
            {
                Verticies = new NativeDynamicArrayCast<Vector3>(),
                Indicies = new NativeDynamicArrayCast<ushort>(),
                BufferIndex = bufferIdx
            });
        }
    }
}