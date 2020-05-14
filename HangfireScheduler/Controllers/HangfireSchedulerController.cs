using Hangfire;
using HangfireScheduler.DTO;
using HangfireScheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangfireSchedulerController : ControllerBase
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly WebScraperClient _webScraperClient;

        public HangfireSchedulerController(IRecurringJobManager recurringJobManager, WebScraperClient webScraperClient)
        {
            _recurringJobManager = recurringJobManager;
            _webScraperClient = webScraperClient;
        }

        [HttpPost("Products")]
        public async Task<IActionResult> CreateProductScheduler([FromBody]ProductSchedulerDto productScheduler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var scheduler in productScheduler.Scheduler)
                _recurringJobManager.AddOrUpdate(
                    $"{productScheduler.ProductId}-{scheduler}",
                    () => _webScraperClient.GetProductPrice(productScheduler.ProductId),
                    scheduler,
                    TimeZoneInfo.Local);

            return Ok();

            //return CreatedAtAction(
            //nameof(GetTodoItem),
            //new { id = todoItem.Id },
            //ItemToDTO(todoItem));
        }


    }
}
