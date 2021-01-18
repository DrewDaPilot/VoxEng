using System.Numerics;
using Svelto.ECS;

namespace VoxEng.Core.ECS.Components
{
    public struct TransformEntityComponent: IEntityComponent
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
    }
}