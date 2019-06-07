using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Syslog.Shared.Model;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Syslog.API
{
    public static class Function1
    {
        [FunctionName("GetLastEntries")]
        public static async Task<HttpResponseMessage> GetLastEntries([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, CloudTable table,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            TableQuery<Message> query = new TableQuery<Message>();
            TableContinuationToken continuationToken = null;

            List<Message> messages = new List<Message>();

            do
            {
                var page = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = page.ContinuationToken;
                messages.AddRange(page.Results);
            }
            while (continuationToken != null);

            return req.CreateResponse(HttpStatusCode.OK, messages);
        }
    }
}
