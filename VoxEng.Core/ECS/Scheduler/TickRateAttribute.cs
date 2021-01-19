namespace VoxEng.Core.ECS.Scheduler
{
    public class TickRateAttribute: System.Attribute
    {
        
        
        /// <summary>
        /// Instances a new TickRate attribute 
        /// </summary>
        /// <param name="maxTps"></param>
        public TickRateAttribute(int maxTps = 60)
        {
            
        }
    }
}