using Microsoft.WindowsAzure.Storage.Table;

namespace EncurtadorDeUrl
{
    public class ShortUrl : TableEntity
    {
        public string Url { get; set; }
    }
}
