namespace VoxEng.Core.ECS.Engines
{
    public class FreqAttribute: System.Attribute
    {
        public int Frequency;

        /// <summary>
        /// Constructs a new scheduler attribute with the specified frequency.
        /// </summary>
        /// <param name="freq">The frequency for the method to be called.</param>
        public FreqAttribute(int freq)
        {
            Frequency = freq;
        }
    }
}