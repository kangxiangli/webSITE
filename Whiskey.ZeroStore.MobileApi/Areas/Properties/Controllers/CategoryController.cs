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
    public class CategoryController : Controller
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

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            var list =_categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var result = list.Select(x=>new {
               CategoryId=x.Id,
               x.CategoryName,
               IconPath=strWebUrl+x.IconPath,
               //x.RenderingsPath,
               ParentId = x.ParentId == null ? 0 : x.ParentId
            });
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取子类数据列表
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public JsonResult GetChildrenList() 
        {
            var list = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true &&x.ParentId!=null);
            var result = list.Select(x => new
            {
                CategoryId = x.Id,
                x.CategoryName,
                IconPath = strWebUrl + x.IconPath,             
                //x.ParentId
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}