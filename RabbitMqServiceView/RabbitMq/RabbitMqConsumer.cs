﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.models;
using RabbitMqService.RabbitMq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Infrastructure.RabbitMq
{
    public class RabbitMqConsumer : IConsumer
    {
        private readonly RmqHttpClient _httpClient;

        public RabbitMqConsumer(RmqHttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }
        public async Task<GetMessagesReturnModel> GetMessage(string queueName, int count)
        {
            var url = $"/api/queues/%2F/{Uri.EscapeDataString(queueName)}/get";
            var requestData = new
            {
                count,
                requeue = true,
                ackmode = "ack_requeue_false",
                encoding = "auto",
            };
            string json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.GetClient().PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonObject = JArray.Parse(responseContent);
            List<string> messages = new List<string>();
            foreach(var i in jsonObject)
            {
                var payload = i["payload"]?.Value<string>();
                if (payload != null)
                {
                    messages.Add(payload);
                }
            }
            return new GetMessagesReturnModel(messages);
        }
    }
}
