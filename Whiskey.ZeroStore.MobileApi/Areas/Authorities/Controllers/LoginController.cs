using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.Web.Extensions;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Authorities.Controllers
{
    public class LoginController : Controller
    {
        #region 初始化操作对象
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
        #endregion

        #region 扫码登陆
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
            return Json(oResult,JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 二维码登录
        [HttpPost]
        [AllowCross]
        public JsonResult ConfirmQrLogin(string uuid, int? adminId)
        {
            if (uuid.IsNullOrEmpty() || !adminId.HasValue)
            {
                OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                oResult.Message = "请使用小蝶办公扫码登录";
                return Json(oResult, JsonRequestBehavior.AllowGet);
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
                        return Json(oResult, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    OperationResult oResult = new OperationResult(OperationResultType.QueryNull);
                    oResult.Message = "用户不存在或已被禁止登录";
                    return Json(oResult, JsonRequestBehavior.AllowGet);
                }
            }

            OperationResult Result = new OperationResult(OperationResultType.ValidError);
            Result.Message = "二维码已过期，请重新扫码";
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 强制退出
        [HttpPost]
        [AllowCross]
        public JsonResult AutoLogout(int? AdminId, bool clearAll, string uuid)
        {
            OperationResult oResult = new OperationResult(OperationResultType.Error, "请使用小蝶,取消授权登录");
            if (AdminId.HasValue)
            {
                oResult = AutoExitHub.AutoExit(AdminId.ToString(), clearAll, uuid);
            }
            return Json(oResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取当前用户所有登录设备
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
        #endregion
    }
}