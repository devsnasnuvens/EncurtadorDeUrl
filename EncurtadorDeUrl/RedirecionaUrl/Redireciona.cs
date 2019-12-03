using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EncurtadorDeUrl.RedirecionaUrl
{
    public static class Redireciona
    {
        [FunctionName("Redireciona")]
        public static async Task<HttpResponseMessage> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, new string[] { "get", "post" }, Route = "Redireciona/{shortUrl}")] HttpRequestMessage req,
          [Microsoft.Azure.WebJobs.Table("shortlinks", Connection = "AzureWebJobsStorage")] CloudTable cloudTable,
          string shortUrl,
          TraceWriter log)
        {
            string empty = string.Empty;

            if (string.IsNullOrWhiteSpace(shortUrl))
                throw new Exception("O ShortLink não existe");

            string partitionKey = shortUrl.Substring(0, 1) ?? "";

            log.Info("Busca pela PartitionKey " + partitionKey + " e RowKey " + shortUrl + ".", (string)null);

            ShortUrl result = (await cloudTable.ExecuteAsync(TableOperation.Retrieve<ShortUrl>(partitionKey, shortUrl, (List<string>)null))).Result as ShortUrl;

            if (result == null)
                throw new Exception("O ShortLink não existe");

            string str = WebUtility.UrlDecode(result.Url);
            HttpResponseMessage response = req.CreateResponse(HttpStatusCode.Found);
            response.Headers.Add("Location", str);

            return response;
        }
    }
}
