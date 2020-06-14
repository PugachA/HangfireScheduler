using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.DTO
{
    public class ProgramDto
    {
        [Required(ErrorMessage = "Name can not be null")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Path can not be null")]
        public string Path { get; set; }

        [Required(ErrorMessage = "UserName can not be null")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "StartScheduler can not be null")]
        public string StartScheduler { get; set; }

        [Required(ErrorMessage = "StopScheduler can not be null")]
        public string StopScheduler { get; set; }
    }
}
