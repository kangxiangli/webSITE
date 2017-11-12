using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class InitialController : Controller
    {
        //
        // GET: /Initial/
        public ActionResult Index()
        {

            return View();
        }
        public JsonResult GetList()
        {
            string basepath=AppDomain.CurrentDomain.BaseDirectory;
            string st=basepath+"Content\\TemData\\urllist.txt";
            string[] paths = null;
            if (System.IO.File.Exists(st))
            {
               paths= System.IO.File.ReadAllLines(st);
            }
            return Json(paths);
        }
	}
}