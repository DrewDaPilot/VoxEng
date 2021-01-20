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
        public NativeDynamicArray Indicies;
        
        //An array of vertices for this mesh.
        public NativeDynamicArray Verticies;
        
        //The index in the buffer array for this mesh. Used internally by the draw loop for buffer allocations.
        internal int BufferIndex;

        public static MeshEntityComponent Cube()
        {
            var mesh = new MeshEntityComponent();
            mesh.Verticies = NativeDynamicArray.Alloc<Vector3>(Allocator.Persistent);
            mesh.Verticies.Add(new Vector3(1f, -1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, -1f, 1f));
            mesh.Verticies.Add( new Vector3(-1f, -1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, -1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, 1f, -1f));
            mesh.Verticies.Add(new Vector3(1f, 1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, 1f, 1f));
            mesh.Verticies.Add(new Vector3(-1f, 1f, -1f));

            mesh.Indicies = NativeDynamicArray.Alloc<ushort>(Allocator.Persistent);
            mesh.Indicies.Add<ushort>( 1); mesh.Indicies.Add<ushort>(2); mesh.Indicies.Add<ushort>(3);
            mesh.Indicies.Add<ushort>(7); mesh.Indicies.Add<ushort>(6); mesh.Indicies.Add<ushort>(5);
            mesh.Indicies.Add<ushort>(4); mesh.Indicies.Add<ushort>(5); mesh.Indicies.Add<ushort>(1);
            mesh.Indicies.Add<ushort>(5); mesh.Indicies.Add<ushort>(6); mesh.Indicies.Add<ushort>(2);
            mesh.Indicies.Add<ushort>(2); mesh.Indicies.Add<ushort>(6); mesh.Indicies.Add<ushort>(7);
            mesh.Indicies.Add<ushort>(0); mesh.Indicies.Add<ushort>(3); mesh.Indicies.Add<ushort>(7);
            mesh.Indicies.Add<ushort>(0); mesh.Indicies.Add<ushort>(1); mesh.Indicies.Add<ushort>(3);
            mesh.Indicies.Add<ushort>(4); mesh.Indicies.Add<ushort>(7); mesh.Indicies.Add<ushort>(5);
            mesh.Indicies.Add<ushort>(0); mesh.Indicies.Add<ushort>(4); mesh.Indicies.Add<ushort>(1);
            mesh.Indicies.Add<ushort>(1); mesh.Indicies.Add<ushort>(5); mesh.Indicies.Add<ushort>(2);
            mesh.Indicies.Add<ushort>(3); mesh.Indicies.Add<ushort>(2); mesh.Indicies.Add<ushort>(7);
            mesh.Indicies.Add<ushort>(4); mesh.Indicies.Add<ushort>(0); mesh.Indicies.Add<ushort>(7);

            return mesh;
        }

        public static MeshEntityComponent Plane()
        {
            var mesh = new MeshEntityComponent();
            mesh.Verticies =
                NativeDynamicArray.Alloc<Vector3>(Allocator.Persistent);
            mesh.Verticies.Add(new Vector3(-1, 1, 0));
            mesh.Verticies.Add(new Vector3(1, 1, 0));
            mesh.Verticies.Add(new Vector3(-1, -1, 0));
            mesh.Verticies.Add(new Vector3(1, -1, 0));
            
            mesh.Indicies = NativeDynamicArray.Alloc<ushort>(Allocator.Persistent);

            return mesh;
        }
        
    }
}