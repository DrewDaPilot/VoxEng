using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using VoxEng.Core.Rendering;
using Vulkan;

namespace VoxEng.Core
{
    public class VoxEng
    {
        //The manager used for rendering objects.
        internal RenderManager _manager;

        /// <summary>
        /// Creates a new instance of the VoxEng.
        /// </summary>
        public VoxEng()
        {
            _manager = new RenderManager();
        }

        public List<RenderEntity> Entities => _manager.Entities;

        public void Draw()
        {
            while (true)
            {
                _manager.Draw();
            }
        }
        
    }
}