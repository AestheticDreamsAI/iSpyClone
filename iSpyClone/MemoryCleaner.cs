using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CamBase
{
    internal class MemoryCleaner
    {
        // Definiere den Schwellenwert (300 MB = 300 * 1024 * 1024 Bytes)
        private const long MemoryThreshold = 300 * 1024 * 1024;

        public static async Task StartAsync(CancellationToken cts)
        {
            Console.WriteLine("- Memory Cleaner started...");
            while (!cts.IsCancellationRequested)
            {
                Process currentProcess = Process.GetCurrentProcess();
                long usedMemory = currentProcess.WorkingSet64;
                //Console.ForegroundColor = ConsoleColor.White;
                //Console.WriteLine($"- Current memory usage: {usedMemory / 1024 / 1024} MB");

                if (CheckMemoryUsage(usedMemory))
                {
                    await FlushMemory();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"- {DateTime.Now.ToShortTimeString()}: Memory has been cleaned");
                }

                await Task.Delay(10000);
            }
            Console.WriteLine("- Memory Cleaner stopped...");
        }

        // Funktion zum Prüfen, ob der Speicherverbrauch über dem Schwellenwert liegt
        public static bool CheckMemoryUsage(long usedMemory)
        {
            // Gib true zurück, wenn der Speicherverbrauch den Schwellenwert überschreitet
            return usedMemory > MemoryThreshold;
        }

        public static async Task FlushMemory()
        {
            Process currentProcess = Process.GetCurrentProcess();
            try
            {
                // Speicherverbrauch vor dem Flush
                
                // Setze MinWorkingSet
                currentProcess.MinWorkingSet = (IntPtr)300000;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error flushing memory: {ex.Message}");
            }
        }


    }
}
