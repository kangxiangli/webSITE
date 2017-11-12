using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;

namespace Whiskey.ZeroStore.MobileApi.Areas.Shares.Controllers
{
    // GET: /Api/Shares/MemberCollocation
    public class MemberCollocationController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberCollocationController));

        protected readonly IMemberCollocationContract _memberCollocationContract;
        protected readonly IMemberColloEleContract _memberColloEleContract;
        protected readonly IMemberContract _memberContract;

        public MemberCollocationController(
            IMemberCollocationContract _memberCollocationContract,
            IMemberColloEleContract _memberColloEleContract,
            IMemberContract _memberContract

            )
        {
            this._memberCollocationContract = _memberCollocationContract;
            this._memberColloEleContract = _memberColloEleContract;
            this._memberContract = _memberContract;
        }

        public ActionResult MyShare(int ColloId, int MemberId)
        {
            var query = _memberCollocationContract.MemberCollocations.Where(w => w.IsEnabled && !w.IsDeleted);
            var modCol = query.FirstOrDefault(f => f.MemberId == MemberId && f.Id == ColloId);
            return View(modCol);
        }
    }
}