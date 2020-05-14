using Hangfire;
using Hangfire.Storage;
using HangfireScheduler.DTO;
using HangfireScheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly ILogger<HangfireSchedulerController> _logger;

        public HangfireSchedulerController(IRecurringJobManager recurringJobManager, WebScraperClient webScraperClient, ILogger<HangfireSchedulerController> logger)
        {
            _recurringJobManager = recurringJobManager;
            _webScraperClient = webScraperClient;
            _logger = logger;
        }

        [HttpPost("Products")]
        public async Task<IActionResult> CreateOrUpdateProductScheduler([FromBody]ProductSchedulerDto productScheduler)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Валидация модели не успешна {ModelState}");
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Поступил запрос на добавления расписания для продукта {JsonSerializer.Serialize(productScheduler)}");

            foreach (var scheduler in productScheduler.Scheduler)
            {
                string id = $"{productScheduler.ProductId}-{scheduler}";
                _recurringJobManager.AddOrUpdate(
                    id,
                    () => _webScraperClient.PostProductPrice(productScheduler.ProductId),
                    scheduler,
                    TimeZoneInfo.Local);

                _logger.LogInformation($"Задача c id={id} успешно добавлена");
            }

            return Ok();
        }

        [HttpDelete("Products")]
        public async Task<IActionResult> DeleteProductScheduler(int productId)
        {
            _logger.LogInformation($"Поступил запрос на удаление задач для продукта productId={productId}");

            var regex = new Regex($@"^{productId}-");

            var productRecurrentJobs = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => regex.IsMatch(j.Id));

            if (!productRecurrentJobs.Any())
            {
                _logger.LogError($"Не найдено задач удовлетворяющих {regex}");
                return NotFound();
            }

            foreach (var productRecurrentJob in productRecurrentJobs)
            {
                _recurringJobManager.RemoveIfExists(productRecurrentJob.Id);

                _logger.LogInformation($"Задача c id={productRecurrentJob.Id} успешно удалена");
            }

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteScheduler(string recurringJobId)
        {
            _logger.LogInformation($"Поступил запрос на удаление задачи с id={recurringJobId}");

            var recurrentJob = JobStorage.Current.GetConnection().GetRecurringJobs().SingleOrDefault(j => j.Id == recurringJobId);

            if (recurrentJob == null)
            {
                _logger.LogError($"Не найдено задачь c id={recurringJobId}");
                return NotFound();
            }

            _recurringJobManager.RemoveIfExists(recurringJobId);

            _logger.LogInformation($"Задача c id={recurringJobId} успешно удалена");

            return Ok();
        }
    }
}
