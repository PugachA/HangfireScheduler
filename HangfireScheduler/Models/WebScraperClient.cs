using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class WebScraperClient
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<WebScraperClient> _logger;

        public WebScraperClient(IConfiguration configuration, ILogger<WebScraperClient> logger)
        {
            if (configuration == null)
                throw new ArgumentNullException($"Значение {nameof(configuration)} не может быть null");

            if (logger == null)
                throw new ArgumentNullException($"Значение {nameof(logger)} не может быть null");

            _logger = logger;
            var webScraperUri = new Uri(configuration.GetValue<string>("WebScraperUri"));
            _restClient = new RestClient(webScraperUri);
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task PostProductPrice(int productId)
        {
            var request = new RestRequest($"api/ProductWatcher/price?productId={productId}");

            var response = await _restClient.ExecutePostAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError($"Не удалось отправить запрос {response.Request} {response.ErrorMessage} {response.ErrorException}");
                throw new HttpRequestException($"Не удалось отправить запрос {response.Request} {response.ErrorMessage} {response.ErrorException}");
            }

            _logger.LogInformation($"Успешно отправлен запрос {response.ResponseUri}");
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task GetProductPrice(int productId)
        {
            var request = new RestRequest($"api/ProductWatcher/price?productId={productId}");

            var response = await _restClient.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError($"Не удалось отправить запрос {response.ErrorMessage} {response.ErrorException}");
                throw new HttpRequestException($"Не удалось отправить запрос {response.ErrorMessage} {response.ErrorException}");
            }

            _logger.LogInformation($"Успешно отправлен запрос {response.ResponseUri}");
        }
    }
}
