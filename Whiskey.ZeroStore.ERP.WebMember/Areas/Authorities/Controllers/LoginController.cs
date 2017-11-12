using System;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Helper;
using Whiskey.Web.Extensions;
using Whiskey.Utility.Web;
using Whiskey.Utility.Secutiry;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Text.RegularExpressions;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Authorities.Controllers
{
    public class LoginController : BaseController
    {
        protected static readonly int QrLoginValidTime = 5;//二维码登录有效时间，分钟
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(LoginController));

        public readonly ITemplateThemeContract _templateThemeContract;

        public readonly IQrLoginContract _qrLoginContract;
        public readonly IMemberContract _memberContract;

        public LoginController(
            ITemplateThemeContract _templateThemeContract
            , IQrLoginContract _qrLoginContract
            , IMemberContract _memberContract
            )
        {
            this._templateThemeContract = _templateThemeContract;
            this._qrLoginContract = _qrLoginContract;
            this._memberContract = _memberContract;
        }

        /// <summary>
        /// 载入登录
        /// </summary>
        /// <returns></returns>
        [_Theme]
        public ActionResult Index()
        {
            if (AuthorityMemberHelper.IsVerified)
            {
                return new EmptyResult();
            }

            #region 登录加密用的密钥

            string tc_key = Guid.NewGuid().ToString("N").Substring(0, 16);
            ViewBag._tc_key = tc_key;
            HttpContext.Session["_tc_key"] = tc_key;//因为seesion会有一个失效时间，所以加先已session来验证,后来用cookie来验证

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, "_tc_key", DateTime.Now, DateTime.MaxValue, false, tc_key);
            var cookieKey = new HttpCookie(ticket.Name, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath,
                Expires = DateTime.MaxValue
            };
            HttpContext.Response.SetCookie(cookieKey);

            #endregion

            return PartialView();
        }

        /// <summary>
        /// 校验登录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Check(MemberDto dto, bool Remembered = false)
        {
            OperationResult result = new OperationResult(OperationResultType.Error, "页面失效,刷新重试");
            var _tc_key_session = HttpContext.Session["_tc_key"];
            var _tc_key = HttpContext.Request.Cookies["_tc_key"];
            if (_tc_key_session.IsNotNull() || _tc_key.IsNotNull())
            {
                var key = string.Empty;
                if (_tc_key_session.IsNotNull())
                {
                    key = _tc_key_session.ToString();
                }
                else if (_tc_key.IsNotNull())
                {
                    var ticket = FormsAuthentication.Decrypt(_tc_key.Value);
                    key = ticket.IsNotNull() ? ticket.UserData : string.Empty;
                }
                dto.MemberPass = AesHelper.Decrypt(dto.MemberPass, key).TrimEnd('\0');

                if (dto.MemberName.IsMobileNumber())
                {
                    dto.MobilePhone = dto.MemberName;
                    dto.MemberName = null;
                }

                result = _memberContract.CheckLogin(dto);
                if (result.ResultType == OperationResultType.Success)
                {
                    var entity = (Member)result.Data;

                    result.Data = new { entity.MemberName, UserPhoto = entity.UserPhoto.IsNotNullAndEmpty() ? WebUrl + entity.UserPhoto : string.Empty };

                    ResponseLoginCookie(entity, Remembered);

                    #region 清除cookie_tc_key
                    if (_tc_key.IsNotNull())
                    {
                        _tc_key.Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Response.SetCookie(_tc_key);
                    }
                    #endregion
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 将用户登录成功信息写入Cookie
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Remembered"></param>
        private void ResponseLoginCookie(Member entity, bool Remembered = false)
        {
            DateTime expiration = Remembered ? DateTime.Now.AddDays(7) : DateTime.Now.Add(FormsAuthentication.Timeout);
            var userData = new { Id = entity.Id, MemberName = entity.MemberName, RealName = entity.RealName };
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, (userData.MemberName + userData.Id.ToString()), DateTime.Now, expiration, Remembered, userData.ToJsonString(), FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(AuthorityMemberHelper.CookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = AuthorityMemberHelper.Domain,
                Path = FormsAuthentication.FormsCookiePath,
            };
            if (Remembered)
            {
                cookie.Expires = DateTime.Now.AddDays(7);
            }
            Response.Cookies.Remove(cookie.Name);
            Response.Cookies.Add(cookie);

            #region 临时客户端tcid = 11.1.1.111;Windows 10;Chrome;2016/11/25 10:30:15
            try
            {
                var hostaddress = Request.UserHostAddress;

                var systemname = UserAgentHelper.GetOperatingSystemName(Request.UserAgent);
                var browsername = UserAgentHelper.GetBrowserName(Request.UserAgent);
                var logintime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                var tcid = AesHelper.Encrypt(string.Format("{0};{1};{2};{3}", hostaddress, systemname, browsername, logintime), userData.Id.ToString());
                HttpCookie tcCookie = new HttpCookie("tcid", tcid);
                tcCookie.Expires = cookie.Expires;
                tcCookie.HttpOnly = cookie.HttpOnly;
                tcCookie.Secure = cookie.Secure;
                tcCookie.Path = cookie.Path;
                Response.Cookies.Add(tcCookie);
            }
            catch { }
            #endregion
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <returns></returns>
        [_Theme]
        public ActionResult FindPassword()
        {
            return PartialView();
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FindPassword(MemberDto dto)
        {
            dto.MemberName = InputHelper.SafeInput(dto.MemberName);
            dto.Email = InputHelper.SafeInput(dto.Email);
            var result = _memberContract.UpdatePassWord(dto);
            return Json(result);
        }

        /// <summary>
        /// 注销身份
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            ClearLoginCookie();
            return RedirectToRoute("Default", new { controller = "Home", action = "Index" });
        }

        /// <summary>
        /// 清除用户登录Cookie信息
        /// </summary>
        private void ClearLoginCookie()
        {
            HttpCookie auCookie = new HttpCookie(AuthorityMemberHelper.CookieName);
            auCookie.Domain = AuthorityMemberHelper.Domain;//不指定domain清除不掉
            auCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(auCookie);

            #region 清除tcid

            HttpCookie aCookie = new HttpCookie("tcid");
            aCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(aCookie);

            #endregion
        }

        #region 注册会员
        [HttpPost]
        public JsonResult Register(string MemberName, string MobilePhone, string MemberPass, string VerifyCode)
        {
            try
            {
                //校验手机号码，密码和验证码
                OperationResult resPhoneNum = CheckPhoneNum(MobilePhone, VerifyCodeFlag.Register);
                if (resPhoneNum.ResultType != OperationResultType.Success)
                {
                    return Json(resPhoneNum);
                }
                OperationResult resPassWord = CheckPassWord(MemberPass, false);
                if (resPassWord.ResultType == OperationResultType.Success)
                {
                    MemberPass = resPassWord.Data.ToString();
                }
                else
                {
                    return Json(resPassWord);
                }
                OperationResult resSecurityCode = CheckVerifyCode(MobilePhone, VerifyCode, VerifyCodeFlag.Register);
                if (resSecurityCode.ResultType != OperationResultType.Success)
                {
                    return Json(resSecurityCode);
                }
                IQueryable<Member> listMember = _memberContract.Members;
                MemberDto dto = new MemberDto()
                {
                    RegisterType = RegisterFlag.Web,
                    MemberName = MemberName,
                    MobilePhone = MobilePhone,
                    MemberPass = MemberPass,
                    MemberTypeId = 1,//普通会员                    
                    UserPhoto = "/Content/Images/logo-_03.png",
                };
                var addRes = _memberContract.Insert(dto);
                if (addRes.ResultType == OperationResultType.Success)
                {
                    addRes.Message = "注册成功";
                }
                return Json(addRes);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }

        #endregion

        #region 检验数据

        #region 校验手机号码
        private OperationResult CheckPhoneNum(string strPhoneNum, VerifyCodeFlag verifyCodeFlag)
        {
            if (string.IsNullOrEmpty(strPhoneNum))
            {
                return new OperationResult(OperationResultType.ValidError, "手机号码不能为空");
            }
            else
            {
                string strReg = "(1(([3587][0-9])|(47)|[8][0126789]))\\d{8}$";
                bool matchRes = Regex.IsMatch(strPhoneNum, strReg);
                if (matchRes)
                {
                    var entity = _memberContract.Members.Where(x => x.MobilePhone == strPhoneNum).FirstOrDefault();
                    if (entity != null)
                    {
                        if (entity.IsDeleted == true && entity.IsEnabled == false)
                        {
                            return new OperationResult(OperationResultType.ValidError, "该手机号码已经被禁用");
                        }
                        else
                        {
                            if (verifyCodeFlag == VerifyCodeFlag.Register)
                            {
                                return new OperationResult(OperationResultType.ValidError, "手机号码已经存在");
                            }
                            else if (verifyCodeFlag == VerifyCodeFlag.ChangePassword)
                            {
                                return new OperationResult(OperationResultType.Success);
                            }
                            else
                            {
                                return new OperationResult(OperationResultType.Error, "获取手机验证失败");
                            }
                        }
                    }
                    else
                    {
                        if (verifyCodeFlag == VerifyCodeFlag.Register)
                        {
                            return new OperationResult(OperationResultType.Success, "手机号码正确");
                        }
                        else if (verifyCodeFlag == VerifyCodeFlag.ChangePassword)
                        {
                            return new OperationResult(OperationResultType.Error, "该手机号码未注册");
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "获取手机验证失败");
                        }
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.ValidError, "请输入正确格式的手机号");
                }
            }
        }
        #endregion

        #region 校验密码
        private OperationResult CheckPassWord(string strPassWord, bool md5 = true)
        {
            strPassWord = strPassWord.Trim();
            if (string.IsNullOrEmpty(strPassWord))
            {
                return new OperationResult(OperationResultType.ValidError, "请输入密码");
            }
            else
            {
                if (strPassWord.Length < 6 || strPassWord.Length > 20)
                {
                    return new OperationResult(OperationResultType.ValidError, "密码长度6-20个字符");
                }
                else
                {
                    strPassWord = md5 ? strPassWord.MD5Hash() : strPassWord;
                    return new OperationResult(OperationResultType.Success, "密码可以使用", strPassWord);
                }
            }
        }
        #endregion

        #region 校验验证码
        private OperationResult CheckVerifyCode(string strPhoneNum, string strSecurityCode, VerifyCodeFlag verifyCodeFlag)
        {
            if (string.IsNullOrEmpty(strSecurityCode))
            {
                return new OperationResult(OperationResultType.ValidError, "请输入验证码");
            }
            else
            {
                int verifyCodeType = (int)verifyCodeFlag;
                string _key = string.Format("_verifycode_{0}_{1}_", verifyCodeType.ToString(), strPhoneNum);
                object obj = CacheHelper.GetCache(_key);
                if (obj == null)
                {
                    return new OperationResult(OperationResultType.Error, "验证码失效，请重新获取");
                }
                else
                {
                    CacheHelper.RemoveCache(_key);
                    return new OperationResult(OperationResultType.Success, "验证码正常");
                }
            }
        }
        #endregion

        #endregion

        #region 扫码登录

        /// <summary>
        /// 生成登录二维码
        /// </summary>
        /// <returns>二维码图片地址</returns>
        private OperationResult CreateQrLoginCode()
        {
            OperationResult Oresult = new OperationResult(OperationResultType.Error, "二维码获取失败");
            QrLoginDto qrlogin = new QrLoginDto();
            qrlogin.QrCode = Guid.NewGuid().ToString("N");
            var imgpath = ConfigurationHelper.GetAppSetting("QrLoginPath") ?? "/Content/Images/QrLogin/";
            var applink = string.Format("uuid={0};type={1}", qrlogin.QrCode, "qr_login");
            var qrimgpath = ImageHelper.CreateQRCode(qrlogin.QrCode, imgpath, applink, 8);
            if (qrimgpath.IsNotNull())
            {
                qrlogin.QrImgPath = qrimgpath;
                OperationResult result = _qrLoginContract.Insert(qrlogin);
                if (result.ResultType == OperationResultType.Success)
                {
                    Oresult.ResultType = OperationResultType.Success;
                    Oresult.Message = "";
                    Oresult.Data = new { qrImg = qrlogin.QrImgPath, uuid = qrlogin.QrCode };
                    return Oresult;
                }
                else
                {
                    //添加失败，删除创建的图片
                    FileHelper.Delete(qrimgpath);
                }
            }
            qrlogin.QrImgPath = Path.Combine("/Content/Images/", "qrcode_error.png");
            qrlogin.QrCode = "xxxxxxxxxxx";
            Oresult.Data = new { qrImg = qrlogin.QrImgPath, uuid = qrlogin.QrCode };
            return Oresult;
        }

        [HttpPost]
        public JsonResult GetQrCode()
        {
            OperationResult result = CreateQrLoginCode();

            return Json(result);
        }

        /// <summary>
        /// 获取二维码登录状态
        /// </summary>
        /// <param name="qrcode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetQrCodeStatus(string uuid)
        {
            var now = DateTime.Now.AddMinutes(-QrLoginValidTime);
            var entity = _qrLoginContract.QrLogins.FirstOrDefault(c => c.QrCode == uuid && c.CreatedTime >= now && c.MemberId != null);
            if (entity.IsNotNull())
            {
                var dto = entity.Member.MapperTo<MemberDto>();
                OperationResult oResult = new OperationResult(OperationResultType.Success);
                oResult.Message = "扫码登录成功";
                //_memberContract.CheckLogin(dto);
                ResponseLoginCookie(entity.Member);
                return Json(oResult);
            }
            else
            {
                OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                oResult.Message = "请扫码登录";
                return Json(oResult);
            }
        }

        [HttpPost]
        [AllowCross]
        public JsonResult ConfirmQrLogin(string uuid, int? memberId)
        {
            if (uuid.IsNullOrEmpty() || !memberId.HasValue)
            {
                OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                oResult.Message = "请使用零时尚扫码登录";
                return Json(oResult);
            }
            var now = DateTime.Now.AddMinutes(-QrLoginValidTime);
            var curMod = _qrLoginContract.QrLogins.FirstOrDefault(f => f.QrCode == uuid && f.MemberId == null && f.CreatedTime >= now);
            if (curMod.IsNotNull())
            {
                var curAdmin = _memberContract.Members.FirstOrDefault(f => f.Id == memberId);
                if (curAdmin.IsNotNull() && curAdmin.IsEnabled && !curAdmin.IsDeleted)
                {
                    curMod.MemberId = memberId;
                    var resultUp = _qrLoginContract.Update(curMod);
                    if (resultUp.ResultType == OperationResultType.Success)
                    {
                        OperationResult oResult = new OperationResult(OperationResultType.Success);
                        oResult.Message = "登录成功";
                        QrLoginHub.sendStatus(uuid);
                        return Json(oResult);
                    }
                }
                else
                {
                    OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                    oResult.Message = "用户不存在或已被禁止登录";
                    return Json(oResult);
                }
            }

            OperationResult Result = new OperationResult(OperationResultType.ValidError);
            Result.Message = "二维码已过期，请重新扫码";
            return Json(Result);
        }

        [HttpPost]
        [AllowCross]
        public JsonResult BackQrLogin(string uuid)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Success);
            oResult.Message = "";
            QrLoginHub.backQrCode(uuid);
            return Json(oResult);
        }

        [HttpPost]
        [AllowCross]
        public JsonResult ScanComplete(string uuid, int? memberId)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Success);
            oResult.Message = "扫码成功";
            var adminImg = string.Empty;
            if (memberId.HasValue)
            {
                var up = _memberContract.Members.Where(w => w.Id == memberId).Select(s => s.UserPhoto).FirstOrDefault();
                adminImg = up.IsNotNullAndEmpty() ? WebUrl + up : string.Empty;
            }
            QrLoginHub.scanComplete(uuid, adminImg);
            return Json(oResult);
        }

        [HttpPost]
        [AllowCross]
        public JsonResult AutoLogout(int? AdminId, bool clearAll, string uuid)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error, "请使用零时尚,取消授权登录");
            if (AdminId.HasValue)
            {
                oResult = AutoExitHub.AutoExit(AdminId.ToString(), clearAll, uuid);
            }
            return Json(oResult);
        }

        /// <summary>
        /// 获取当前用户所有登录设备
        /// </summary>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        [AllowCross]
        public JsonResult GetAllLoginDevice(int? AdminId)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error, "请使用零时尚");
            if (AdminId.HasValue)
            {
                oResult.ResultType = OperationResultType.Success;
                oResult.Message = "";
                oResult.Data = AutoExitHub._hubInfo.Where(w => w.MemberId == AdminId.ToString()).DistinctBy(d => d.browserId).Select(s => s.uuid).ToList();
            }
            return Json(oResult, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}