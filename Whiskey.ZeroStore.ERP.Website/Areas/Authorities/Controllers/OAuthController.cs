using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.OAuths;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    // GET: Authorities/OAuth
    public class OAuthController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OAuthController));
        protected readonly IMemberContract _MemberContract;
        protected readonly IOAuthRecordContract _OAuthRecordContract;

        public OAuthController(
            IMemberContract _MemberContract,
            IOAuthRecordContract _OAuthRecordContract
            )
        {
            this._MemberContract = _MemberContract;
            this._OAuthRecordContract = _OAuthRecordContract;
        }

        public ActionResult Index()
        {
            //微信第一次握手后得到的code
            string code = Request["code"];
            if (code.IsNullOrEmpty() || code == "authdeny")
            {
                if (code.IsNullOrEmpty())
                {
                    //发起授权(第一次微信握手)
                    string authUrl = OAuthWX.GetWeiXinCode(Request.Url.ToString());
                    Response.Redirect(authUrl, true);
                }
                else
                {
                    // 用户取消授权
                    Response.Redirect(Request.Url.ToString(), true);
                }
            }
            else
            {
                //获取微信的Access_Token（第二次微信握手）
                var modelResult = OAuthWX.GetWeiXinAccessToken(code);
                //获取微信的用户信息(第三次微信握手)
                var userInfo = OAuthWX.GetWeiXinUserInfo(modelResult.SuccessResult.access_token, modelResult.SuccessResult.openid);
                //用户信息（判断是否已经获取到用户的微信用户信息）
                if (userInfo.Result && userInfo.UserInfo.openid != "")
                {
                    var resRecord = _OAuthRecordContract.InsertIfNotExist(new OAuthRecord() { OpenId = userInfo.UserInfo.openid, ThirdLoginFlag = ThirdLoginFlag.WeChat });
                    if (resRecord.ResultType == OperationResultType.Success)
                    {
                        //根据OpenId判断数据库是否存在
                        var res = _MemberContract.CheckThirdAccount(userInfo.UserInfo.openid, ThirdLoginFlag.WeChat);
                        if (res.ResultType == OperationResultType.Success)
                        {
                            //var dy = res.Data as dynamic;
                            Response.Redirect($"/home/store/index.html?MemberInfo={res.Data.ToJsonString()}", true);//?adminId={dy.AdminId}&U_Num={dy.U_Num}
                        }
                        else
                        {
                            Response.Redirect($"/home/store/index.html?WxInfo={userInfo.UserInfo.ToJsonString()}", true);//?openId={userInfo.UserInfo.openid}&headImg={userInfo.UserInfo.headimgurl}&sex={userInfo.UserInfo.sex}
                        }
                    }
                    else
                    {
                        Response.Redirect(Request.Url.ToString(), true);
                    }
                }
                else
                {
                    Response.Redirect(Request.Url.ToString(), true);
                }
            }
            return View();
        }
    }
}