using System.Numerics;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;
using VoxEng.Core.Rendering;

namespace VoxEng.Core.ECS.Components
{
    /// <summary>
    /// A component representing the data required to render the mesh (triangles and vertices).
    /// </summary>
    public struct MeshEntityComponent: IEntityComponent
    {
        //An array of indices for this mesh. 
        public NativeDynamicArrayCast<ushort> Indicies;
        
        //An array of vertices for this mesh.
        public NativeDynamicArrayCast<Vector3> Verticies;
        
        //The index in the buffer array for this mesh. Used internally by the draw loop for buffer allocations.
        internal int BufferIndex;

        public static MeshEntityComponent Cube()
        {
            var mesh = new MeshEntityComponent();
            mesh.Verticies = new NativeDynamicArrayCast<Vector3>(NativeDynamicArray.Alloc<Vector3>(Allocator.Persistent));
            mesh.Verticies.Add(new Vector3(1f, -1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, -1f, 1f));
            mesh.Verticies.Add( new Vector3(-1f, -1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, -1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, 1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, 1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, 1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, 1f, -1f));

            mesh.Indicies = new NativeDynamicArrayCast<ushort>(NativeDynamicArray.Alloc<ushort>(Allocator.Persistent));
            mesh.Indicies.Add(1); mesh.Indicies.Add(2); mesh.Indicies.Add(3);
            mesh.Indicies.Add(7); mesh.Indicies.Add(6); mesh.Indicies.Add(5);
            mesh.Indicies.Add(4); mesh.Indicies.Add(5); mesh.Indicies.Add(1);
            mesh.Indicies.Add(5); mesh.Indicies.Add(6); mesh.Indicies.Add(2);
            mesh.Indicies.Add(2); mesh.Indicies.Add(6); mesh.Indicies.Add(7);
            mesh.Indicies.Add(0); mesh.Indicies.Add(3); mesh.Indicies.Add(7);
            mesh.Indicies.Add(0); mesh.Indicies.Add(1); mesh.Indicies.Add(3);
            mesh.Indicies.Add(4); mesh.Indicies.Add(7); mesh.Indicies.Add(5);
            mesh.Indicies.Add(0); mesh.Indicies.Add(4); mesh.Indicies.Add(1);
            mesh.Indicies.Add(1); mesh.Indicies.Add(5); mesh.Indicies.Add(2);
            mesh.Indicies.Add(3); mesh.Indicies.Add(2); mesh.Indicies.Add(7);
            mesh.Indicies.Add(4); mesh.Indicies.Add(0); mesh.Indicies.Add(7);

            return mesh;
        }

        public static MeshEntityComponent Plane()
        {
            var mesh = new MeshEntityComponent();
            mesh.Verticies =
                new NativeDynamicArrayCast<Vector3>(NativeDynamicArray.Alloc<Vector3>(Allocator.Persistent));
            mesh.Verticies.Add(new Vector3(-1, 1, 0));
            mesh.Verticies.Add(new Vector3(1, 1, 0));
            mesh.Verticies.Add(new Vector3(-1, -1, 0));
            mesh.Verticies.Add(new Vector3(1, -1, 0));
            
            mesh.Indicies = new NativeDynamicArrayCast<ushort>(NativeDynamicArray.Alloc<ushort>(Allocator.Persistent));

            return mesh;
        }
        
    }
}