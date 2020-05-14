using Hangfire;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class WebScraperClient
    {
        private readonly IRestClient _restClient;
        //private readonly ILogger _logger;

        public WebScraperClient(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException($"Значение {nameof(configuration)} не может быть null");

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
                //_logger.LogError($"Не удалось получить информацию от MvcPaySystemAdmins {response.ErrorMessage} {response.ErrorException}");
                throw new HttpRequestException($"Не удалось отправить запрос {response.ErrorMessage} {response.ErrorException}");
            }
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task GetProductPrice(int productId)
        {
            var request = new RestRequest($"api/ProductWatcher/price?productId={productId}");

            var response = await _restClient.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
            {
                //_logger.LogError($"Не удалось получить информацию от MvcPaySystemAdmins {response.ErrorMessage} {response.ErrorException}");
                throw new HttpRequestException($"Не удалось отправить запрос {response.ErrorMessage} {response.ErrorException}");
            }
        }
    }
}
