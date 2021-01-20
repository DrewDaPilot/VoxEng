using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;
using SharpDX.Text;
using Svelto.ECS.DataStructures;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using VoxEng.Core.ECS.Components;
using Encoding = System.Text.Encoding;

namespace VoxEng.Core.Rendering
{
    internal struct EntityBuffers
    {
        //The vertex buffer for this entity.
        public DeviceBuffer VertBuff;
        
        //The index buffer for this entity.
        public DeviceBuffer IdxBuf;
    }
    
    internal class AgentConfig
    {
        //Whether or not vertical-sync is enabled.
        public bool VSync;

        //The height of the instanced window.
        public int Height = 540;
        
        //The width of the instanced window.
        public int Width = 960;

        public float Fov = 90;

        /// <summary>
        /// Creates a new agent configuration with the specified settings.
        /// </summary>
        /// <param name="height">The window height in pixels</param>
        /// <param name="width">The window width in pixels</param>
        /// <param name="vsync">Whether or not vsync should be enabled.
        /// Defaults to false.</param>
        public AgentConfig(int height, int width, bool vsync = false)
        {
            Height = height;
            Width = width;
            VSync = vsync;
        }

        public AgentConfig()
        {
            
        }
    }
    
    /// <summary>
    /// An agent responsible for window management and
    /// rendering.
    /// </summary>
    internal class RenderAgent: IDisposable
    {
        #region Global Buffers

        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _worldBuffer;
        private List<EntityBuffers> _entBuffers;
        
        #endregion
        
        //The window associated with this RenderAgent.
        public Sdl2Window Window;
        
        //The configuration used for this agent.
        private AgentConfig _config;

        //The commands list used to send commands to the graphics API.
        private CommandList _cl;

        //The primary pipeline used.
        private Pipeline _pipeline;
        
        //The graphics device used.
        private GraphicsDevice _graphics;

        //the imgui instance used for rendering UI.
        private ImGuiRenderer _imgui;

        /// <summary>
        /// Creates a new RenderAgent
        /// </summary>
        /// <param name="cfg">The configuration to use.</param>
        public RenderAgent(AgentConfig cfg = default)
        {
            _config = cfg;
            Init();
        }

        //Marked private so it is not used.
        private RenderAgent()
        {
            //Set the config to the default value.
            _config = default;
            Init();
        }

        public void Init()
        {
            _entBuffers = new List<EntityBuffers>();
            
            WindowCreateInfo wci = new()
            {
                X = 100,
                Y = 100,
                WindowWidth = _config.Width,
                WindowHeight = _config.Height,
                WindowTitle = "VoxEngine 0.01a"
            };

            GraphicsDeviceOptions graphicOpts = new();
            
            VeldridStartup.CreateWindowAndGraphicsDevice(wci, graphicOpts,
                out var win, out var gd);

            Window = win;
            _graphics = gd;

            _imgui = new ImGuiRenderer(_graphics,
                _graphics.MainSwapchain.Framebuffer.OutputDescription,
                Window.Width,
                Window.Height);
            
            Window.Resized += () =>
            {
                _imgui.WindowResized(Window.Width, Window.Height);
                _graphics.MainSwapchain.Resize((uint) Window.Width, (uint) Window.Height);
            };
            

            var fac = _graphics.ResourceFactory;
            InitResources();
        }

        private ResourceSet _projViewSet;
        private ResourceSet _worldSet;
        

        public void InitResources()
        {
            var fac = _graphics.ResourceFactory;
            
            _projectionBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _worldBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            
            //TODO: overhaul to load local assets, add async support, and load more then the two default shaders on an as-needed basis.
            byte[] fragShader = Encoding.UTF8.GetBytes(File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\frag.glsl"));
            byte[] vertShader = Encoding.UTF8.GetBytes(File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\vert.glsl"));
            
            //Create the shaders (using SPIRV if necessary)
            ShaderSetDescription shaderSetDesc = new(new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
            }, fac.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertShader, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragShader, "main")));
            
            //Describe the layout of elements in the projection view buffer.
            ResourceLayout projSetLayout = fac.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer,
                    ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            
            ResourceLayout worldSetLayout = fac.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer,
                        ShaderStages.Vertex)
                ));
            
            //Create a graphics pipeline using the layout described.
            _pipeline = fac.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend, 
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default, 
                PrimitiveTopology.TriangleList, shaderSetDesc, 
                new ResourceLayout[] { projSetLayout, worldSetLayout },
                _graphics.MainSwapchain.Framebuffer.OutputDescription));

            _projViewSet =
                _graphics.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(projSetLayout, _projectionBuffer, _viewBuffer));
            
            _worldSet =
                _graphics.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(worldSetLayout, _worldBuffer));

            //Create a new command list to use.
            _cl = fac.CreateCommandList();
            
            //Forward the configuration to the gpu.
            UpdateCfg(_config);

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        }

        /// <summary>
        /// Updates the configuration of this RenderAgent.
        /// </summary>
        /// <param name="newCfg">The new configuration to use.</param>
        public void UpdateCfg(AgentConfig newCfg)
        {
            _config = newCfg;
            
            //Begin a command.
            _cl.Begin();
            
            //Update the projection buffer to use the current window sizes and the specified planar distances.
            _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 
                (float) Window.Width / Window.Height, 0.5f, 100f));
            
            //End the command list so further commands can be executed.
            _cl.End();
            _graphics.SubmitCommands(_cl);
        }

        /// <summary>
        /// Disposes this RenderAgent by safely disposing of its components.
        /// </summary>
        public void Dispose()
        {
            _projectionBuffer.Dispose();
            _viewBuffer.Dispose();
            _worldBuffer.Dispose();
            _cl.Dispose();
            _pipeline.Dispose();
            _graphics.Dispose();
        }

        /// <summary>
        /// Sets up the API for the draw loop itself.
        /// </summary>
        public void PreDraw(Vector3 camPos)
        {
            var snapshot = Window.PumpEvents();
            _imgui.Update(1f / 60f, snapshot);
            
            if (ImGui.Begin("Debug Window"))
            {
                ImGui.Text($"Application average ms/frame: {1000.0f/ ImGui.GetIO().Framerate}");
                ImGui.Text($"Application average fps : {ImGui.GetIO().Framerate}");
                ImGui.End();
            }

            //Instruct the graphics API that we wish to begin submitting commands.
            _cl.Begin();
            
            _cl.SetFramebuffer(_graphics.SwapchainFramebuffer);

            _cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(camPos, Vector3.Zero, Vector3.UnitY));

            _cl.SetPipeline(_pipeline);

            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _worldSet);
            
            _cl.ClearColorTarget(0, RgbaFloat.White);
            
            
            
            //This code was deemed unnecessary per-frame since the perspective never changes at runtime. (Unless a change of FOV is made.
            _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 
                (float) Window.Width / Window.Height, 0.5f, 100f));
            
            //This code was not functional because count was not being incremented. 
            /*
                In practice, the following logic would allow the camera to have a rotational axis, but I am not certain its necessary.
             
            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY) * 
                            Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (count / 3000f));
            _cl.UpdateBuffer(_worldBuffer, 0, ref rot);
            */
            
            //Swap the framebuffer. Not entirely sure what this does (yet). This might be needed for v-sync.
            
            _cl.SetFramebuffer(_graphics.MainSwapchain.Framebuffer);
        }

        /// <summary>
        /// Call this with the required components to draw it.
        /// </summary>
        public void DrawEntity(TransformEntityComponent trans, MeshEntityComponent mesh)
        {
            _cl.SetVertexBuffer(0, _entBuffers[(int) mesh.BufferIndex].VertBuff);
            _cl.SetIndexBuffer(_entBuffers[mesh.BufferIndex].IdxBuf, IndexFormat.UInt16);
            _cl.UpdateBuffer(_worldBuffer, 0, Matrix4x4.CreateTranslation(trans.Position)  * Matrix4x4.CreateFromQuaternion(trans.Rotation) * Matrix4x4.CreateScale(trans.Scale));
            _cl.DrawIndexed((uint) mesh.Indicies.Count<ushort>(), 1, 0,0,0);
        }

        /// <summary>
        /// Called after drawing is ocmplete.
        /// </summary>
        public void PostDraw()
        {
            _imgui.Render(_graphics, _cl);
            _cl.End();
            _graphics.SubmitCommands(_cl);
            _graphics.SwapBuffers(_graphics.MainSwapchain);
        }

        /// <summary>
        /// Allocates a new vert/index buffer for rendering.
        /// </summary>
        /// <returns>The index of the buffer.</returns>
        public int AllocBuffer(MeshEntityComponent init)
        {

            var idx = _entBuffers.Count;

            var buff = new EntityBuffers();
            buff.VertBuff = _graphics.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint) init.Verticies.Count<Vector3>() * 12,
                BufferUsage.VertexBuffer));
            if(init.Indicies.Count<ushort>() > 0)
                buff.IdxBuf = _graphics.ResourceFactory.CreateBuffer(new BufferDescription(
                sizeof(ushort) * (uint) init.Indicies.Count<ushort>(),
                BufferUsage.IndexBuffer));
            
            _entBuffers.Add(buff);
            
            
            //TODO: when svelto supports it, attempt to use the native pointer directly so there is no overhead from a copy.
            
            _graphics.UpdateBuffer(_entBuffers[idx].VertBuff, 0, init.Verticies.ToIntPTR<Vector3>(), (uint) init.Verticies.Count<Vector3>() * 12);
            _graphics.UpdateBuffer(_entBuffers[idx].IdxBuf, 0, init.Indicies.ToIntPTR<ushort>(), (uint) init.Indicies.Count<ushort>() * sizeof(ushort));

            return idx;
        }
    }
}