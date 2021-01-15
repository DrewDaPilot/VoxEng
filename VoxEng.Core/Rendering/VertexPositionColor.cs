using System.Numerics;
using Veldrid;

namespace VoxEng.Core.Rendering
{
    internal struct VertexPositionColor
    {
        public Vector3 Position;
        public RgbaFloat Color;
        
        public VertexPositionColor(Vector3 pos, RgbaFloat color)
        {
            Position = pos;
            Color = color;
        }

        public const uint SizeInBytes = 28;
    }
}