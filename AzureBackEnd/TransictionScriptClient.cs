using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace AzureBackEnd
{
    public static class Ping
    {
        [FunctionName("Ping")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PING - C# HTTP trigger function processed a request.");

            return new OkObjectResult("OK");
        }
    }


    public static class PostClientLog
    {
        [FunctionName("PostClientLog")]
        public static async Task<IActionResult> Run(
                   [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
                   [CosmosDB(
                            databaseName: "ClientsLogs",
                            containerName: "ClientsLogs",
                            Connection = "_secret:CosmosDBConnectionStringSetting")]IAsyncCollector<dynamic> documentsOut, 
                   ILogger log)
        {
                        
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

                       
            
            try
            {
                await documentsOut.AddAsync(requestBody);
                return new OkObjectResult("OK");
            }
            catch (Exception ex) {
                return new OkObjectResult(ex);
            }

        }
    }

   
}
