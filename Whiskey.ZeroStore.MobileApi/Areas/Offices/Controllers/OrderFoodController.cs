using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

// GET: Offices/OrderFood
namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
{
    [LicenseAdmin]
    public class OrderFoodController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OrderFoodController));

        protected readonly IOrderFoodContract _OrderFoodContract;
        protected readonly IAdministratorContract _AdministratorContract;
        public OrderFoodController(
            IOrderFoodContract _OrderFoodContract,
            IAdministratorContract _AdministratorContract
            )
        {
            this._OrderFoodContract = _OrderFoodContract;
            this._AdministratorContract = _AdministratorContract;
        }

        #region 订餐确认

        public JsonResult ConfirmFood(int AdminId)
        {
            var data = OperationHelper.Try((oper) =>
            {
                var res = new OperationResult(OperationResultType.Error);
                var mod = _OrderFoodContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.CreatedTime >= DateNowDate).OrderByDescending(o => o.Id).FirstOrDefault();
                if (mod.IsNotNull())
                {
                    if (mod.Admins.Any(w => w.Id == AdminId))
                    {
                        res.Message = "已预约";
                        return res;
                    }
                    else
                    {
                        var depIds = new int[] { 7, 8, 9, 10, 11, 12, 13, 1016, 1025, 1027 };//网络部	仓储部	运营部	合规部	人事部	财务部	编辑部	产品部	市场部	行政部

                        var modAdmin = _AdministratorContract.Administrators.FirstOrDefault(f => f.Id == AdminId && depIds.Contains(f.DepartmentId.Value));
                        if (modAdmin.IsNotNull())
                        {
                            mod.Admins.Add(modAdmin);
                            res = _OrderFoodContract.Update(mod);
                        }
                        else
                        {
                            res.Message = "所在部门暂未开放订餐服务";
                            return res;
                        }
                    }
                }
                else
                {
                    OrderFoodDto dto = new OrderFoodDto();
                    dto.AdminIds.Add(AdminId);
                    res = _OrderFoodContract.Insert(dto);
                }

                if (res.ResultType == OperationResultType.Success)
                {
                    res.Message = $"{oper}成功";
                }

                return res;
            }, "订餐");

            return Json(data);
        }

        #endregion
    }
}