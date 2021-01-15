using System;
using System.Numerics;
using Veldrid;

namespace VoxEng.Core.Rendering.Primitives
{
    public class Cube: Primitive
    {
        internal override Tuple<VertexPositionColor[], ushort[]> Definition()
        {
            ushort[] indicies;
            
            VertexPositionColor[] verticies = new[]
            {
                new VertexPositionColor(new Vector3(1f, -1f, -1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(1f, -1f, 1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(-1f, -1f, 1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(-1f, -1f, -1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(1f, 1f, -1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(1f, 1f, 1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(-1f, 1f, 1f), RgbaFloat.Red),
                new VertexPositionColor(new Vector3(-1f, 1f, -1f), RgbaFloat.Red),
            };

            indicies = new ushort[]
            {
                1, 2, 3,
                7, 6, 5,
                4, 5, 1,
                5, 6, 2,
                2, 6, 7,
                0, 3, 7,
                0, 1, 3,
                4, 7, 5,
                0, 4, 1,
                1, 5, 2,
                3, 2, 7,
                4, 0, 7
            };

            return new Tuple<VertexPositionColor[], ushort[]>(verticies, indicies);
        }
    }
}