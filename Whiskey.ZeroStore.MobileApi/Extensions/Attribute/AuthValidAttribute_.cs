using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthValidAttribute : ActionFilterAttribute
    {
        public IModuleContract _moduleContract { get; set; }
        public IPermissionContract _permissionContract { get; set; }
        public IAdministratorContract _administratorContract { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //output.Write("<script>$(function(){alert('hi')})</script>");
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            #region 弃用转入LayoutAttribute，在基类中执行时，每个请求都会经过这里，但不是每个请求都是有页面的

            //string requrl = filterContext.HttpContext.Request.RawUrl;
            //if (requrl.StartsWith("/"))
            //    requrl = requrl.Remove(0, 1);
            //if (requrl.EndsWith("/"))
            //    requrl = requrl.Substring(0, requrl.Length - 1);
            //Module modu = CacheAccess.GetModules(_moduleContract).Where(c => c.PageUrl.Contains(requrl) && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            //if (modu != null)
            //{
            //    List<Permission> perli = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == modu.Id && c.IsDeleted == false && c.IsEnabled == true).ToList();
            //    List<Permission> currentUserPermi = GetCurrPermission();
            //    if (currentUserPermi != null)
            //    {
            //        //当前用户的所有权限
            //        List<int> currperids = currentUserPermi.Select(c => c.Id).ToList();
            //        //当前用户在当前模块所具有的权限
            //        List<Permission> curModulePer = perli.Where(c => currperids.Contains(c.Id)).ToList();
            //        //当前用户在当前模块不具有的权限
            //        List<Permission> curModuleNoPer = perli.Where(c => !currperids.Contains(c.Id)).ToList();
            //        var invali = curModuleNoPer.Select(c => c.OnlyFlag).ToList();

            //        ((ViewResult)filterContext.Result).ViewBag.inval = string.Join("|", invali);

            //    }
            //    else
            //    {

            //    }
            //}

            #endregion
        }
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetCurrPermission()
        {
            return CacheAccess.GetCurrentUserPermission(_administratorContract, _permissionContract);
        }
    }
}