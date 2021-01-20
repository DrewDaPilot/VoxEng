using Svelto.ECS;
using VoxEng.Core.ECS.Components;
using VoxEng.Core.ECS.Descriptors;

namespace VoxEng.Core.ECS.Builders
{
    internal struct CameraBuilder
    {
        public void Build(IEntityFactory factory, TransformEntityComponent transform)
        {
            EntityInitializer init = factory.BuildEntity
                <TransformDescriptor>
                (EgidFactory.GetNextId(), Groups.CameraGroup);
            
            init.Init(transform);
        }
    }
}