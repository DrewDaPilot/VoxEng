using Svelto.ECS;
using VoxEng.Core.ECS.Components;

namespace VoxEng.Core.ECS.Descriptors
{
    //A descriptor to describe an entity that contains a render mesh and a transform.
    public class RenderMeshWithTransformDescriptor: GenericEntityDescriptor<TransformEntityComponent, MeshEntityComponent> { }
}