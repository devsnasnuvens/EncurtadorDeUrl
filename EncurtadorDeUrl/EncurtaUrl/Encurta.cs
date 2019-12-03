using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EncurtadorDeUrl.EncurtaUrl
{
    public static class Encurta
    {
        public static readonly string SHORTENER_URL = Environment.GetEnvironmentVariable(nameof(SHORTENER_URL));

        [FunctionName("Encurta")]
        public static async Task<HttpResponseMessage> Run(
          [HttpTrigger(AuthorizationLevel.Function, new string[] { "get", "post" }, Route = null)] HttpRequestMessage req,
          [Microsoft.Azure.WebJobs.Table("shortlinks", Connection = "AzureWebJobsStorage")] CloudTable cloudTable,
          TraceWriter log)
        {
            if (req == null)
                return req.CreateResponse(HttpStatusCode.NotFound);

            Request request = await req.Content.ReadAsAsync<Request>();

            if (request == null)
                return req.CreateResponse(HttpStatusCode.NotFound);

            string url = request.Url;
            string shortLinkName = request.ShortLinkName;

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("É necessário uma URL para encurtar");

            if (string.IsNullOrWhiteSpace(shortLinkName))
                throw new Exception("O nome do short link é obrigatório");

            log.Info("Encurtar a URL " + url + " para " + Encurta.SHORTENER_URL + shortLinkName, (string)null);

            ShortUrl shortUrl = new ShortUrl();
            shortUrl.PartitionKey = shortLinkName.Substring(0, 1);
            shortUrl.RowKey = shortLinkName;
            shortUrl.Url = url;

            ShortUrl newUrl = shortUrl;
            TableResult tableResult = await cloudTable.ExecuteAsync(TableOperation.Insert((ITableEntity)newUrl));

            return req.CreateResponse<Response>(HttpStatusCode.OK, new Response()
            {
                ShortUrl = Encurta.SHORTENER_URL + newUrl.RowKey,
                Url = WebUtility.UrlDecode(newUrl.Url)
            });
        }
    }
}
