using System;
using System.Threading.Tasks;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        static async Task MainAsync(string[] args)
        {
            VoxEng.Core.VoxEng eng = new VoxEng.Core.VoxEng();
            await Task.Delay(-1);
        }
    }
}