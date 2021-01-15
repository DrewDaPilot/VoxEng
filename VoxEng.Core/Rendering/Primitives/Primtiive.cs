using System;

namespace VoxEng.Core.Rendering.Primitives
{
    public abstract class Primitive: RenderEntity
    {
        internal abstract Tuple<VertexPositionColor[], ushort[]> Definition();
    }
}