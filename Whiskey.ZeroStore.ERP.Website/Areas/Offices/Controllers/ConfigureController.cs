using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class ConfigureController : Controller
    {
        #region 声明业务层操作对象
        protected readonly IConfigureContract _configureContract;

        public ConfigureController(
            IConfigureContract configureContract)
        {
            _configureContract = configureContract;
        }
        #endregion

        // GET: Offices/Configure
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetXmlNodeByXpath(string DirName, string xmlFileName, string QueryElementID, string defValue = "0")
        {
            try
            {
                //return Json(XmlStaticHelper.GetXmlNodeByXpath(DirName, xmlFileName, QueryElementID, defValue), JsonRequestBehavior.AllowGet);
                return Json(_configureContract.GetConfigureValue(DirName, xmlFileName, QueryElementID, defValue), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(defValue, JsonRequestBehavior.AllowGet);
            }
        }
    }
}