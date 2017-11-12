using System;
using System.Collections.Generic;
using System.Linq;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;

namespace Whiskey.Web.Helper
{
    /// <summary>
    /// 用户权限相关辅助类
    /// </summary>
    public class PermissionHelper
    {
        /// <summary>
        /// 当前页面的所有权限
        /// </summary>
        /// <param name="requrl"></param>
        /// <returns></returns>
        public static List<Permission> GetCurrentPagePermission(string requrl, IModuleContract _moduleContract, IPermissionContract _permissionContract)
        {
            List<Permission> perli = null;
            if (requrl.StartsWith("/"))
                requrl = requrl.Remove(0, 1);

            if (requrl.EndsWith("/"))
                requrl = requrl.Substring(0, requrl.Length - 1);
            try
            {
                Module modu = CacheAccess.GetModules(_moduleContract).Where(c => c.PageUrl != null && c.PageUrl.Contains(requrl) && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();

                if (modu != null)
                {
                    //当前模块的所有权限
                    perli = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == modu.Id && c.IsDeleted == false && c.IsEnabled == true).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }

            return perli;
        }
        /// <summary>
        /// 获取当前用户在当前页的所有权限
        /// </summary>
        /// <returns></returns>
        public static List<Permission> GetCurrentUserPagePermission(string requrl, IAdministratorContract _administratorContract, IModuleContract _moduleContract, IPermissionContract _permissionContract)
        {
            List<Permission> per = null;
            var perli = GetCurrentPagePermission(requrl, _moduleContract, _permissionContract);
            List<Permission> currentUserPermi = GetCurrentUserPermission(_administratorContract, _permissionContract);
            if (currentUserPermi != null && perli != null)
            {
                List<int> currperids = currentUserPermi.Select(c => c.Id).ToList();
                //当前用户在当前模块所具有的权限
                per = perli.Where(c => currperids.Contains(c.Id)).ToList();

            }
            return per;
        }
        /// <summary>
        /// 获取当前用户在当前页面不具有的权限
        /// </summary>
        /// <returns></returns>
        public static List<Permission> GetCurrentUserPageNoPermission(string requrl, IAdministratorContract _administratorContract, IModuleContract _moduleContract, IPermissionContract _permissionContract)
        {
            List<Permission> per = null;
            var perli = GetCurrentPagePermission(requrl, _moduleContract, _permissionContract);
            List<Permission> currentUserPermi = GetCurrentUserPermission(_administratorContract, _permissionContract);
            if (currentUserPermi != null && perli != null)
            {
                List<int> currperids = currentUserPermi.Select(c => c.Id).ToList();

                per = perli.Where(c => !currperids.Contains(c.Id)).ToList();
            }
            return per;
        }

        //fsf 2016-09-27
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        private static List<Permission> GetCurrentUserPermission(IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            return CacheAccess.GetCurrentUserPermission(_administratorContract, _permissionContract);
        }
        //fsf 2016-09-27
        /// <summary>
        /// 获取某个用户的所有权限
        /// </summary>
        /// <param name="AdminId">用户Id</param>
        /// <param name="_administratorContract"></param>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static List<Permission> GetOneUserPermission(int AdminId, IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            List<Permission> lis = new List<Permission>();
            Administrator admin = _administratorContract.Administrators.Where(c => c.Id == AdminId).FirstOrDefault();
            if (admin.IsNotNull())
            {
                List<int> listperIds = new List<int>();

                #region 用户自身权限

                //listperIds.AddRanges(admin.AdministratorPermissionRelations.Select(s => s.PermissionId.Value).ToList());

                #endregion

                #region 用户角色具有的权限

                var aroleper = admin.Roles
                    .Where(w => !w.IsDeleted && w.IsEnabled)
                    .SelectMany(s => s.ARolePermissionRelations).Where(w => !w.IsDeleted && w.IsEnabled)
                    .Select(s => s.PermissionsId.Value).ToList();
                listperIds.AddRanges(aroleper);

                #endregion

                #region 用户所属组具有的权限

                //var agroupper = admin.Groups
                //    .Where(w => !w.IsDeleted && w.IsEnabled)
                //    .SelectMany(s => s.AGroupPermissionRelations).Where(w => !w.IsDeleted && w.IsEnabled).Select(s => s.PermissionsId.Value).ToList();
                //listperIds.AddRanges(agroupper);

                #endregion

                #region 用户所属部门具有的权限

                //if (admin.Department.IsEnabled && !admin.Department.IsDeleted)
                //{
                //    var adepper = admin.Department
                //    .ADepartmentPermissionRelations.Where(w => !w.IsDeleted && w.IsEnabled).Select(s => s.PermissionsId.Value).ToList();
                //    listperIds.AddRanges(adepper);
                //}

                #endregion

                listperIds = listperIds.Distinct().ToList();
                lis = CacheAccess.GetPermissions(_permissionContract).Where(w => listperIds.Contains(w.Id)).Where(w => !w.IsDeleted && w.IsEnabled).ToList();
            }
            return lis;
        }

        /// <summary>
        /// 判断用户是否具有某个模块的权限
        /// </summary>
        /// <param name="AdminId">用户Id</param>
        /// <param name="moduleId">模块Id</param>
        /// <param name="_moduleContract"></param>
        /// <param name="_administratorContract"></param>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static bool HasModulePermission(int AdminId, int moduleId, IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            return GetOneUserPermission(AdminId, _administratorContract, _permissionContract).Where(s => s.ModuleId == moduleId).Any();
        }
        /// <summary>
        /// 判断用户是否具有某个权限
        /// </summary>
        /// <param name="AdminId">用户Id</param>
        /// <param name="permissionId">权限Id</param>
        /// <param name="_administratorContract"></param>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static bool HasPermission(int AdminId, int permissionId, IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            return GetOneUserPermission(AdminId, _administratorContract, _permissionContract).Where(s => s.Id == permissionId).Any();
        }
        /// <summary>
        /// 获取且有某个权限的所有用户
        /// </summary>
        /// <param name="permissionId"></param>
        /// <param name="_administratorContract"></param>
        /// <param name="_permissionContract"></param>
        /// <returns></returns>
        public static List<int> HasPermissionAllAdmin(int permissionId, IAdministratorContract _administratorContract, IPermissionContract _permissionContract)
        {
            var listAdminIds = _administratorContract.Administrators.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => s.Id).ToList();
            var listHasAdminIds = new List<int>();
            foreach (var item in listAdminIds)
            {
                if (HasPermission(item, permissionId, _administratorContract, _permissionContract))
                {
                    listHasAdminIds.Add(item);
                }
            }
            return listHasAdminIds;
        }

        #region 店铺相关权限

 



        /// <summary>
        /// 用户掌管的所有仓库
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="_administratorContract"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<TResult> ManagedStorages<TResult>(int AdminId, IAdministratorContract _adminContract, Func<Storage, TResult> selector)
        {
            return GetCurUserDepartList(AdminId, _adminContract, s => s.Stores.SelectMany(ss => ss.Storages)).Where(w => !w.IsDeleted && w.IsEnabled).Distinct().Select(selector).ToList();
        }
        /// <summary>
        /// 用户掌管的所有仓库
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="_administratorContract"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<TResult> ManagedStorages<TResult>(int AdminId, IAdministratorContract _adminContract, Func<Storage, TResult> selector, Func<Storage, bool> predicate)
        {
            return GetCurUserDepartList(AdminId, _adminContract, s => s.Stores.SelectMany(ss => ss.Storages)).Where(w => !w.IsDeleted && w.IsEnabled).Distinct().Where(predicate).Select(selector).ToList();
        }
        /// <summary>
        /// 用户掌管的所有仓库
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="_administratorContract"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<TResult> ManagedStorages<TResult>(int AdminId, IAdministratorContract _adminContract, Func<Storage, IEnumerable<TResult>> selector, Func<Storage, bool> predicate)
        {
            return GetCurUserDepartList(AdminId, _adminContract, s => s.Stores.SelectMany(ss => ss.Storages)).Where(w => !w.IsDeleted && w.IsEnabled).Distinct().Where(predicate).SelectMany(selector).ToList();
        }

        #endregion

        #region 部门相关权限

        /// <summary>
        /// 获取当前用户所属职位，可以管理到的部门列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public static List<Department> GetCurUserDepartList(int? adminId, IAdministratorContract _adminContract)
        {
            List<Department> listdeps = new List<Department>();
            var admin = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId.Value).FirstOrDefault();
            var listDeps = admin?.JobPosition?.Departments.Where(w => !w.IsDeleted && w.IsEnabled);

            if (listDeps.IsNotNullOrEmptyThis())
            {
                listdeps.AddRange(listDeps);
            }

            var adminDep = admin?.Department;
            if (adminDep != null && adminDep.IsEnabled && !adminDep.IsDeleted && !listdeps.Contains(adminDep))
            {
                listdeps.Insert(0, adminDep);
            }

            return listdeps;
        }

        /// <summary>
        /// 获取当前用户所属职位，可以管理到的部门列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="adminId"></param>
        /// <param name="_adminContract"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<TResult> GetCurUserDepartList<TResult>(int? adminId, IAdministratorContract _adminContract, Func<Department, TResult> selector)
        {
            return GetCurUserDepartList(adminId, _adminContract).Select(selector).ToList();
        }
        /// <summary>
        /// 获取当前用户所属职位，可以管理到的部门列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="adminId"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<TResult> GetCurUserDepartList<TResult>(int? adminId, IAdministratorContract _adminContract, Func<Department, IEnumerable<TResult>> selector)
        {
            return GetCurUserDepartList(adminId, _adminContract).SelectMany(selector).ToList();
        }


        #endregion
    }
}