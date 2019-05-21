using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp7
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string issue = req.Query["text"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            issue = issue ?? data?.text;

            if (issue != null)
            {
                var results = await SendSlackMessage(issue);
                return (ActionResult)new OkObjectResult($"Issue posted to Slack successfully.");
            }
            else
            {
                return new BadRequestObjectResult("Please pass a issue on the query string or in the request body");
            }
        }

        public static async Task<string> SendSlackMessage(string text)
        {
            using (var client = new HttpClient())
            {

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("text", text);

                string json = JsonConvert.SerializeObject(dictionary);

                var requestData = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(String.Format("https://hooks.slack.com/services/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"), requestData);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}
