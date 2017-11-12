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

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Controllers
{
    [License(CheckMode.Verify)]
    public class ProductAttributeController : Controller
    {
        #region 声明业务层操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductAttributeController));

        protected readonly IProductAttributeContract _productAttrContract;

        public ProductAttributeController(IProductAttributeContract productattributeContract)
        {
            _productAttrContract = productattributeContract;
        }
        #endregion

        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        #region 获取商品属性
        /// <summary>
        /// 获取商品属性
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {
            string strProductAttrType = Request["ProductAttrType"];//0表示无图，1表示有图
            IQueryable<ProductAttribute> listProAttr= _productAttrContract.ProductAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId==1);
            try
            {
                int productAttrType=int.Parse(strProductAttrType);
                if (productAttrType==0)
                {
                    var result = listProAttr.Select(x => new 
                    {
                       ProductAttrId= x.Id,
                       x.AttributeName,                       
                    });
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", result),JsonRequestBehavior.AllowGet);
                }
                else if (productAttrType==1)
                {
                    var result = listProAttr.Select(x => new 
                    {
                       ProductAttrId= x.Id,
                       ProductAttrName=x.AttributeName,
                       IconPath=strWebUrl+x.IconPath
                    });
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", result),JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍候重试！"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍候重试！"), JsonRequestBehavior.AllowGet);
            }
            
        }
        #endregion
    }
}