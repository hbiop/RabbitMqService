using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Exceptions;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.models;
using System.Text.Json;


namespace RabbitMqServiceView.Controllers
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class MessageBrokerController(ILogger<MessageBrokerController> logger,IConsumer consumer, IProducer producer) : Controller
    {
        private readonly ILogger<MessageBrokerController> _logger = logger;
        private readonly IProducer _producer = producer;
        private readonly IConsumer _consumer = consumer;

        [HttpPost]
        public IActionResult SendMessage([FromBody] PostMessageModel request)
        {
            try
            {
                string result = _producer.SendMessage(request).Result;
                _logger.LogInformation("Сообщение было отправлено");
                return Ok(result);
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogCritical($"Ошибка: Брокер недоступен. {ex.Message}");
                return BadRequest($"Ошибка: Брокер недоступен. {ex.Message}");
            }
            catch (AuthenticationFailureException ex)
            {
                _logger.LogCritical($"Ошибка аутентификации: {ex.Message}");
                return BadRequest($"Ошибка аутентификации: {ex.Message}");
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogCritical($"Операция прервана: {ex.Message}");
                return BadRequest($"Операция прервана: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Ошибка при отправке сообщения: {ex.Message}");
                return BadRequest($"Ошибка при отправке сообщения: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult GetMessage([FromBody] GetMessagesModel getMessages,string queueName = "default", int count=1)
        {
            try
            {
                var result = _consumer.GetMessage(queueName, getMessages.Login, getMessages.Password, count).Result;
                _logger.LogInformation("Сообщение было отправлено");
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogCritical($"Ошибка HTTP: {ex.Message}");
                return BadRequest(new GetMessagesReturnModel(new List<string> { $"Ошибка HTTP: {ex.Message}" }));
            }
            catch (JsonException ex)
            {
                _logger.LogCritical($"Ошибка JSON: {ex.Message}");
                return BadRequest(new GetMessagesReturnModel(new List<string> { $"Ошибка JSON: {ex.Message}" }));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Неизвестная ошибка: {ex.Message}");
                return BadRequest(new GetMessagesReturnModel(new List<string> { $"Неизвестная ошибка: {ex.Message}" }));
            }
        }
    }
}
