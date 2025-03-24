using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Domain.settings
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int MaxReconnectAttempts { get; set; } = 2;
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
    }
}
