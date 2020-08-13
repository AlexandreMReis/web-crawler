using Microsoft.Extensions.Configuration;
using System;

namespace web_crawler
{
    public class Crawler : IDisposable
    {
        private readonly IConfiguration _configuration;

        private readonly string _baseURL;
        public Crawler(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseURL = configuration["IdealistaURL"];
        }

        private bool GetAnnouncements()
        {
            var url = $"{_baseURL}/com-preco-max_150000,tamanho-min_100000/";

            var anouncements = ApiRequest.Get<object>(url);

            return true;
        }

        public void Start()
        {

            this.GetAnnouncements();

        }

        public void Dispose()
        {
            
        }
    }
}
