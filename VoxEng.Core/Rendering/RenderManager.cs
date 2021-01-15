using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using BlendStateDescription = Veldrid.BlendStateDescription;
using BufferDescription = Veldrid.BufferDescription;
using CommandList = Veldrid.CommandList;
using DepthStencilStateDescription = Veldrid.DepthStencilStateDescription;
using RasterizerStateDescription = Veldrid.RasterizerStateDescription;

namespace VoxEng.Core.Rendering
{

    /// <summary>
    /// The buffer container is a container which contains a vertex and index buffer for each unique object
    /// type. This container must be instanced for every entity that wishes to be rendered.
    /// </summary>
    internal struct BufferContainer
    {
        //The vertex buffer for this object.
        public DeviceBuffer VertexBuffer;
        
        //The index buffer for this object.
        public DeviceBuffer IndexBuffer;
    }
    
    internal class RenderManager: IDisposable
    {
        //The cancellation source used to cancel the draw
        private CancellationTokenSource _drawCancel;
        
        
        #region Global Buffers

        //The buffer used for the projection matrix.
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _worldBuffer;
        
        #endregion
        
        #region Graphics Resources

        private Sdl2Window _window;    
        private CommandList _cl;
        private Pipeline _pipeline;
        private GraphicsDevice _graphics;

        #endregion

        /// <summary>
        /// Creates a new RenderManager.
        /// </summary>
        public RenderManager()
        {
            Entities = new List<RenderEntity>();
            
            WindowCreateInfo ci = new()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "VoxEngine 0.01a"
            };

            GraphicsDeviceOptions opts = new()
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };

            VeldridStartup.CreateWindowAndGraphicsDevice(ci, opts, out var win, out var gd);
            _window = win;
            _graphics = gd;

            var fac = _graphics.ResourceFactory;
            InitializeResources(fac);
        }
        
        /// <summary>
        /// Initializes all graphics resources.
        /// </summary>
        /// <param name="fac"></param>
        private void InitializeResources(ResourceFactory fac)
        {
            _projectionBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _worldBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            
            string fragStr = File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\frag.glsl");
            string vertStr = File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\vert.glsl");

            byte[] fragShader = Encoding.UTF8.GetBytes(fragStr);
            byte[] vertShader = Encoding.UTF8.GetBytes(vertStr);
            
            ShaderSetDescription shaderSet = new(new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            }, fac.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertShader, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragShader, "main")));

            ResourceLayout projViewLayout = fac.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer,
                    ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldTextureLayout = fac.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer,
                        ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.UniformBuffer,
                        ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler,
                        ShaderStages.Vertex)
                    ));
            _pipeline = fac.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend, 
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default, 
                PrimitiveTopology.TriangleList, shaderSet, 
                new ResourceLayout[] { projViewLayout, worldTextureLayout},
                _graphics.MainSwapchain.Framebuffer.OutputDescription));
            

            _cl = fac.CreateCommandList();
        }


        /// <summary>
        /// Disposes of this RenderAgent by cancelling the window event pumper.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            //Cancel the window event pumper.
            _drawCancel.Cancel();
            
            _projectionBuffer.Dispose();
            _viewBuffer.Dispose();
            _worldBuffer.Dispose();
            
            _cl.Dispose();
            _pipeline.Dispose();
            _graphics.Dispose();
        }


        public List<RenderEntity> Entities;

        public int count = 0;
        
        public void Draw()
        {

            _cl.Begin();

            _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(1.0f, 
                (float) _window.Width / _window.Height, 0.5f, 100f));
                
            _cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(Vector3.UnitZ * 2.5f, Vector3.Zero, Vector3.UnitY));

            Matrix4x4 rot = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (count / 1000f)) * 
                            Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (count / 3000f));
            _cl.UpdateBuffer(_worldBuffer, 0, ref rot);
            
            _cl.SetFramebuffer(_graphics.MainSwapchain.Framebuffer);
            
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.SetPipeline(_pipeline);
            
            for (int i = 0; i < Entities.Count; i++)
            {
                if (!Entities[i].Initialized)
                {
                    Entities[i].Mesh.EntityBuffers = new BufferContainer();
                    Entities[i].Mesh.EntityBuffers.IndexBuffer = _graphics.ResourceFactory.CreateBuffer(
                        new BufferDescription(sizeof(ushort) * (uint) Entities[i].Mesh.Indicies.Length,
                            BufferUsage.IndexBuffer));
                    Entities[i].Mesh.EntityBuffers.VertexBuffer = _graphics.ResourceFactory.CreateBuffer(
                        new BufferDescription((uint) (12 * Entities[i].Mesh.Verticies.Length),
                            BufferUsage.VertexBuffer));
                    Entities[i].Initialized = true;
                }
                
                _cl.SetVertexBuffer(0, Entities[i].Mesh.EntityBuffers.VertexBuffer);
                _cl.SetIndexBuffer(Entities[i].Mesh.EntityBuffers.IndexBuffer, IndexFormat.UInt16);
                _cl.DrawIndexed((uint)Entities[i].Mesh.Indicies.Length, 1, 0,0,0);
            }
            
            _cl.End();
            _graphics.SubmitCommands(_cl);
            _graphics.SwapBuffers(_graphics.MainSwapchain);
            count++;
            _graphics.WaitForIdle();
        }
    }
}