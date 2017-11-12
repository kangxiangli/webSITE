using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.WebMember
{
    public class BaseController : Controller
    {
        /// <summary>
        /// WebUrl地址，来自web.config
        /// </summary>
        protected readonly string WebUrl = ConfigurationHelper.WebUrl;
        /// <summary>
        /// ApiUrl地址，来自web.config
        /// </summary>
        protected readonly string ApiUrl = ConfigurationHelper.ApiUrl;

        protected ViewResult NoOpenView { get { return View("~/Views/Home/NotOpen.cshtml"); } }

        public BaseController()
        {

        }

    }
}