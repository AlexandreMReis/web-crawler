using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;


namespace web_crawler
{
    public class Program
    {
        static void Main(string[] args)
        {
            IHostingEnvironment env;

            var a = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);



            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            using (Crawler crawler = new Crawler(configuration))
            {
                crawler.Start();
            }
        }
    }
}
