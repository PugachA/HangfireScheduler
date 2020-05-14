using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.DTO
{
    public class ProductSchedulerDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "ProductId can not be less then 0")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Scheduler can not be null")]
        public List<string> Scheduler { get; set; }

        //TODO добавить валидация cron https://docs.microsoft.com/ru-ru/aspnet/core/mvc/models/validation?view=aspnetcore-3.1

    }
}
