using System.Numerics;
using Svelto.ECS;

namespace VoxEng.Core.ECS.Components
{
    /// <summary>
    /// A transform component representing the rotation, position and scale of an entity.
    /// </summary>
    public struct TransformEntityComponent: IEntityComponent
    {
        /// <summary>
        /// The position of this entity in world space.
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// The scale of this object, in world space.
        /// </summary>
        public Vector3 Scale;
        
        /// <summary>
        /// The rotation of this object in world space.
        /// </summary>
        public Quaternion Rotation;
    }
}