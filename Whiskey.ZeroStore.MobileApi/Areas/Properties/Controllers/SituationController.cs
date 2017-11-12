using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Properties.Controllers
{
    [License(CheckMode.Verify)]
    public class SituationController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SituationController));

        protected readonly IProductAttributeContract _situationContract;

        public SituationController(IProductAttributeContract situationContract)
        {
            _situationContract = situationContract;

        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            var list = _situationContract.ProductAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true &&x.ParentId==2);
            var result = list.Select(x=>new {
               SituationId = x.Id,
               SituationName=x.AttributeName,
               IconPath = strWebUrl + x.IconPath,                              
            });            
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}