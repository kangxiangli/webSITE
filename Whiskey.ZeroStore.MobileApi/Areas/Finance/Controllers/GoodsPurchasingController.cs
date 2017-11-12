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
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;

namespace Whiskey.ZeroStore.MobileApi.Areas.Finance.Controllers
{
    public class GoodsPurchasingController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GoodsPurchasingController));

        protected readonly IGoodsPurchasingContract _GoodsPurchasingContract;

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;

        protected readonly IAdministratorContract _administratorContract;

        public GoodsPurchasingController(
            IGoodsPurchasingContract _GoodsPurchasingContract,
            ICompanyGoodsCategoryContract CompanyGoodsCategoryContract,
            IAdministratorContract administratorContract
            )
        {
            this._GoodsPurchasingContract = _GoodsPurchasingContract;
            this._CompanyGoodsCategoryContract = CompanyGoodsCategoryContract;
            this._administratorContract = administratorContract;
        }
        #endregion

        #region 添加物品采购信息
        /// <summary>
        /// 添加物品采购信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult InsertGoodsPurchasing(GoodsPurchasingDto dto)
        {
            if (!_administratorContract.CheckExists(a => a.Id == dto.AdminId && !a.IsDeleted && a.IsEnabled))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"), JsonRequestBehavior.AllowGet);
            }
            var result = _GoodsPurchasingContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取物品采购列表
        /// <summary>
        /// 获取物品采购列表
        /// </summary>
        /// <param name="status">归还状态</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <returns></returns>
        public JsonResult GetGoodsPurchasings(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                int totalCount = 0;
                int totalPage = 0;

                IQueryable<GoodsPurchasing> query = _GoodsPurchasingContract.Entities.Where(c => !c.IsDeleted && c.IsEnabled).OrderByDescending(c => c.CreatedTime);


                var list = (from c in query.Where<GoodsPurchasing, int>(pageIndex, pageSize, out totalCount, out totalPage).ToList()

                            select new
                            {
                                c.Id,
                                c.CompanyGoodsCategory.CompanyGoodsCategoryName,
                                c.CompanyGoodsCategory.ImgAddress,
                                AdminName = c.Admin.Member.RealName,
                                c.TotalAmount,
                                c.Quantity,
                                c.Notes,
                                CreatedTime = c.CreatedTime.ToString("yyyy-MM-dd"),
                                OperatorName = c.Operator.Member.RealName
                            }).ToList();

                OperationResult opera = new OperationResult(OperationResultType.Success, "", list);
                IDictionary<string, int> dic = new Dictionary<string, int>();
                dic.Add("totalCount", totalCount);
                dic.Add("totalPage", totalPage);
                opera.Other = dic;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, ex.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}