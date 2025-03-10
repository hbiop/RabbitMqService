using Microsoft.AspNetCore.Mvc;
using RabbitMqService.App.Abstractions;
using Serilog;

namespace RabbitMqServiceView.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MessageBrokerController(IConsumer consumer, IProducer producer) : Controller
    {
        private readonly IProducer _producer = producer;
        private readonly IConsumer _consumer = consumer;

        [HttpPost]
        public IActionResult SendMessage(string message, string exchangeName = "Default Exchange", string routingKey = "Default Routing Key", string queueName = "Default Queue")
        {
            string result = _producer.SendMessage(message, exchangeName, routingKey, queueName).Result;
            Log.Information(result);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetMessage(string exchangeName = "Default Exchange", string routingKey = "Default Routing Key", string queueName = "Default Queue")
        {
            var result = _consumer.GetMessage(exchangeName, routingKey, queueName).Result;
            Log.Information(result);
            return Ok(result);
        }
    }
}
