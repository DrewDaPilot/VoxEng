using FixedMaths;

namespace VoxEng.Core.ECS.Engines
{
    public interface IScheduledEngine
    {
        public abstract void Run();

        public string Name { get; }
    }
}