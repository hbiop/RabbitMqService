using RabbitMqService.Domain.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.App.Abstractions
{
    public interface IConsumer
    {
        Task<GetMessagesReturnModel> GetMessage(string queueName, string login, string password, int count);
    }
}
