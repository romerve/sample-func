using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace healthcheck
{
    public static class healthcheck
    {
        [FunctionName("healthcheck")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "ContosoHealthcheck", collectionName: "Submissions",
                ConnectionStringSetting = "CosmosDbConnectionString"
                )]IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            
            string userid = req.Query["userid"];
            string status = req.Query["status"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userid = userid ?? data?.userid;
            status = status ?? data?.status;

            if (!string.IsNullOrEmpty(userid))
            {
                await documentsOut.AddAsync(
                    new{
                        id = userid,
                        status = status
                    }
                );
            }

            string responseMessage = string.IsNullOrEmpty(userid)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {userid}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);

        }
    }
}
