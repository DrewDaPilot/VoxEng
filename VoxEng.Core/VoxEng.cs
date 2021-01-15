using System.Security.Cryptography.X509Certificates;
using VoxEng.Core.Rendering;
using VoxEng.Core.Rendering.Primitives;

namespace VoxEng.Core
{
    public class VoxEng
    {
        private RenderAgent agent;

        /// <summary>
        /// Creates a new instance of the VoxEng.
        /// </summary>
        public VoxEng()
        {
            agent = new RenderAgent();
            agent.ConfigurePrimitive(new Cube());
            agent.RenderEntities.Add(new Cube());
            agent.DrawForever();

        }
    }
}