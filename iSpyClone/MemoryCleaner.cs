using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamBase
{
    internal class MemoryCleaner
    {
        public static async Task StartAsync(CancellationToken cts)
        {
            Console.WriteLine("- Memory Cleaner started...");
            while (!cts.IsCancellationRequested)
            {
                FlushMemory();
                await Task.Delay(10000);
            }
            Console.WriteLine("- Memory Cleaner stopped...");
        }
        public static void FlushMemory()
        {
            Process currentProcess = Process.GetCurrentProcess();
            try
            {
                currentProcess.MinWorkingSet = (IntPtr)300000;
            }
            catch (Exception)
            {
            }
        }
    }
}
