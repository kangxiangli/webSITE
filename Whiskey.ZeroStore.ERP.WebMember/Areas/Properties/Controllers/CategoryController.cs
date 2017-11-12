using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Properties.Controllers
{
    public class CategoryController : BaseController
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CategoryController));

        protected readonly ICategoryContract _categoryContract;

        public CategoryController(ICategoryContract categoryContract)
        {
            _categoryContract = categoryContract;

        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        // GET: Properties/Category
        public ActionResult Index()
        {
            return View();
        }


        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            var list = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var result = list.Select(x => new
            {
                CategoryId = x.Id,
                x.CategoryName,
                IconPath = strWebUrl + x.IconPath,
                //x.RenderingsPath,
                ParentId = x.ParentId == null ? 0 : x.ParentId
            });
            return Json(new OperationResult(OperationResultType.Success, "", result), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}