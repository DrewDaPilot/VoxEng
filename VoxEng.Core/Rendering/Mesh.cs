using System.Numerics;

namespace VoxEng.Core.Rendering
{
    public class Mesh
    {
        public ushort[] Indicies;
        public Vector3[] Verticies;
        
        //Buffers used for this specific entity.
        internal BufferContainer EntityBuffers;
    }
}