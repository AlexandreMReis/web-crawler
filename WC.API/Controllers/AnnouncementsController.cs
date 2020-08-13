using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WC.API.Controllers
{
    [Route("v1/Announcements")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly CrawlerLogic _crawlerLogic = new CrawlerLogic();

        // GET api/values
        [HttpGet]
        public ActionResult<List<string>> GetAnnouncements()
        {
            var result = _crawlerLogic.RunCrawler();

            return result;
        }
    }
}
