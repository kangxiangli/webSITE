using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;


namespace Whiskey.ZeroStore.MobileApi.Areas.Properties.Controllers
{
    public class SizeController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SizeController));

        protected readonly ISizeContract _sizeContract;

        protected readonly ICategoryContract _categoryContract;
        public SizeController(ISizeContract sizeContract,
            ICategoryContract categoryContract)
        {
            _categoryContract = categoryContract;
            _sizeContract = sizeContract;
            ViewBag.Size = (_sizeContract.SelectList("选择尺码").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
        }
        #endregion

        #region 获取尺码列表
        /// <summary>
        /// 获取尺码列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetList()
        {
            try
            {
                string strCategoryId = Request["CategoryId"];
                if (string.IsNullOrEmpty(strCategoryId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"),JsonRequestBehavior.AllowGet);
                }
                else
                {
                                        
                    int categoryId = int.Parse(strCategoryId);
                    IQueryable<Size> listSize = _sizeContract.GetSize(categoryId);
                    var result = listSize.Select(x => new { 
                       SizeId=x.Id,
                       x.SizeName,
                       x.IconPath,
                    });
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！",result), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);                
            }
            
        }
        #endregion
    }
}