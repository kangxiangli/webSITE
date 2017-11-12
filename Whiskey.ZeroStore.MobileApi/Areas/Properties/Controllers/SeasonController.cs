using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;


namespace Whiskey.ZeroStore.MobileApi.Areas.Properties.Controllers
{
    [License(CheckMode.Verify)]
    public class SeasonController : Controller
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SeasonController));

        protected readonly ISeasonContract _seasonContract;

        public SeasonController(ISeasonContract seasonContract)
        {
            _seasonContract = seasonContract;
            ViewBag.Season = (_seasonContract.SelectList("选择季节").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 获取季节列表
        /// <summary>
        /// 获取季节列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {                        
            IQueryable<Season> listSeason = _seasonContract.Seasons.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var result = listSeason.Select(x => new {
              SeasonId=x.Id,
              x.SeasonName,
              IconPath = strWebUrl + x.IconPath,
            });
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}