using System.Numerics;

namespace VoxEng.Core.Rendering.Primitives
{
    public class Cube: RenderEntity
    {
        public Cube()
        {
            base.Mesh = new Mesh();
            base.Mesh.Verticies = new[]
            {
                new Vector3(1f, -1f, -1f),
                new Vector3(1f, -1f, 1f),
                new Vector3(-1f, -1f, 1f),
                new Vector3(-1f, -1f, -1f),
                new Vector3(1f, 1f, -1f),
                new Vector3(1f, 1f, 1f),
                new Vector3(-1f, 1f, 1f),
                new Vector3(-1f, 1f, -1f)
            };
            base.Mesh.Indicies = new ushort[]
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
        }
    }
}