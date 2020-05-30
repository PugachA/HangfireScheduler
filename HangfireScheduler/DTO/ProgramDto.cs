using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.DTO
{
    public class ProgramDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string StartScheduler { get; set; }
        public string StopScheduler { get; set; }
    }
}
