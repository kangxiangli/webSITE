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
    public class FigureController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SituationController));

        protected readonly IProductAttributeContract _figureController;

        public FigureController(IProductAttributeContract situationContract)
        {
            _figureController = situationContract;

        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        #region 获取数据列表
        /// <summary>
        /// 获取体形信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            try
            {
                const string attrName = "体型";
                var figureParentEntity = _figureController.ProductAttributes.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.AttributeName == attrName);
                if (figureParentEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "未找到体形信息"), JsonRequestBehavior.AllowGet);
                }
                var list = _figureController.ProductAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == figureParentEntity.Id);
                var result = list.Select(x => new
                {
                    FigureId = x.Id,
                    FigureName = x.AttributeName
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, e.Message), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

    }
}