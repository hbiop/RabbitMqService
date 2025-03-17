using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Domain.models
{
    public class ModifiersModel
    {
        public bool durable {  get; set; }
        public bool exclusive { get; set; }
        public bool auto_delete { get; set; }
    }
}
