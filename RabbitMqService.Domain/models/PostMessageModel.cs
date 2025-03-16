using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Domain.models
{
    public class PostMessageModel
    {
        public string QueueName { get; set; } = string.Empty;
        public object Message { get; set; } = string.Empty;
        public List<string> Modifiers { get; set; } = new List<string>();
    }
}
