using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class Test
    {
        public void StopProgram(string name)
        {
            Process[] workers = Process.GetProcessesByName(name);

            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }
        }
    }
}
