using Hangfire;
using Hangfire.Storage;
using HangfireScheduler.DTO;
using HangfireScheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> CreateOrUpdateProductScheduler([FromBody]ProductSchedulerDto productScheduler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var scheduler in productScheduler.Scheduler)
                _recurringJobManager.AddOrUpdate(
                    $"{productScheduler.ProductId}-{scheduler}",
                    () => _webScraperClient.PostProductPrice(productScheduler.ProductId),
                    scheduler,
                    TimeZoneInfo.Local);

            return Ok();
        }

        [HttpDelete("Products")]
        public async Task<IActionResult> DeleteProductScheduler(int productId)
        {
            var regex = new Regex($@"^{productId}-");

            var productRecurrentJobs = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => regex.IsMatch(j.Id));

            if (!productRecurrentJobs.Any())
                return NotFound();

            foreach (var productRecurrentJob in productRecurrentJobs)
                _recurringJobManager.RemoveIfExists(productRecurrentJob.Id);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteScheduler(string recurringJobId)
        {
            var recurrentJob = JobStorage.Current.GetConnection().GetRecurringJobs().SingleOrDefault(j => j.Id == recurringJobId);

            if (recurrentJob == null)
                return NotFound();

            _recurringJobManager.RemoveIfExists(recurringJobId);

            return Ok();
        }
    }
}
