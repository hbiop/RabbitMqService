using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Domain.models
{
    public class GetMessagesReturnModel
    {
        public List<string> messages { get; set; } = new List<string>();
        public GetMessagesReturnModel(List<string> messages)
        {
            this.messages = messages;   
        }
    }
}
