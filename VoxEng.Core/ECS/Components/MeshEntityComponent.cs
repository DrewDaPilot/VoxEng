using System.Numerics;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace VoxEng.Core.ECS.Components
{
    public struct MeshEntityComponent: IEntityComponent
    {
        public NativeDynamicArrayCast<ushort> Indicies;
        
        public NativeDynamicArrayCast<Vector3> Verticies;
        
        //The index in the buffer array for this mesh.
        internal int BufferIndex;
    }
}