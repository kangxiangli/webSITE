using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;
using System.Web.Mvc;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    public static class Utils
    {
        /// <summary>
        /// 获取当前页请求路径
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        private static string GetPageUrl(HttpRequestBase Request)
        {
            try
            {
                var routeData = Request.RequestContext.RouteData;
                var area = routeData.DataTokens.ContainsKey("area") ? routeData.DataTokens["area"].ToString().ToLower() : string.Empty;
                var controller = routeData.Values["controller"].ToString().ToLower();
                string pageUrl = string.Format("{0}/{1}", area, controller);
                return pageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("获取请求页面路径失败");
            }
        }

        /// <summary>
        /// 获取当前请求页面所属模块的Id
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="_moduleContract"></param>
        /// <returns></returns>
        public static int? GetCurrPageModuleId(HttpRequestBase Request, IModuleContract _moduleContract)
        {
            try
            {
                string pageUrl = GetPageUrl(Request);
                return GetCurrPageModuleId(pageUrl, _moduleContract);
            }
            catch (Exception)
            {
                throw new Exception("获取请求页面所属模块出错");
            }
        }
        /// <summary>
        /// 获取请求页面所属模块的Id
        /// </summary>
        /// <param name="pageUrl">控制器路径,结构：/{area}/{controller} 例：/Members/AdjustDeposit</param>
        /// <param name="_moduleContract"></param>
        /// <returns></returns>
        public static int? GetCurrPageModuleId(string pageUrl, IModuleContract _moduleContract)
        {
            if (pageUrl.IsNullOrWhiteSpace()) return null;
            pageUrl = pageUrl.ToLower();
            var moduId = CacheAccess.GetModules(_moduleContract).Where(c => !string.IsNullOrWhiteSpace(c.PageUrl) && c.PageUrl.ToLower().Contains(pageUrl) && !c.IsDeleted && c.IsEnabled).Select(s => s.Id).FirstOrDefault();
            if (moduId == 0)
                return null;
            return moduId;
        }
        /// <summary>
        /// 获取当前页面对应模块菜单的BadgeTag
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="_moduleContract"></param>
        /// <returns></returns>
        public static string GetCurPageModuleBadgeTag(HttpRequestBase Request, IModuleContract _moduleContract)
        {
            try
            {
                string pageUrl = GetPageUrl(Request);
                return GetCurPageModuleBadgeTag(pageUrl, _moduleContract);
            }
            catch (Exception)
            {
                throw new Exception("获取请求页面模块BadgeTag出错");
            }
        }
        /// <summary>
        /// 获取当前页面对应模块菜单的BadgeTag
        /// </summary>
        /// <param name="pageUrl">控制器路径,结构：/{area}/{controller} 例：/Members/AdjustDeposit</param>
        /// <param name="_moduleContract"></param>
        /// <returns></returns>
        public static string GetCurPageModuleBadgeTag(string pageUrl, IModuleContract _moduleContract)
        {
            if (pageUrl.IsNullOrWhiteSpace()) return null;
            pageUrl = pageUrl.ToLower();
            var tag = CacheAccess.GetModules(_moduleContract).Where(c => !string.IsNullOrWhiteSpace(c.PageUrl) && c.PageUrl.ToLower().Contains(pageUrl) && !c.IsDeleted && c.IsEnabled).Select(s => s.BadgeTag).FirstOrDefault();
            return tag;
        }

        /// <summary>
        /// 更新导航属性会员信息
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="memId"></param>
        public static Member UpdateNavMemberInfo(Administrator admin, IMemberContract _memberContract)
        {
            var curMember = admin.Member;
            var modMember = new Member();
            if (curMember.IsNotNull())
            {
                modMember = _memberContract.Members.FirstOrDefault(w => w.Id == admin.MemberId.Value);

                modMember.MemberName = curMember.MemberName;
                //if (!curMember.MemberPass.IsNullOrEmpty())
                //{
                //    modMember.MemberPass = curMember.MemberPass.MD5Hash();
                //}
                modMember.Gender = curMember.Gender;
                modMember.Email = curMember.Email;
                modMember.MobilePhone = curMember.MobilePhone;
                modMember.RealName = curMember.RealName;
            }
            return modMember;
        }

        public static List<SelectListItem> GetCurUserDepartList(int? adminId,IAdministratorContract _adminContract, bool hasTitle = true)
        {
            var list = PermissionHelper.GetCurUserDepartList(adminId, _adminContract, s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.DepartmentName,
            }).ToList();
            if (hasTitle)
            {
                list.Add(new SelectListItem()
                {
                    Text = "所有部门",
                    Value = "",
                    Selected = true
                });
            }

            return list;
        }

        /// <summary>
        /// 获取登录检测超时天数
        /// </summary>
        /// <returns></returns>
        public static int GetCheckLoginTimeOutDay()
        {
            Utility.XmlHelper helper = new Utility.XmlHelper("JobPosition", "CheckLoginConfig");
            var node = helper.GetElement("DayCount");
            return (node?.Value ?? "7").CastTo<int>();
        }
    }
}
