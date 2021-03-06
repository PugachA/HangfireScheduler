using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class MiningClient
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<WebScraperClient> _logger;

        public MiningClient(IConfiguration configuration, ILogger<WebScraperClient> logger)
        {
            if (configuration == null)
                throw new ArgumentNullException($"Значение {nameof(configuration)} не может быть null");

            if (logger == null)
                throw new ArgumentNullException($"Значение {nameof(logger)} не может быть null");

            _logger = logger;
            var miningHunterUrl = new Uri(configuration.GetValue<string>("MiningHunterUrl"));
            _restClient = new RestClient(miningHunterUrl);
            _restClient.Timeout = 300000;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task PostEquipment(int equipmentId)
        {
            var request = new RestRequest($"api/MiningEquipmentHunter?equipmentId={equipmentId}");

            var response = await _restClient.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError($"Не удалось отправить запрос {response.Request} {response.ErrorMessage} {response.ErrorException}");
                throw new HttpRequestException($"Не удалось отправить запрос {response.Request} {response.ErrorMessage} {response.ErrorException}");
            }

            _logger.LogInformation($"Успешно отправлен запрос {response.ResponseUri}");
        }
    }
}
