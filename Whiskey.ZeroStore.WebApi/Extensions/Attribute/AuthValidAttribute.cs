using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.WebApi.Extensions.Attribute
{
    //yxk 2015-11-
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthValidAttribute : ActionFilterAttribute
    {
        public IRoleContract _roleContract { get; set; }
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
            string requrl = filterContext.HttpContext.Request.RawUrl;
            if (requrl.StartsWith("/"))
                requrl = requrl.Remove(0, 1);
            if (requrl.EndsWith("/"))
                requrl = requrl.Substring(0, requrl.Length - 1);
            Module modu = CacheAccess.GetModules(_moduleContract).Where(c => c.PageUrl.Contains(requrl) && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            if (modu != null)
            {
                List<Permission> perli = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == modu.Id && c.IsDeleted == false && c.IsEnabled == true).ToList();
                List<Permission> currentUserPermi = GetCurrPermission();
                if (currentUserPermi != null)
                {
                    //当前用户的所有权限
                    List<int> currperids = currentUserPermi.Select(c => c.Id).ToList();
                    //当前用户在当前模块所具有的权限
                    List<Permission> curModulePer = perli.Where(c => currperids.Contains(c.Id)).ToList();
                    //当前用户在当前模块不具有的权限
                    List<Permission> curModuleNoPer = perli.Where(c => !currperids.Contains(c.Id)).ToList();
                    var invali = curModuleNoPer.Select(c => c.OnlyFlag).ToList();

                    ((ViewResult)filterContext.Result).ViewBag.inval = string.Join("|", invali);

                }
                else
                {

                }


            }

        }
        //yxk 2015-11-27
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetCurrPermission()
        {

            List<Permission> li = new List<Permission>();
            //用户本身具有的权限
            Administrator admin = _administratorContract.Administrators
          .Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault();
            List<Permission> newli = new List<Permission>();
            if (admin != null && admin.Permissions.ToList().Count() > 0)
            {
                li.AddRange(admin.Permissions.ToList());
                //用户角色具有的权限
                var roles = admin.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Permissions).ToList();
                foreach (var item in roles)
                {
                    li.AddRange(item);
                }

                //用户所属组具有的权限
                var groupli = admin.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Permissions).ToList();
                foreach (var item in groupli)
                {
                    li.AddRange(item.ToList());
                }
                //用户所属部门具有的权限
                li.AddRange(admin.Department.Permissions.ToList());
              
                foreach (var item in li)
                {
                    if (newli.Where(c => c.Id == item.Id).Count() == 0)
                    {
                        newli.Add(item);
                    }
                }
            }

            return newli;
        }

    }
}