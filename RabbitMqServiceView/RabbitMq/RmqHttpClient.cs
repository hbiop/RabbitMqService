using Microsoft.Extensions.Options;
using RabbitMqService.Domain.settings;
using RabbitMqService.settings;
using System.Net.Http.Headers;
using System.Text;

namespace RabbitMqService.RabbitMq
{
    public class RmqHttpClient
    {
        HttpClient _httpClient;
        public RmqHttpClient(HttpClient httpClient, IOptions<RabbitMqApiSettings> settings)
        {
            _httpClient = httpClient;
            var _settings = settings.Value;
            httpClient.BaseAddress = new Uri(_settings.ConnectionString);
            string _basicAuthHeader = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.UserName}:{_settings.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", _basicAuthHeader);
        }

        public HttpClient GetClient() 
        { 
            return _httpClient;
        }
    }
}
