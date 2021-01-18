using Svelto.ECS;

namespace VoxEng.Core.ECS
{
    public static class Groups
    {
        public abstract class RenderTag : GroupTag<RenderTag> { }

        public abstract class Dynamic : GroupTag<Dynamic> { }

        public abstract class RenderMeshWithTransform: GroupCompound<RenderTag, Dynamic> {}
    }
}