using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Services.Implements;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.jpush.api;
using Whiskey.Web.Extensions;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    [AllowCross]
    public class AdminLoginController : Controller
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AdminLoginController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IMemberContract _memberContract;

        public AdminLoginController(IAdministratorContract administratorContract,
            IMemberContract memberContract)
        {
            _administratorContract = administratorContract;
            _memberContract = memberContract;
        }
        #endregion

        #region 登录
        [HttpPost]
        public JsonResult Check(AppTypeFlag? AppType, bool? Shopkeeper)//AppType等所以App版本更新后启用【必传不允许空】
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strName = Request["AdminName"];
                string strPassword = Request["AdminPass"];
                string strJpushRegistrationID = Request["JPushRegistrationID"];
                if (string.IsNullOrEmpty(strName))
                {
                    oper.Message = "请填写登录名或者手机号码";
                    return Json(oper);
                }
                if (string.IsNullOrEmpty(strPassword))
                {
                    oper.Message = "请填写密码";
                    return Json(oper);
                }

                //if (dtoinfo.IsNull() || dtoinfo.AppVersion.IsNullOrEmpty() || dtoinfo.DeviceToken.IsNullOrEmpty())
                //{
                //    oper.Message = "登录失败,参数丢失";
                //    return Json(oper);
                //}

                Administrator dto = new Administrator();
                dto.Member = new Member();
                dto.Member.MemberName = strName;
                dto.Member.MobilePhone = strName;
                dto.Member.MemberPass = strPassword;
                dto.JPushRegistrationID = strJpushRegistrationID;
                oper = _administratorContract.CheckLogin(dto);
                if (oper.ResultType == OperationResultType.Success)
                {
                    var entity = (Administrator)oper.Data;

                    // 查找部门下的店铺
                    var store = entity.Department.Stores.FirstOrDefault();

                    var storeId = 14;
                    var storeName = "零时尚";

                    if (store != null)
                    {
                        //部门没有归属店铺,就不需要统计
                        storeId = store.Id;
                        storeName = store.StoreName;
                    }

                    //蝶掌柜登录权限判断,App全部更新后需要弃用
                    if (!ConfigurationHelper.IsDevelopment())
                    {
                        if (Shopkeeper.HasValue)
                        {
                            if (!entity.JobPosition.AppVerManages.Any(a => a.AppType == AppTypeFlag.碟掌柜 && a.IsEnabled && !a.IsDeleted))
                            {
                                oper = new OperationResult(OperationResultType.LoginError, "无权登录");
                                return Json(oper);
                            }
                        }

                        if (AppType.HasValue)
                        {
                            if (!entity.JobPosition.AppVerManages.Any(a => a.IsEnabled && !a.IsDeleted && a.AppType == AppType))
                            {
                                return Json(new OperationResult(OperationResultType.Error, $"没有登录 {AppType + ""} 的权限"));
                            }
                        }
                    }
                    

                    DateTime expiration = DateTime.Now.Add(FormsAuthentication.Timeout);
                    var userData = new { Id = entity.Id, AdminName = entity.Member.MemberName, RealName = entity.Member.RealName,
                        JPushRegistrationID = strJpushRegistrationID,

                    };
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, (userData.AdminName + userData.Id.ToString()), DateTime.Now, expiration, false, userData.ToJsonString(), FormsAuthentication.FormsCookiePath);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
                    {
                        HttpOnly = true,
                        Secure = FormsAuthentication.RequireSSL,
                        Domain = FormsAuthentication.CookieDomain,
                        Path = FormsAuthentication.FormsCookiePath,
                    };
                    Response.Cookies.Remove(cookie.Name);
                    Response.Cookies.Add(cookie);
                    //oper.Data = null;
                    bool EnablePhone = false;

                    if (entity.Member.IsNotNull() && !entity.Member.MobilePhone.IsNullOrEmpty())
                    {
                        EnablePhone = true;
                    }

                    oper = _administratorContract.GetWorkTime(entity.Id);
                    WorkTime workTime = new WorkTime();
                    if (oper.ResultType == OperationResultType.Success)
                    {
                        workTime = oper.Data as WorkTime;
                        workTime = OfficeHelper.CheckworkTime(workTime);
                    }
                    oper.Data = new
                    {
                        entity.Id,
                        EnablePhone,
                        workTime.AmStartTime,
                        workTime.PmEndTime,
                        workTime.IsVacations,
                        workTime.WorkWeek,
                        storeId,
                        storeName,
                        entity.IsPersonalTime
                    };
                    #region 添加设置标签Jpush使用
                    if (!userData.JPushRegistrationID.IsNullOrEmpty())
                    {
                        JPushClient jclient = new JPushClient(JpushApi.app_key, JpushApi.master_secret);
                        var dresult = jclient.updateDeviceTagAlias(userData.JPushRegistrationID, userData.Id.ToString(), null, new HashSet<string>() { "yuangong" }, null);
                        //oper.Other = dresult;
                    }
                    #endregion
                }
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                oper.Message = "服务器忙，请稍后重试";
                _Logger.Error<string>(ex.ToString());
                return Json(oper,JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 退出登录
        [HttpPost]
        public JsonResult Exit()
        {
            OperationResult operResult = new OperationResult(OperationResultType.Success);
            try
            {
                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)// && Request.IsAuthenticated
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    string userdata = authTicket.UserData;
                    if (!string.IsNullOrEmpty(userdata))
                    {
                        var dyuserdata = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(userdata, new { Id = string.Empty, JPushRegistrationID = string.Empty });
                        var strUid = dyuserdata.Id;
                        if (!strUid.IsNullOrEmpty())
                        {
                            Administrator modadmin = _administratorContract.View(strUid.CastTo<int>());
                            modadmin.JPushRegistrationID = null;
                            operResult = _administratorContract.Update(modadmin);
                            if (operResult.ResultType == OperationResultType.Success)
                            {
                                if (!dyuserdata.JPushRegistrationID.IsNullOrEmpty())
                                {
                                    JPushClient jclient = new JPushClient(JpushApi.app_key, JpushApi.master_secret);
                                    //var dresult = jclient.deleteDeviceTags(dyuserdata.JPushRegistrationID, new HashSet<string>() { dyuserdata.TagDepartment, "yuangong" });
                                    var dresult = jclient.updateDeviceTagAlias(dyuserdata.JPushRegistrationID, "", null, null, new HashSet<string>() { "yuangong" });
                                    //operResult.Other = dresult;
                                }
                            }
                        }
                    }
                    if (operResult.ResultType == OperationResultType.Success)
                    {
                        #region 清除Cookie和Session
                        Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddDays(-1);
                        System.Web.Security.FormsAuthentication.SignOut();
                        Session.Abandon();
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                operResult.Message = "服务器忙，请稍后重试";
                _Logger.Error<string>(ex.ToString());
            }
            return Json(operResult);
        }
        #endregion
    }
}