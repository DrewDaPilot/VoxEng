using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using VoxEng.Core;
using Vulkan.Xlib;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
        
        static async Task MainAsync(string[] args)
        {
            var e = new Eng();
            await e.Execute();
        }
    }
}