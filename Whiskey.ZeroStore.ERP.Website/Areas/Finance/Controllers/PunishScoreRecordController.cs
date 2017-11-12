using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class PunishScoreRecordController : Controller
    {
        private IPunishScoreRecordContract _PunishScoreRecordContract;
        public PunishScoreRecordController(IPunishScoreRecordContract adminPunishScoreRecordContract)
        {
            _PunishScoreRecordContract = adminPunishScoreRecordContract;
        }
        // GET: Finance/AdminPunishScoreRecord
        [Layout]
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult List(DateTime? startDate, DateTime? endDate, PunishTypeEnum? punishType)
        {
            var request = new GridRequest(Request);
            var query = _PunishScoreRecordContract.Entities;
            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedTime >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                var endTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(p => p.CreatedTime <= endTime);
            }

            if (punishType.HasValue)
            {
                query = query.Where(p => p.PunishType == punishType.Value);
            }

            var allScore = query.OrderByDescending(p => p.CreatedTime)
                                .Skip(request.PageCondition.PageIndex)
                                .Take(request.PageCondition.PageSize);
            var da = allScore.Select(r => new
            {
                Id = r.Id,
                r.PunishScore,
                r.CreatedTime,
                PunishType = r.PunishType.ToString(),
                r.PunishAdminId,
                MemberName = r.PunishAdmin.Member.MemberName,
                Operator = r.Operator.Member.MemberName,
                r.PunishAdmin.Department.DepartmentName,
                r.Remarks
            }).ToList()
            .Select(r => new
            {
                Id = r.Id,
                r.PunishScore,
                r.CreatedTime,
                PunishType = r.PunishType.ToString(),
                r.PunishAdminId,
                MemberName = r.MemberName,
                Operator = r.Operator,
                r.DepartmentName,
                r.Remarks

            }).ToList();

            GridData<object> data = new GridData<object>(da, query.Count(), request.RequestInfo);
            return Json(data);
        }


    }
}