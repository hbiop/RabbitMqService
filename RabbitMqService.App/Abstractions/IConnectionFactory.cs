using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.App.Abstractions
{
    public interface IConnectionFactory
    {
        IConnection GetConnection();
        void Dispose();
    }
}
