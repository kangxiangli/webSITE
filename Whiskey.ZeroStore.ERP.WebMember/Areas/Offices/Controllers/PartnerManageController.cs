using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.WebMember.Controllers;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Web;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Collections.Generic;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class PartnerManageController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerManageController));

        protected readonly IPartnerManageContract _PartnerManageContract;
        protected readonly IMemberContract _MemberContract;

        public PartnerManageController(
            IPartnerManageContract _PartnerManageContract
            , IMemberContract _MemberContract
            )
        {
            this._PartnerManageContract = _PartnerManageContract;
            this._MemberContract = _MemberContract;
        }

        [Layout]
        public ActionResult Index()
        {
            var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == AuthorityMemberHelper.OperatorId.Value);
            if (modPar.IsNotNull())
            {
                if (!modPar.IsRead || modPar.CheckStatus == CheckStatusFlag.待审核)
                {
                    ViewBag.CheckStatus = (int)modPar.CheckStatus;
                    ViewBag.CheckNotes = modPar.CheckNotes;
                    return View("AuditStatus");
                }
                else
                {
                    if (modPar.CheckStatus == CheckStatusFlag.未通过)
                    {
                        return View("JoinUs");
                    }
                }
            }
            else
            {
                return View("Agree");
            }
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult JoinUs()
        {
            var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == AuthorityMemberHelper.OperatorId.Value);
            if (modPar.IsNotNull())
            {
                if (!modPar.IsRead || modPar.CheckStatus == CheckStatusFlag.待审核)
                {
                    ViewBag.CheckStatus = (int)modPar.CheckStatus;
                    ViewBag.CheckNotes = modPar.CheckNotes;
                    return View("AuditStatus");
                }
                else
                {
                    if (modPar.CheckStatus == CheckStatusFlag.通过)
                    {
                        return View("Index");
                    }
                }
            }
            return View();
        }

        public JsonResult AuditStatusRead()
        {
            var oper = new OperationResult(OperationResultType.Error);
            var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == AuthorityMemberHelper.OperatorId.Value);
            if (modPar.IsNotNull())
            {
                modPar.IsRead = true;
                oper = _PartnerManageContract.Update(modPar);
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJoinUsInfo()
        {
            var data = OperationHelper.Try((opera) =>
            {
                var oper = new OperationResult(OperationResultType.Success);
                var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == AuthorityMemberHelper.OperatorId.Value);
                if (modPar.IsNotNull())
                {
                    oper.Data = new
                    {
                        modPar.Email,
                        modPar.MemberName,
                        modPar.Gender,
                        modPar.MobilePhone,
                        IDCard_Front = WebUrl + modPar.IDCard_Front,
                        IDCard_Reverse = WebUrl + modPar.IDCard_Reverse,
                        modPar.Province,
                        modPar.City,
                        modPar.Address,
                        LicencePhoto = WebUrl + modPar.LicencePhoto,
                        StorePhoto = WebUrl + modPar.StorePhoto,
                        modPar.ZipCode,
                        CheckStatus = modPar.CheckStatus + "",
                        modPar.IsRead,
                    };
                }
                else
                {
                    oper.ResultType = OperationResultType.QueryNull;
                    oper.Message = "未提交过资料";
                }

                return oper;

            }, "获取加盟信息");

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Layout]
        public ActionResult Agree()
        {
            return View();
        }
        [Layout]
        public ActionResult AuditStatus()
        {
            var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == AuthorityMemberHelper.OperatorId.Value);
            if (modPar.IsNotNull())
            {
                if (modPar.IsRead && modPar.CheckStatus == CheckStatusFlag.通过)
                {
                    return View("Index");
                }
                else
                {
                    ViewBag.CheckStatus = (int)modPar.CheckStatus;
                    ViewBag.CheckNotes = modPar.CheckNotes;
                    return View();
                }
            }
            else
            {
                return View("Agree");
            }
        }
        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult JoinUs(PartnerManageDto dto)
        {
            var data = OperationHelper.Try((opera) =>
            {
                #region 补充会员信息

                var result = new OperationResult(OperationResultType.Error);

                var dtoMember = _MemberContract.Edit(AuthorityMemberHelper.OperatorId.Value);
                dto.MemberId = dtoMember.Id;
                dto.MemberName = dtoMember.MemberName;
                dto.Email = dtoMember.Email.IsNullOrEmpty() ? dto.Email : dtoMember.Email;
                dto.Gender = (GenderFlag_CN)Enum.Parse(typeof(GenderFlag_CN), dtoMember.Gender.ToString(), true);
                dto.MobilePhone = dtoMember.MobilePhone;
                dto.CheckStatus = CheckStatusFlag.待审核;
                dto.IsRead = false;
                dto.CreateTime = DateTime.Now;

                #endregion

                var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == dtoMember.Id);
                if (modPar.IsNull())
                {
                    result = _PartnerManageContract.Insert(dto);
                }
                else
                {
                    if (modPar.CheckStatus == CheckStatusFlag.待审核)
                    {
                        result.Message = "资料正在审核中,请勿重复提交";
                        return result;
                    }
                    else if (modPar.CheckStatus == CheckStatusFlag.通过)
                    {
                        result.Message = "资料审核已通过,不能做修改";
                        return result;
                    }

                    modPar.Email = dto.Email;
                    modPar.MemberName = dto.MemberName;
                    modPar.Gender = dto.Gender;
                    modPar.MobilePhone = dto.MobilePhone;
                    modPar.IDCard_Front = dto.IDCard_Front;
                    modPar.IDCard_Reverse = dto.IDCard_Reverse;
                    modPar.Province = dto.Province;
                    modPar.City = dto.City;
                    modPar.Address = dto.Address;
                    modPar.LicencePhoto = dto.LicencePhoto;
                    modPar.StorePhoto = dto.StorePhoto;
                    modPar.ZipCode = dto.ZipCode;
                    modPar.CheckStatus = dto.CheckStatus;
                    modPar.IsRead = dto.IsRead;

                    result = _PartnerManageContract.Update(modPar);
                }

                return OperationHelper.ReturnOperationResult(result.ResultType == OperationResultType.Success, opera);

            }, "提交资料");

            return Json(data);
        }
    }
}