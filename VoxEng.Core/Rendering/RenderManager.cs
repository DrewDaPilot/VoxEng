using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using SharpDX.DXGI;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using VoxEng.Core.ECS.Components;
using Vulkan.Xlib;

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
        private List<BufferContainer> _buffers;
        
        //The cancellation source used to cancel the draw
        private CancellationTokenSource _drawCancel;

        private Stopwatch _sw;

        #region Global Buffers

        //The buffer used for the projection matrix.
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _worldBuffer;

        private ResourceSet _projViewSet;
        private ResourceSet _worldSet;
        
        #endregion
        
        #region Graphics Resources

        public Sdl2Window _window;
        private CommandList _cl;
        private Pipeline _pipeline;
        private GraphicsDevice _graphics;

        private Shader[] _shaders;

        #endregion
        
        /// <summary>
        /// All of the entities that this RenderManager is responsible for rendering.
        /// </summary>
        private List<RenderEntity> _entities;

        /// <summary>
        /// Creates a new RenderManager.
        /// </summary>
        public RenderManager()
        {
            _buffers = new List<BufferContainer>();
            
            _drawCancel = new CancellationTokenSource();
            _sw = new Stopwatch();
            
            _entities = new List<RenderEntity>();
            
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

            VeldridStartup.CreateWindowAndGraphicsDevice(ci, opts, GraphicsBackend.Direct3D11,out var win, out var gd);
            _window = win;
            _graphics = gd;

            _window.Closed += Dispose;

            var fac = _graphics.ResourceFactory;
            InitializeResources(fac);
        }
        
        /// <summary>
        /// Initializes all graphics resources.
        /// </summary>
        /// <param name="fac"></param>
        private void InitializeResources(ResourceFactory fac)
        {

            ResourceLayout projViewLayout = fac.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout worldLayout = fac.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                ));
            
            _projectionBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _viewBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _worldBuffer = fac.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            //Removed the color from the vertex structure.
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
            
            string fragStr = File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\frag.glsl");
            string vertStr = File.ReadAllText(@"C:\Users\drewr\Documents\GitHub\VoxEng\VoxEng.Core\Rendering\Shaders\vert.glsl");

            _cl = fac.CreateCommandList();

            
            
            ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertStr), "main");
            ShaderDescription fragmentShaderDesc =
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragStr), "main");

            _shaders = fac.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            var shaderSet = new ShaderSetDescription(new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position,
                        VertexElementFormat.Float3))
            }, _shaders);
            
            var pipelineDesc = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projViewLayout, worldLayout },
                _graphics.MainSwapchain.Framebuffer.OutputDescription);
            
            pipelineDesc.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDesc.PrimitiveTopology = PrimitiveTopology.TriangleList;
            
            pipelineDesc.ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, _shaders);
            pipelineDesc.Outputs = _graphics.SwapchainFramebuffer.OutputDescription;

            _pipeline = _graphics.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);

            _projViewSet =
                fac.CreateResourceSet(new ResourceSetDescription(projViewLayout, _projectionBuffer, _viewBuffer));
            _worldSet = fac.CreateResourceSet(new ResourceSetDescription(worldLayout, _worldBuffer));

            _graphics.SyncToVerticalBlank = false;
        }


        /// <summary>
        /// Disposes of this RenderAgent by cancelling the window event pumper.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            //Cancel the window event pumper.
            _drawCancel.Cancel();
            
            /*
            _projectionBuffer.Dispose();
            _viewBuffer.Dispose();
            _worldBuffer.Dispose();
            */
            
            _cl.Dispose();
            _pipeline.Dispose();
            _graphics.Dispose();
        }


        public void PreDraw()
        {
            _window.PumpEvents();
            _cl.Begin();
            
            _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreatePerspectiveFieldOfView(
                (float) Math.PI / 2,
                (float) _window.Width / _window.Height,
                0.5f,
                1000f));
            
            /*
            _cl.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreateOrthographic(
                20 , 
                20,
                0.5f,
                100f));
            */
            
            _cl.SetPipeline(_pipeline);
            
            _cl.SetViewport(0, new Viewport(0f, 0f, _window.Width, _window.Height, 0f, 1f));
            
            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _worldSet);
            
            _cl.UpdateBuffer(_viewBuffer, 0, Matrix4x4.CreateLookAt(new Vector3(0, 0, -5.0f), Vector3.Zero, Vector3.UnitY));
            
            _cl.SetFramebuffer(_graphics.SwapchainFramebuffer);
            _cl.SetPipeline(_pipeline);
            _cl.ClearColorTarget(0, RgbaFloat.White);
        }

        public void EndDraw()
        {
            _cl.End();
            _graphics.SubmitCommands(_cl);
            _graphics.SwapBuffers();
        }

        /// <summary>
        /// Call infinitely to run the draw loop.
        /// </summary>
        public void DrawEntity(MeshEntityComponent mesh, TransformEntityComponent trans)
        {
            _cl.SetVertexBuffer(0, _buffers[(int) mesh.BufferIndex].VertexBuffer);
            _cl.SetIndexBuffer(_buffers[(int) mesh.BufferIndex].IndexBuffer, IndexFormat.UInt16);
            _cl.UpdateBuffer(_worldBuffer, 0, Matrix4x4.CreateTranslation(trans.Position)  * Matrix4x4.CreateFromQuaternion(trans.Rotation) * Matrix4x4.CreateScale(trans.Scale));
            _cl.DrawIndexed((uint) mesh.Indicies.count, 1, 0, 0, 0);

        }

        /// <summary>
        /// Registers the select entity for rendering, allowing it to be rendered on the next frame.
        /// </summary>
        /// <param name="ent">TThe entity to register</param>
        /// <returns>Whether the registration was successful.</returns>
        public void RegisterEntity(RenderEntity ent)
        {
            if (ent.Mesh.RenderBuffers.VertexBuffer == null)
                ent.Mesh.RenderBuffers.VertexBuffer = _graphics.ResourceFactory.CreateBuffer(
                    new BufferDescription((uint) ent.Mesh.Verticies.Length * 12,
                        BufferUsage.VertexBuffer));
            
            if (ent.Mesh.RenderBuffers.IndexBuffer == null)
                ent.Mesh.RenderBuffers.IndexBuffer = _graphics.ResourceFactory.CreateBuffer(
                    new BufferDescription((uint) ent.Mesh.Indicies.Length * sizeof(ushort),
                        BufferUsage.IndexBuffer));
            
            _entities.Add(ent);
            
            _graphics.UpdateBuffer(ent.Mesh.RenderBuffers.VertexBuffer, 0, ent.Mesh.Verticies);
            _graphics.UpdateBuffer(ent.Mesh.RenderBuffers.IndexBuffer, 0, ent.Mesh.Indicies);
        }
    }
}