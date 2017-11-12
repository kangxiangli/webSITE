
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
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Services.Implements;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Helper;
using Whiskey.Web.Extensions;
using Whiskey.Utility.Web;
using Whiskey.Utility.Secutiry;
using Whiskey.ZeroStore.ERP.Website.Controllers;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    public class LoginController : BaseController
    {
        protected static readonly int QrLoginValidTime = 5;//二维码登录有效时间，分钟
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(LoginController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IModuleContract _moduleContract;

        public readonly ITemplateThemeContract _templateThemeContract;

        public readonly IQrLoginContract _qrLoginContract;

        public readonly IAttendanceContract _attendanceContract;

        public LoginController(IAdministratorContract administratorContract,
            IModuleContract moduleContract
            , ITemplateThemeContract _templateThemeContract
            , IQrLoginContract _qrLoginContract,
            IAttendanceContract _attendanceContract
            )
        {
            _administratorContract = administratorContract;
            _moduleContract = moduleContract;
            this._templateThemeContract = _templateThemeContract;
            this._qrLoginContract = _qrLoginContract;
            this._attendanceContract = _attendanceContract;
        }

        /// <summary>
        /// 载入登录
        /// </summary>
        /// <returns></returns>

        public ActionResult Index()
        {
            //Response.Cache.SetOmitVaryStar(true);
            if (!ConfigurationHelper.IsDevelopment())//到登录页直接清除之前的缓存
                ClearLoginCookie();

            var entityService = DependencyResolver.Current.GetService<ModuleService>();
            var enService = DependencyResolver.Current.GetService<IModuleContract>();
            if (AuthorityHelper.IsVerified)
            {
                return RedirectToAction("Index", "Home", new { area = "Authorities" });
            }
            ViewBag.bgImg = "/Content/Images/login_bg_default.jpg";//默认登录背景图
            TemplateTheme theme = CacheAccess.GetCurTheme(_templateThemeContract, TemplateThemeFlag.ERP);
            if (theme.IsNotNull())
            {
                ViewBag.themePath = theme.Path;
                var themeExist = false;
                if (!theme.Path.IsNullOrEmpty())
                {
                    themeExist = FileHelper.ThemeIsExist(theme.Path);
                }
                ViewBag.themeExist = themeExist;
                if (theme.BackgroundImg.IsNotNullAndEmpty() && FileHelper.FileIsExist(theme.BackgroundImg))
                {
                    ViewBag.bgImg = theme.BackgroundImg;
                }
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

            return View();
        }

        /// <summary>
        /// 校验登录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Check(Administrator dto, bool Remembered = false)
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
                dto.Member.MemberPass = AesHelper.Decrypt(dto.Member.MemberPass, key).TrimEnd('\0');
                result = _administratorContract.CheckLogin(dto);
                if (result.ResultType == OperationResultType.Success)
                {
                    var entity = (Administrator)result.Data;
                    var timeoutDay = Utils.GetCheckLoginTimeOutDay();

                    var attTime = _attendanceContract.Attendances.Where(w => w.IsEnabled && !w.IsDeleted && w.AdminId == entity.Id).OrderByDescending(o => o.AttendanceTime).Select(s => s.AttendanceTime).FirstOrDefault();

                    if (!ConfigurationHelper.IsDevelopment())
                    {
                        if (entity?.JobPosition?.CheckLogin == true && attTime < DateTime.Now.Date.AddDays(-Math.Abs(timeoutDay)))
                        {
                            if (!ConfigurationHelper.IsDevelopment())
                            {
                                return Json(new OperationResult(OperationResultType.LoginError, "账号已冻结，请使用小蝶办公签到解冻"));
                            }
                        }

                        if (entity?.JobPosition?.AllowPwd == true)
                        {
                            result.Data = null;

                            ResponseLoginCookie(entity, Remembered);

                            #region 清除cookie_tc_key
                            if (_tc_key.IsNotNull())
                            {
                                _tc_key.Expires = DateTime.Now.AddDays(-1);
                                HttpContext.Response.SetCookie(_tc_key);
                            }
                            #endregion
                        }
                        else
                        {
                            result.ResultType = OperationResultType.LoginError;
                            result.Data = null;
                            result.Message = "请使用扫码登录";
                        }
                    }
                    else
                    {
                        result.Data = null;

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
            }
            return Json(result);
        }

        private void WriteLoginCookieClearTcKey()
        {

        }

        /// <summary>
        /// 将用户登录成功信息写入Cookie
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Remembered"></param>
        private void ResponseLoginCookie(Administrator entity, bool Remembered = false)
        {
            DateTime expiration = Remembered ? DateTime.Now.AddDays(7) : DateTime.Now.Add(FormsAuthentication.Timeout);
            var userData = new { Id = entity.Id, AdminName = entity.Member.MemberName, RealName = entity.Member.RealName };
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, (userData.AdminName + userData.Id.ToString()), DateTime.Now, expiration, Remembered, userData.ToJsonString(), FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
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

                //var hostname = Request.LogonUserIdentity.Name;//获取得不正确,暂时弃用

                var systemname = UserAgentHelper.GetOperatingSystemName(Request.UserAgent);
                var browsername = UserAgentHelper.GetBrowserName(Request.UserAgent);
                var logintime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                var tcid = AesHelper.Encrypt(string.Format("{0};{1};{2};{3}", hostaddress, systemname, browsername, logintime), userData.Id.ToString());
                HttpCookie tcCookie = new HttpCookie("tcid", tcid);
                tcCookie.Expires = cookie.Expires;
                tcCookie.HttpOnly = cookie.HttpOnly;
                tcCookie.Secure = cookie.Secure;
                tcCookie.Domain = cookie.Domain;
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
        [Log]
        public ActionResult FindPassword(Administrator dto)
        {
            dto.Member.MemberName = InputHelper.SafeInput(dto.Member.MemberName);
            dto.Member.Email = InputHelper.SafeInput(dto.Member.Email);
            var result = _administratorContract.FindPassword(dto);
            return Json(result);
        }

        /// <summary>
        /// 注销身份
        /// </summary>
        /// <returns></returns>
        [Log]
        public ActionResult Logout()
        {
            ClearLoginCookie();
            Module module = _moduleContract.Modules.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PageUrl == "/Authorities/Login/index");
            if (module == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return Redirect("/" + module.OverrideUrl);
            }
        }
        /// <summary>
        /// 清除用户登录Cookie信息
        /// </summary>
        private void ClearLoginCookie()
        {
            FormsAuthentication.SignOut();

            #region 清除tcid

            HttpCookie aCookie = new HttpCookie("tcid");
            aCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(aCookie);

            #endregion

            var adminId = AuthorityHelper.GetId();
            if (adminId.HasValue)
            {
                CacheAccess.ClearPermissionCache(adminId.Value);
            }
        }

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
            //var appurl = ConfigurationHelper.GetAppSetting("QrLoginUrl");
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
        [Log]
        public JsonResult GetQrCodeStatus(string uuid)
        {
            var now = DateTime.Now.AddMinutes(-QrLoginValidTime);
            var entity = _qrLoginContract.QrLogins.FirstOrDefault(c => c.QrCode == uuid && c.CreatedTime >= now && c.AdminId != null);
            if (entity.IsNotNull())
            {
                OperationResult oResult = new OperationResult(OperationResultType.Success);
                oResult.Message = "扫码登录成功";
                _administratorContract.CheckLogin(entity.Administrator);
                ResponseLoginCookie(entity.Administrator);
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
        public JsonResult GetQrCodeStatus2(string uuid)
        {
            var now = DateTime.Now.AddMinutes(-QrLoginValidTime);
            var entity = _qrLoginContract.QrLogins.FirstOrDefault(c => c.QrCode == uuid && c.CreatedTime >= now && c.AdminId != null);
            if (entity.IsNotNull())
            {
                var modJob = entity.Administrator.JobPosition;
                var attTime = _attendanceContract.Attendances.Where(w => w.IsEnabled && !w.IsDeleted && w.AdminId == entity.AdminId).OrderByDescending(o => o.AttendanceTime).Select(s => s.AttendanceTime).FirstOrDefault();


                if (!ConfigurationHelper.IsDevelopment())
                {


                    if (modJob.CheckLogin == true && attTime < DateTime.Now.Date.AddDays(-Math.Abs(Utils.GetCheckLoginTimeOutDay())))
                    {


                        return Json(new OperationResult(OperationResultType.LoginError, "账号已冻结，请使用小蝶办公签到解冻"));

                    }
                    if (!modJob.AppVerManages.Any(a => a.AppType == ERP.Models.Enums.AppTypeFlag.碟掌柜 && a.IsEnabled && !a.IsDeleted))
                    {
                        return Json(new OperationResult(OperationResultType.LoginError, "账号需要碟掌柜权限"));
                    }
                }

                var liststores = modJob.Departments.Where(w => w.IsEnabled && !w.IsDeleted).SelectMany(s => s.Stores.Where(w => w.IsEnabled && !w.IsDeleted));

                if (liststores.IsEmpty())
                {
                    return Json(new OperationResult(OperationResultType.LoginError, "账号没有掌管的店铺,取消本次登录授权"));
                }
                else if (liststores.Count() > 1)
                {
                    return Json(new OperationResult(OperationResultType.LoginError, "账号掌管的店铺过多,取消本次登录授权"));
                }

                var modMember = entity.Administrator.Member;
                var modStore = liststores.FirstOrDefault();

                var data = new
                {
                    AdminImg = modMember.UserPhoto.IsNotNullAndEmpty() ? WebUrl + modMember.UserPhoto : null,
                    AdminName = modMember.MemberName,
                    AdminId = entity.AdminId,
                    StoreId = modStore.Id,
                    StoreName = modStore.StoreName,
                    StoreImg = modStore.StorePhoto.IsNotNullAndEmpty() ? WebUrl + modStore.StorePhoto : null,
                };

                OperationResult oResult = new OperationResult(OperationResultType.Success);
                oResult.Message = "扫码登录成功";
                oResult.Data = data;
                ResponseLoginCookie(entity.Administrator);
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
        public JsonResult ConfirmQrLogin(string uuid, int? adminId)
        {
            if (uuid.IsNullOrEmpty() || !adminId.HasValue)
            {
                OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                oResult.Message = "请使用小蝶办公扫码登录";
                return Json(oResult);
            }
            var now = DateTime.Now.AddMinutes(-QrLoginValidTime);
            var curMod = _qrLoginContract.QrLogins.FirstOrDefault(f => f.QrCode == uuid && f.AdminId == null && f.CreatedTime >= now);
            if (curMod.IsNotNull())
            {
                var curAdmin = _administratorContract.Administrators.FirstOrDefault(f => f.Id == adminId);
                if (curAdmin.IsNotNull() && curAdmin.IsEnabled && !curAdmin.IsDeleted)
                {
                    curMod.AdminId = adminId;
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
        public JsonResult ScanComplete(string uuid, int? adminId)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Success);
            oResult.Message = "扫码成功";
            var adminImg = string.Empty;
            if (adminId.HasValue)
            {
                adminImg = _administratorContract.View(adminId.Value).Member.UserPhoto;
            }
            QrLoginHub.scanComplete(uuid, adminImg);
            return Json(oResult);
        }

        [HttpPost]
        [AllowCross]
        public JsonResult AutoLogout(int? AdminId, bool clearAll, string uuid)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error, "请使用小蝶,取消授权登录");
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
            OperationResult oResult = new OperationResult(OperationResultType.Error, "请使用小蝶");
            if (AdminId.HasValue)
            {
                oResult.ResultType = OperationResultType.Success;
                oResult.Message = "";
                oResult.Data = AutoExitHub._hubInfo.Where(w => w.AdminId == AdminId.ToString()).DistinctBy(d => d.browserId).Select(s => s.uuid).ToList();
            }
            return Json(oResult, JsonRequestBehavior.AllowGet);
        }

    }
}