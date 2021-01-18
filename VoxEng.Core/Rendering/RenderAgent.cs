﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SharpDX.Text;
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
        
        /// <summary>
        /// Creates a new RenderAgent
        /// </summary>
        /// <param name="cfg">The configuration to use.</param>
        public RenderAgent(AgentConfig cfg = default)
        {
            _config = cfg;
            _entBuffers = new List<EntityBuffers>();
            Init();
        }

        //Marked private so it is not used.
        private RenderAgent()
        {
            //Set the config to the default value.
            _config = default;
            _entBuffers = new List<EntityBuffers>();
            Init();
        }

        public void Init()
        {
            WindowCreateInfo wci = new()
            {
                X = 0,
                Y = 0,
                WindowWidth = _config.Width,
                WindowHeight = _config.Height,
                WindowTitle = "VoxEngine 0.01a"
            };

            GraphicsDeviceOptions graphicOpts = new();
            
            VeldridStartup.CreateWindowAndGraphicsDevice(wci, graphicOpts,
                out var win, out var gd);

            Window = win;
            _graphics = gd;

            var fac = _graphics.ResourceFactory;
            
        }

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

            //Create a new command list to use.
            _cl = fac.CreateCommandList();
            
            //Forward the configuration to the gpu.
            UpdateCfg(_config);
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
        public void PreDraw()
        {
            //Instruct the graphics API that we wish to begin submitting commands.
            _cl.Begin();
            
            //This code was deemed unnecessary per-frame since the perspective never changes at runtime.
            /*
             _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 
                (float) Window.Width / Window.Height, 0.5f, 100f));
            */
            
            //Technically useless, this would update the camera each frame if it was being moved.
            _cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(Vector3.UnitZ * 2.5f, Vector3.Zero, Vector3.UnitY));

            //This code was not functional because count was not being incremented. 
            /*
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
            _cl.SetIndexBuffer(_entBuffers[(int) mesh.BufferIndex].IdxBuf, IndexFormat.UInt16);
            _cl.DrawIndexed((uint) mesh.Indicies.count, 1, 0,0,0);
        }

        /// <summary>
        /// Called after drawing is ocmplete.
        /// </summary>
        public void PostDraw()
        {
            _cl.End();
            _graphics.SubmitCommands(_cl);
            _graphics.SwapBuffers(_graphics.MainSwapchain);
        }
    }
}