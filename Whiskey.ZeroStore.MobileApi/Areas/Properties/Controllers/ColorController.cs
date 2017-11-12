using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
    public class ColorController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ColorController));

        protected readonly IColorContract _colorContract;

        public ColorController(IColorContract colorContract)
        {
            _colorContract = colorContract;
        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        #region 获取颜色列表
        /// <summary>
        /// 获取颜色列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            var list = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true && !x.ColorName.Contains("其他"));
            var result = list.Select(x => new
            {
                ColorId = x.Id,
                x.ColorName,                
                IconPath=strWebUrl+x.IconPath,
            });

            return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
