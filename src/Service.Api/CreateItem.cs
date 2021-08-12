using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Service.Api
{
    public static class CreateItem
    {
        [Function("CreateItem")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateItem");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string connectionString = Environment.GetEnvironmentVariable("StorageConnecionString");
            string reqString = req.ReadAsString();
            var item = JsonSerializer.Deserialize<Item>(reqString);
            var client = new QueueClient(connectionString, "items");
            client.CreateIfNotExists();
            if (client.Exists())
                client.SendMessage(reqString);


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(new
            {
                success = true,
                message = "Item enviado",
                body = item
            });

            return response;
        }
    }
}
