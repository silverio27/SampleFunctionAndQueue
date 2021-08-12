using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues.Models;
using System.IO;

namespace Consumer.App
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public Worker(ILogger<Worker> logger,
        IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string connectionString = _configuration["Services:QueueConnection"];
            string queueName = _configuration["Services:QueueName"];
            string output = _configuration["Services:Output"];
            var client = new QueueClient(connectionString, queueName);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                QueueMessage[] messages = client.ReceiveMessages();
                if (messages.Length > 0)
                {
                    var message = messages[0];
                    if (!Directory.Exists(output))
                        Directory.CreateDirectory(output);

                    string fileName = $"{output}/{message.MessageId}.json";
                    File.WriteAllText(fileName, message.Body.ToString());
                    _logger.LogInformation($"Mensagem enviada para o local: {fileName}");
                    client.DeleteMessage(message.MessageId,message.PopReceipt);
                }
                

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task<string> GetMessage(QueueClient client)
        {
            if (!client.Exists())
            {
                return await Task.FromResult($"Não existe uma fila com esse nome: {client.Name}");
            }
            else
            {
                QueueMessage[] messages = await client.ReceiveMessagesAsync();
                var message = messages[0].Body;
                return message.ToString();
            }
        }
    }
}
