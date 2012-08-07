using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Web.Models;

namespace Scrabble.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Setup(ICollection<PlayerModel> players)
        {
            return View();
        }
    }
}
