using System;
using System.Threading.Tasks;
using VoxEng.Core.Rendering.Primitives;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            VoxEng.Core.VoxEng eng = new VoxEng.Core.VoxEng();
            eng.Entities.Add(new Cube());
            while (true)
            {
                eng.Draw();
            }
            //MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        static async Task MainAsync(string[] args)
        {

        }
    }
}