using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
{
    // GET: Offices/PartnerMange
    [LicenseAdmin]
    public class PartnerMangeController : Controller
    {
        protected readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerMangeController));

        protected readonly IPartnerManageContract _PartnerManageContract;

        public PartnerMangeController(
            IPartnerManageContract _PartnerManageContract
            )
        {
            this._PartnerManageContract = _PartnerManageContract;
        }

        public JsonResult JoinUs(PartnerManageDto dto, int AdminId)
        {
            dto.ProposerId = AdminId;
            var data = _PartnerManageContract.JoinUs(dto);
            return Json(data);
        }

        public JsonResult GetJoinUsInfo(int AdminId, int PartnerMangeId)
        {
            var data = OperationHelper.Try(() =>
            {
                var oper = new OperationResult(OperationResultType.Success);
                var modPar = _PartnerManageContract.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.Id == PartnerMangeId && w.ProposerId == AdminId);
                if (modPar.IsNotNull())
                {
                    oper.Data = new
                    {
                        modPar.Email,
                        modPar.MemberName,
                        modPar.Gender,
                        modPar.MobilePhone,
                        IDCard_Front = modPar.IDCard_Front,
                        IDCard_Reverse = modPar.IDCard_Reverse,
                        modPar.Province,
                        modPar.City,
                        modPar.Address,
                        LicencePhoto = modPar.LicencePhoto,
                        StorePhoto = modPar.StorePhoto,
                        modPar.ZipCode,
                        CheckStatus = modPar.CheckStatus + "",
                        modPar.IsRead,
                        modPar.CheckNotes,
                    };
                }
                else
                {
                    oper.ResultType = OperationResultType.QueryNull;
                    oper.Message = "资料不存在,或无权访问";
                }

                return oper;

            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取加盟信息失败", true);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetMyList(int AdminId, int PageIndex = 1, int PageSize = 10)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var count = 0;
                var query = _PartnerManageContract.Entities.Where(w => w.ProposerId == AdminId && w.IsEnabled && !w.IsDeleted);

                count = query.Count();
                double page = (double)count / PageSize;
                int totalPage = (int)Math.Ceiling(page);

                query = query.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                var list = (from s in query
                            select new
                            {
                                s.Id,
                                s.MemberName,
                                s.MobilePhone,
                                CheckStatus_CN = s.CheckStatus + "",
                                CheckStatus = s.CheckStatus

                            }).ToList();
                var rdata = new { total = count, totaPage = totalPage, result = list };

                OperationResult oResult = new OperationResult(OperationResultType.Success, "", rdata);
                return oResult;
            }
            , (ex) =>
            {
                _Logger.Error(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取加盟列表失败", true);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region 获取加盟的业绩

        public JsonResult GetPerformance(int AdminId)
        {
            var data = OperationHelper.Try(() =>
            {
                var query = _PartnerManageContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.ProposerId == AdminId);

                var rdata = new
                {
                    IsFinished = query.Count(c => c.CheckStatus == ERP.Models.CheckStatusFlag.通过),
                    IsUnFinished = query.Count(c => c.CheckStatus == ERP.Models.CheckStatusFlag.未通过),
                    IsProcessing = query.Count(c => c.CheckStatus == ERP.Models.CheckStatusFlag.待审核),
                };

                return OperationHelper.ReturnOperationResult(true, "", rdata);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取业绩失败", true);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}