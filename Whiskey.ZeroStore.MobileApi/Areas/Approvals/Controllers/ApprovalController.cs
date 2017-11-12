using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.MobileApi.Areas.Approvals.Controllers
{
    [License(CheckMode.Verify)]
    public class ApprovalController : Controller
    {

        #region 初始化业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApprovalController));
        //声明业务层操作对象
        protected readonly IMemberCollocationContract _memberCollocationContract;

        protected readonly IApprovalContract _approvalContract;


        //构造函数-初始化业务层操作对象
        public ApprovalController(IMemberCollocationContract memberCollocationContract,
            IApprovalContract approvalContract)
        {
            _memberCollocationContract = memberCollocationContract;
            _approvalContract = approvalContract;
        }
        #endregion

        #region 点赞和取消赞
        /// <summary>
        /// 点赞
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AddApproval()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strSourceId = Request["SourceId"];
                string strApprovalType = Request["ApprovalSource"];
                //string strProductType = Request["ProductType"];
                if (string.IsNullOrEmpty(strMemberId) && string.IsNullOrEmpty(strSourceId) && string.IsNullOrEmpty(strApprovalType))
                {
                    return Json(new OperationResult(OperationResultType.Error, "暂时无法点赞！"));
                }
                else
                {

                    ApprovalDto approvalDto = new ApprovalDto();
                    approvalDto.IsApproval = true;
                    approvalDto.MemberId = int.Parse(strMemberId);
                    approvalDto.SourceId = int.Parse(strSourceId);
                    //approvalDto.ProductType = int.Parse(strProductType);
                    approvalDto.ApprovalSource = int.Parse(strApprovalType);
                    var insertRes = _approvalContract.Insert(approvalDto);
                    if (insertRes.ResultType != OperationResultType.Success)
                    {
                        return Json(OperationResult.Error("点赞失败"));
                    }
                    // 获取点赞会员头像
                    var userPhotos = _approvalContract.Approvals.Where(a => !a.IsDeleted && a.IsEnabled && a.SourceId == approvalDto.SourceId)
                        .Select(a => a.Member.UserPhoto)
                        .ToList()
                        .Where(a => !string.IsNullOrEmpty(a))
                        .Select(a => strWebUrl + a)
                        .ToList();
                    return Json(new OperationResult(OperationResultType.Success,string.Empty, userPhotos), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务正在升级维护中，请稍后重试！"));
            }
        }
        /// <summary>
        /// 取消赞
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult CancelApproval()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strSourceId = Request["SourceId"];
                string strApprovalType = Request["ApprovalSource"];
                //string strProductType = Request["ProductType"];
                if (string.IsNullOrEmpty(strMemberId) && string.IsNullOrEmpty(strSourceId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
                }
                else
                {
                    ApprovalDto approvalDto = new ApprovalDto();
                    approvalDto.MemberId = int.Parse(strMemberId);
                    approvalDto.SourceId = int.Parse(strSourceId);
                    approvalDto.ApprovalSource = int.Parse(strApprovalType);
                    var insertRes = _approvalContract.Delete(approvalDto);
                    if (insertRes.ResultType != OperationResultType.Success)
                    {
                        return Json(OperationResult.Error("取消点赞失败"));
                    }

                    // 获取点赞会员头像
                    var userPhotos = _approvalContract.Approvals.Where(a => !a.IsDeleted && a.IsEnabled && a.SourceId == approvalDto.SourceId)
                        .Select(a => a.Member.UserPhoto)
                        .ToList()
                        .Where(a => !string.IsNullOrEmpty(a))
                        .Select(a => strWebUrl + a)
                        .ToList();
                    return Json(new OperationResult(OperationResultType.Success,string.Empty, userPhotos), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务正在升级维护中，请稍后重试！"));
            }
        }

        #endregion
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        [HttpPost]
        public JsonResult GetApprovalList(int SourceId, int ApprovalSource)
        {
            try
            {
                var res = _approvalContract.Approvals.Where(a => a.IsDeleted == false && a.IsEnabled == true && a.SourceId == SourceId && a.ApprovalSource == ApprovalSource && a.IsApproval == true)

                    .OrderByDescending(a => a.Id)
                    .Select(a => new
                    {
                        a.SourceId,
                        a.MemberId,
                        a.Member.UserPhoto,
                        a.CreatedTime
                    }).ToList();

                var data = res.Select(s => new
                {
                    s.SourceId,
                    s.MemberId,
                    UserPhoto = string.IsNullOrEmpty(s.UserPhoto) ? string.Empty : strWebUrl + s.UserPhoto,
                    CreatedTime = s.CreatedTime.ToUnixTime()
                }).ToList();

                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务正在升级维护中，请稍后重试！"));
            }

        }

    }
}