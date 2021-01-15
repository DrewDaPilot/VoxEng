using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using VoxEng.Core.Rendering.Primitives;

namespace VoxEng.Core.Rendering
{

    internal class RenderAgent: IDisposable
    {
        private struct BufferContainer
        {
            public DeviceBuffer VertexBuffer;
            public DeviceBuffer IndexBuffer;
        }
        
        /// <summary>
        /// Resources used internally by RenderAgent for rendering.
        /// </summary>
        private class RenderResources
        {
            public CommandList CommandList;

            public Dictionary<string, BufferContainer> Buffers;
            
            public DeviceBuffer TransformBuffer;

            public RenderResources(GraphicsDevice dev)
            {
                Buffers = new Dictionary<string, BufferContainer>();
                
                int startTransformCount = 100;
                
                ResourceFactory fac = dev.ResourceFactory;
                
                TransformBuffer = fac.CreateBuffer(new BufferDescription(64 * 100, BufferUsage.UniformBuffer));
                
                CommandList = fac.CreateCommandList();
                
                GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
                pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

            }
        }

        public bool ConfigurePrimitive(Primitive p)
        {
            if (!_resources.Buffers.ContainsKey(p.GetType().Name))
            {
                var bc = new BufferContainer();
                bc.VertexBuffer =
                    _graphics.ResourceFactory.CreateBuffer(new BufferDescription((uint) p.Definition().Item1.Length * VertexPositionColor.SizeInBytes,
                        BufferUsage.VertexBuffer));
                
                bc.IndexBuffer =
                    _graphics.ResourceFactory.CreateBuffer(new BufferDescription((uint) p.Definition().Item2.Length * sizeof(ushort),
                        BufferUsage.IndexBuffer));
                
                _graphics.UpdateBuffer(bc.VertexBuffer, 0, p.Definition().Item1);
                _graphics.UpdateBuffer(bc.IndexBuffer, 0, p.Definition().Item2);
                
                _resources.Buffers.Add(p.GetType().Name, bc);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// The list of entities to render.
        /// </summary>
        public List<RenderEntity> RenderEntities;

        public static Pipeline Pipeline;

        public static Shader[] Shaders;
        
        
        /// <summary>
        /// The sdl2 window used to handle user input and resizing of
        /// the main application.
        /// </summary>
        internal Sdl2Window _window;

        /// <summary>
        /// The veldrid graphics device used
        /// </summary>
        internal GraphicsDevice _graphics;


        /// <summary>
        /// The resources this RenderAgent has available to it.
        /// </summary>
        private RenderResources _resources;

        /// <summary>
        /// The cancellation token source used when cancelling the event loop.
        /// </summary>
        private CancellationTokenSource _eventLoopCancelSource;
        

        /// <summary>
        /// The cancellation token for the event loop processor.
        /// </summary>
        private CancellationToken EventLoopCancel => _eventLoopCancelSource.Token;

        
        /// <summary>
        /// Constructs a new render manager for rendering items.
        /// </summary>
        public RenderAgent()
        {
            RenderEntities = new List<RenderEntity>();
            
            _eventLoopCancelSource = new CancellationTokenSource();
            
            WindowCreateInfo windowCi = new()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "VoxEng 0.01a",
            };
            
            GraphicsDeviceOptions opts = new ()
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };
            
            VeldridStartup.CreateWindowAndGraphicsDevice(windowCi, opts, GraphicsBackend.Direct3D11,out var _window, out var _gd);
            this._graphics = _gd;
            this._window = _window;


            _resources = new RenderResources(_graphics);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            string vertexCode = File
                .ReadAllText(
                    @"C:\Users\drewr\RiderProjects\VoxEng\VoxEng.Core\Rendering\Primitives\Shaders\vertex.glsl");
            string fragmentCode =
                File.ReadAllText(
                    @"C:\Users\drewr\RiderProjects\VoxEng\VoxEng.Core\Rendering\Primitives\Shaders\fragment.glsl");
            

            ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
            ShaderDescription fragmentShaderDesc =
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentCode), "main");

            Shaders = _graphics.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            var pipelinedesc = new GraphicsPipelineDescription();
            pipelinedesc.BlendState = BlendStateDescription.SingleOverrideBlend;
            
            pipelinedesc.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            
            pipelinedesc.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelinedesc.PrimitiveTopology = PrimitiveTopology.TriangleList;

            pipelinedesc.ResourceLayouts = System.Array.Empty<ResourceLayout>();

            pipelinedesc.ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, Shaders);
            pipelinedesc.Outputs = _graphics.SwapchainFramebuffer.OutputDescription;

            Pipeline = _graphics.ResourceFactory.CreateGraphicsPipeline(pipelinedesc);
            
        }

        public void DrawForever()
        {
            while (!EventLoopCancel.IsCancellationRequested)
            {
                _window.PumpEvents();
                Draw();
            }
        }

        public void Draw()
        {
            try
            {
                _resources.CommandList.Begin();
                _resources.CommandList.SetFramebuffer(_graphics.SwapchainFramebuffer);

                foreach (var entity in RenderEntities)
                {
                    _resources.CommandList.SetVertexBuffer(0, _resources.Buffers[entity.GetType().Name].VertexBuffer);
                    _resources.CommandList.SetIndexBuffer(_resources.Buffers[entity.GetType().Name].IndexBuffer, IndexFormat.UInt16);
                
                    _resources.CommandList.SetPipeline(Pipeline);
                
                    _resources.CommandList.DrawIndexed(36, 1, 0, 0, 0);
                    
                }
                
                _resources.CommandList.End();
                _graphics.SubmitCommands(_resources.CommandList);
                _graphics.SwapBuffers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Disposes of this RenderAgent by cancelling the window event pumper.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            //Cancel the window event pumper.
            _eventLoopCancelSource.Cancel();
        }
    }
}