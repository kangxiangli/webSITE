using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Models;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Attribute
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LicenseAttribute:AuthorizeAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LicenseAttribute));
        public IMemberContract _memberContract { get; set; }        
        public IOAuthRecordContract _OAuthRecordContract { get; set; }        

        public CheckMode _checkMode;

        public LicenseAttribute(CheckMode mode)
        {
            _checkMode = mode;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (IsNotOpenId(filterContext))
            {
                string strMemberId = filterContext.HttpContext.Request["MemberId"];
                string strNum = filterContext.HttpContext.Request["U_Num"];
                int memberId = 0;
                int.TryParse(strMemberId, out memberId);
                OperationResult oper = new OperationResult(OperationResultType.LoginError);
                object obj = CacheHelper.GetCache("MemberVerify");
                if (obj == null)
                {
                    oper = this.Check(memberId, strNum);
                }
                else
                {
                    Dictionary<int, string> dic = obj as Dictionary<int, string>;
                    if (dic.ContainsKey(memberId))
                    {
                        string num = dic[memberId];
                        if (num != strNum)
                        {
                            oper.Message = "授权失效,请重新登录";
                        }
                        else
                        {
                            oper.ResultType = OperationResultType.Success;
                        }
                    }
                    else
                    {
                        oper = this.Check(memberId, strNum);
                    }
                }
                if (oper.ResultType != OperationResultType.Success)
                {
                    filterContext.Result = new JsonResult { Data = oper, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
        }
        /// <summary>
        /// 检测不是第三方授权登录
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        private bool IsNotOpenId(AuthorizationContext filterContext)
        {
            string openid = filterContext.HttpContext.Request["OpenId"];
            string strMemberId = filterContext.HttpContext.Request["MemberId"];
            if (openid.IsNotNullAndEmpty() && strMemberId.IsNullOrEmpty())
            {
                var validOpenid = true;
                var list = LoginHelper.GetMemberOpenId(); var mod = list.FirstOrDefault(f => f.OpenId == openid);
                if (mod.IsNull())
                {
                    validOpenid = _OAuthRecordContract.Entities.Any(w => w.IsEnabled && !w.IsDeleted && w.OpenId == openid);//校验openid是否有效
                    if (!validOpenid)
                    {
                        OperationResult oper = new OperationResult(OperationResultType.LoginError, "授权失效,请重新授权登录");
                        filterContext.Result = new JsonResult { Data = oper, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        mod = new MemberVerify() { OpenId = openid };
                        list.Add(mod);
                        LoginHelper.SetMemberOpenId(list);
                    }
                }
                if (validOpenid)//如果有效判断是否和会员有绑定关系
                {
                    var hasMember = _memberContract.Members.Any(w => w.IsEnabled && !w.IsDeleted && w.ThirdLoginId == openid);
                    if (!hasMember)
                    {
                        filterContext.Result = new JsonResult { Data = new OperationResult((OperationResultType)1001, "未绑定会员"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
                return false;
            }
            return true;
        }

        #region 校验用户数据
        
        /// <summary>
        /// 校验用户数据
        /// </summary>
        /// <param name="memberId">用户标识</param>
        /// <param name="strNum">SIGN标识</param>
        /// <returns></returns>
        private OperationResult Check(int memberId,string strNum)
        {
            OperationResult oper = new OperationResult(OperationResultType.LoginError);
            Member member = _memberContract.Members.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == memberId);
            if (member != null)
            {
                if (member.IsDeleted == true || member.IsEnabled == false)
                {
                    oper.Message = "该账户已经被禁用";                    
                }
                else
                {
                    if (string.IsNullOrEmpty(strNum))
                    {
                        oper.Message = "授权失效,请重新登录";                        
                    }
                    else
                    {
                        //string num = $"{member.MobilePhone}{member.UniquelyIdentifies}{member.MemberPass}".MD5Hash();
                        var strcode = member.MobileInfos.OrderByDescending(o => o.Id).Select(s => s.DeviceToken).FirstOrDefault();
                        string num = $"{member.MobilePhone}{member.UniquelyIdentifies}{member.MemberPass}{strcode}".MD5Hash();
                        if (num != strNum)
                        {
                            oper.Message = "授权失效,请重新登录";                              
                        }
                        else
                        {
                            oper.ResultType = OperationResultType.Success;
                            Dictionary<int, string> dic = new Dictionary<int, string>();
                            dic.Add(member.Id, num);
                            CacheHelper.SetCache("MemberVerify", dic);
                        }
                    }
                }

            }
            else
            {
                oper.Message = "授权失效,请重新登录";
            }
            return oper;
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LicenseAdminAttribute : AuthorizeAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LicenseAdminAttribute));
        public IAdministratorContract _administratorContract { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string strAdminId = filterContext.HttpContext.Request["AdminId"];
            string strToken = filterContext.HttpContext.Request["Token"];
            OperationResult oper = new OperationResult(OperationResultType.LoginError);

            int adminId = 0;
            if (strAdminId.IsNullOrEmpty() || strToken.IsNullOrEmpty() || !int.TryParse(strAdminId, out adminId))
            {
                oper.Message = "授权信息检验失败!";
            }
            else
            {
                var list = LoginHelper.GetAdmin();
                var mod = list.FirstOrDefault(f => f.Token == strToken && f.AdminId == adminId);
                if (mod.IsNull())
                {
                    var admin = _administratorContract.Administrators.FirstOrDefault(f => f.Id == adminId);
                    if (admin.IsEnabled == false || admin.IsDeleted == true || admin.Member == null || admin.Member.IsDeleted == true || admin.Member.IsEnabled == false)
                    {
                        oper.Message = "该用户已经被禁用";
                    }
                    else
                    {
                        var mem = admin.Member;
                        var strTokenNow = $"{mem.UniquelyIdentifies}_{mem.Id}_{mem.MemberPass}".MD5Hash();
                        if (strToken == strTokenNow)
                        {
                            mod = new AdminVerify();
                            mod.AdminId = adminId;
                            mod.Token = strToken;
                            list.Add(mod);
                            LoginHelper.SetAdmin(list);
                            oper.Message = "";
                            oper.ResultType = OperationResultType.Success;
                        }
                        else
                        {
                            oper.Message = "授权信息已失效!";
                        }
                    }
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                }
            }

            if (oper.ResultType != OperationResultType.Success)
            {
                filterContext.Result = new JsonResult { Data = oper, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

    }
}
