using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.MobileApi
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    class AuthValidAttribute : AuthorizeAttribute
    {
        public IRoleContract _roleContract { get; set; }
        public IModuleContract _moduleContract { get; set; }
        public IPermissionContract _permissionContract { get; set; }
        public IAdministratorContract _administratorContract { get; set; }
        public IStoreContract _storeContract { get; set; }

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AuthValidAttribute));
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool resul = false;

            // string area = httpContext.Request.RequestContext.RouteData.DataTokens["area"].ToString();

            string control = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string action = httpContext.Request.RequestContext.RouteData.Values["action"].ToString();


            var permission = _permissionContract.Permissions.FirstOrDefault(c => c.ControllName == control && c.ActionName == action && c.IsEnabled && !c.IsDeleted);
            /* 2016-4-20 yxk 
               如果当前权限对应的模块已经完成了权限的添加操作 则判断用户是否有该权限
             * 如果当前权限对应的模块未完成权限的添加操作，不考虑用户是否具有该权限
             */
            if (permission != null)
            {
                httpContext.Items.Add("_ruledes", permission.Description);
                if (permission.Module.IsCompleteRule)
                {
                    //当前用户的所有权限
                    List<Permission> currentUserPermi = CacheAccess.GetCurrentUserPermission(_administratorContract,_permissionContract);


                    //当前模块的所有权限
                    var currModulePermission = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == permission.ModuleId&&c.IsEnabled&&!c.IsDeleted).ToList();
                    //当前用户在当前模块具有的权限
                    var currUserPermiForCurrModule = currentUserPermi.Where(c => currModulePermission.Select(g => g.Id).Contains(c.Id)).ToList();
                    //当前用户在当前模块不具有的权限
                    var currUserNoPermiForCurrModule =
                        currModulePermission.Where(c => !(currentUserPermi.Select(g => g.Id).Contains(c.Id))).ToList();
                    var invali = currUserNoPermiForCurrModule.Where(c => !string.IsNullOrEmpty(c.OnlyFlag)).Select(c => c.OnlyFlag).ToList();
                    httpContext.Items.Remove("_inval_role");
                    httpContext.Items.Add("_inval_role", invali);
                    //具有该权限
                    resul = currentUserPermi.Any(c => c.Id == permission.Id);
                }
                else
                {
                    //未完成权限添加
                    resul = true;
                }
            }
            else
            {//当前连接未加入权限控制
                resul = true;
            }

            return resul;
            //var moduId = CacheAccess.GetModules(_moduleContract).Where(c => c.PageController.ToLower() == control.ToLower() && c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).FirstOrDefault();


            //if (moduId != 0)
            //{
            //List<Permission> perli = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ControllName == control &&c.ActionName==action&& !c.IsDeleted  && c.IsEnabled ).ToList();
            //List<Permission> currentUserPermi = CacheAccess.GetCurrentUserPermission(_administratorContract);
            //if (currentUserPermi != null)
            //{
            //当前用户的所有权限
            // List<int> currperids = currentUserPermi.Select(c => c.Id).ToList();
            //当前用户在当前模块所具有的权限
            // List<Permission> curModulePer = perli.Where(c => currperids.Contains(c.Id)).ToList();


            //当前用户在当前模块不具有的权限
            //List<Permission> noperli = new List<Permission>();
            //foreach (var curp in perli)
            //{
            //    if (noperli.Count == 0)
            //        noperli = perli;
            //    noperli = noperli.Where(c => c.ModuleId != curp.ModuleId && c.ActionName != curp.ActionName).ToList();

            //}

            //List<Permission> curModuleNoPer = perli.Where(c => !currperids.Contains(c.Id)).ToList();
            //List<Permission> curModuleNoPer = perli.Where(c => !currperids.Contains(c.Id)).ToList();

            //var invali = curModuleNoPer.Where(c => !string.IsNullOrEmpty(c.OnlyFlag)).Select(c => c.OnlyFlag).ToList();
            //httpContext.Items.Remove("_inval_role");
            //httpContext.Items.Add("_inval_role", invali);
            //var nopers = curModuleNoPer.Any(c => c.ModuleId == moduId && c.ActionName == action);
            //if (!nopers)
            //{

            //    resul = true;
            //}
            //test
            //if (requrl.Contains("Products/Product/List"))
            //    resul = false;

            // }

            //}


        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string method = filterContext.HttpContext.Request.HttpMethod.ToUpperInvariant();
            if (method == "GET")
            {
                _Logger.Error<string>("未读取Cookie数据");
                filterContext.Result = AuthorityHelper.OperatorId == null
                    ? new RedirectResult("/Authorities/Login/Index")
                    : new RedirectResult("/Content/Error/index.html");
            }
            else
            {
                var desc = HttpContext.Current.Items["_ruledes"] as string;
                string messg = "没有权限";
                if (!string.IsNullOrEmpty(desc))
                    messg += ":" + desc;
                filterContext.Result = new JsonResult()
                {
                    Data = new OperationResult(OperationResultType.ValidError, messg),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
    public class AuthoriAttribute : ActionFilterAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(AuthoriAttribute));
        public IPermissionContract _permissionContract { get; set; }
        public IModuleContract _moduleContract { get; set; }
        public IAdministratorContract _administratorContract { get; set; }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var t = filterContext.HttpContext.Items["_inval_role"] as List<string>;
            if (t != null && t.Any())
            {
                var resul = filterContext.Result as ViewResult;
                if (resul != null)
                    ((ViewResult)filterContext.Result).ViewBag.inval = string.Join("|", t);
            }
        }
    }
}