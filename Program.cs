using System;
using System.Threading.Tasks;

namespace PollingService
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Polling Service From APIs started... \n Press Enter to exit...");
            await StartApp.Run();
            Console.ReadLine();
        }
    }
}
