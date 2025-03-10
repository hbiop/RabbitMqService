using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.App.Abstractions
{
    public interface IConsumer
    {
        Task<string> GetMessage(string exchangeName, string routingKey, string queueName);
    }
}
