using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.App.Abstractions
{
    public interface IProducer
    {
        Task<string> SendMessage(string message, string exchangeName, string routingKey, string queueName);
    }
}
