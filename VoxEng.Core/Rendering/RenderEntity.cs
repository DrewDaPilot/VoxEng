using System.Numerics;

namespace VoxEng.Core.Rendering
{
    /// <summary>
    /// An abstract entity which can be rendered using a RenderManager.
    /// </summary>
    public abstract class RenderEntity
    {
        /// <summary>
        /// The rotation of this entity.
        /// </summary>
        public Quaternion Rotation;
        
        /// <summary>
        /// The position of this entity.
        /// </summary>
        public Vector3 Position;

        public Vector3 Scale = Vector3.One;

        public Mesh Mesh;
    }
}