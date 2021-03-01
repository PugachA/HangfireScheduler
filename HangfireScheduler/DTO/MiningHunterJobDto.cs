using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HangfireScheduler.DTO
{
    public class MiningHunterJobDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "EquipmentId can not be less then 0")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Scheduler can not be null")]
        public string Scheduler { get; set; }
    }
}
