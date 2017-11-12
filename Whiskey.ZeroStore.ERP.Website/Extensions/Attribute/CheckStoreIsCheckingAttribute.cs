using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Attribute
{

    /// <summary>
    /// 对正在盘点中的店铺限制action
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CheckStoreIsCheckingAttribute : ActionFilterAttribute
    {
        public CheckStoreIsCheckingAttribute()
        {

        }
        public IStoreContract _storeService { get; set; }
        public IAdministratorContract _administratorContract { get; set; }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 获取盘点中的store
            var checkingStoreIds = _storeService.GetStoresInChecking();
            if (!checkingStoreIds.Any())
            {
                //无盘点
                base.OnActionExecuting(filterContext);
            }
            else
            {
                //获取当前操作用户
                var managerdStores = _storeService.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

                if (managerdStores == null || managerdStores.Count <= 0)
                {
                    base.OnActionExecuting(filterContext);
                }
                else
                {
                    //获取当前用户可用的store
                    

                    //如果是单店铺权限的用户,检查店铺是否盘点中
                    if (managerdStores.Count == 1 && checkingStoreIds.Contains(managerdStores.First()))
                    {
                        filterContext.Result = new JsonResult() { Data = OperationResult.Error("店铺正在盘点中"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        base.OnActionExecuting(filterContext);
                    }
                }

            }

        }

    }
}