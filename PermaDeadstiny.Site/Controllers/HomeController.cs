using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PermaDeadstiny.BLL;
using PermaDeadstiny.Site.Models;

namespace PermaDeadstiny.Site.Controllers
{
    public class HomeController : Controller
    {
        public BungieClient _BungieClient { get; set; }

        public HomeController()
        {
            _BungieClient = new BungieClient();
        }
        public ActionResult Index()
        {
             return View();
        }

        [HttpPost]
        public ActionResult Index(SearchModel model)
        {
            var results = _BungieClient.RetrieveUserDetails(model.Console == "Xbox" ? 1 : 2, model.Gamertag);
            var isPerma = _BungieClient.PermaCheck(results);

            return View("Results", isPerma);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}