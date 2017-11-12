using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Shares.Controllers
{
    // GET: /API/Shares/MemberShare
    [License(CheckMode.Verify)]
    public class MemberShareController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberShareController));

        protected readonly IMemberContract _memberContract;
        protected readonly IMemberShareContract _memberShareContract;

        public MemberShareController(
           IMemberContract _memberContract,
           IMemberShareContract _memberShareContract

           )
        {
            this._memberContract = _memberContract;
            this._memberShareContract = _memberShareContract;
        }
        /// <summary>
        /// 分享,相同来源每日限分享3次
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public JsonResult Share(int MemberId, ShareFlag Flag)
        {
            var result = OperationHelper.Try(opera =>
            {
                var count = _memberShareContract.ShareCountToday(MemberId, Flag);
                if (count > 3)
                {
                    return OperationResult.Error("每日限分享3次");
                }

                var mod = new MemberShare()
                {
                    Flag = Flag,
                    MemberId = MemberId,
                };

                return _memberShareContract.Insert(mod);

            }, "分享");

            return Json(result);
        }
    }
}