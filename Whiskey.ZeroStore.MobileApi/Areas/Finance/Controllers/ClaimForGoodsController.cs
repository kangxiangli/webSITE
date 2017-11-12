using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;

namespace Whiskey.ZeroStore.MobileApi.Areas.Finance.Controllers
{
    public class ClaimForGoodsController : BaseController
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ClaimForGoodsController));

        protected readonly IClaimForGoodsContract _ClaimForGoodsContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IDepartmentContract _departmentContract;

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;

        public ClaimForGoodsController(
            IClaimForGoodsContract _ClaimForGoodsContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            ICompanyGoodsCategoryContract CompanyGoodsCategoryContract
            )
        {
            this._ClaimForGoodsContract = _ClaimForGoodsContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContract;
            this._CompanyGoodsCategoryContract = CompanyGoodsCategoryContract;
        }
        #endregion

        #region 添加物品申领信息
        /// <summary>
        /// 添加物品申领信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult InsertClaimorGoods(ClaimForGoodsDto dto)
        {
            if (!_administratorContract.CheckExists(a => a.Id == dto.OperatorId && !a.IsDeleted && a.IsEnabled))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"), JsonRequestBehavior.AllowGet);
            }
            var result = _ClaimForGoodsContract.Apply(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 归还提醒
        /// <summary>
        /// 归还提醒
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult ReturnReminder(int[] Id)
        {
            var result = _ClaimForGoodsContract.ReturnReminder(Id, sendNotificationAction);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 归还物品
        /// <summary>
        /// 归还物品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult ReturnGoods(int[] Id)
        {
            var result = _ClaimForGoodsContract.ReturnGoods(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取物品申领列表
        /// <summary>
        /// 获取物品申领列表
        /// </summary>
        /// <param name="status">归还状态</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <returns></returns>
        public JsonResult GetClaimForGoods(int status = 0, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                int totalCount = 0;
                int totalPage = 0;

                IQueryable<ClaimForGoods> query;
                if (status > 0)
                {
                    query = _ClaimForGoodsContract.Entities.Where(c => c.ReturnStatus == status && !c.IsDeleted && c.IsEnabled).OrderByDescending(c => c.CreatedTime);
                }
                else
                {
                    query = _ClaimForGoodsContract.Entities.Where(c => !c.IsDeleted && c.IsEnabled).OrderBy(c => c.ReturnStatus).ThenByDescending(c => c.CreatedTime);
                }

                var list = (from c in query.Where<ClaimForGoods, int>(pageIndex, pageSize, out totalCount, out totalPage).ToList()

                            select new
                            {
                                c.Id,
                                c.CompanyGoodsCategory.CompanyGoodsCategoryName,
                                c.CompanyGoodsCategory.ImgAddress,
                                ApplicantName = c.Applicant.Member.RealName,
                                c.Department.DepartmentName,
                                c.IsReturn,
                                c.ReturnStatus,
                                EstimateReturnTime = c.EstimateReturnTime == null ? "" : Convert.ToDateTime(c.EstimateReturnTime).ToString("yyyy-MM-dd"),
                                ReturnTime = c.ReturnTime == null ? "" : Convert.ToDateTime(c.ReturnTime).ToString("yyyy-MM-dd"),
                                CreatedTime = c.CreatedTime.ToString("yyyy-MM-dd"),
                                OperatorName = c.Operator.Member.RealName,
                                IsReminder = c.ReturnStatus == 1 && c.IsReturn && c.ReturnTimeLimit && c.EstimateReturnTime <= DateTime.Now ? 1 : 2
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