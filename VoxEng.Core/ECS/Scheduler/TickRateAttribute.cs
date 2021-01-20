namespace VoxEng.Core.ECS.Scheduler
{
    public class TickRateAttribute: System.Attribute
    {
        public int MaxTps;
        
        
        /// <summary>
        /// Instances a new TickRate attribute 
        /// </summary>
        /// <param name="maxTps">The maximum ticks per second for this system.</param>
        public TickRateAttribute(int maxTps = 60)
        {
            MaxTps = maxTps;
        }
    }
}