using Microsoft.Ajax.Utilities;
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
using Whiskey.ZeroStore.ERP.Transfers.Entities;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    public class ScoreRuleController : Controller
    {
        protected readonly IScoreRuleContract _scoreRuleContract;
        public ScoreRuleController(IScoreRuleContract scoreRuleContract)
        {
            _scoreRuleContract = scoreRuleContract;
        }
        //
        // GET: /Properties/ScoreRule/
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);
            Expression<Func<ScoreRule, bool>> predicate = FilterHelper.GetExpression<ScoreRule>(request.FilterGroup);
            var allScore = _scoreRuleContract.ScoreRules.Where(predicate);
            var da = allScore.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
             {
                 c.Id,
                 c.ScoreName,
                 c.ScoreNumber,
                 c.CreatedTime,
                 c.ConsumeUnit,
                 c.ScoreUnit,
                 c.Descrip,
                 c.IsEnabled,
                 c.IsDeleted,
                 c.MinConsum,
                 AdminName=c.Operator!=null?c.Operator.Member.MemberName:""

             }).ToList();
            GridData<object> data = new GridData<object>(da, allScore.Count(), request.RequestInfo);
            return Json(data);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return PartialView(new ScoreRuleDto());
        }
        [HttpPost]
        public ActionResult Create(ScoreRule scoreRule) {
            //_scoreRuleContract.ScoreRules.Where(c => c.IsEnabled).ForEach(c => c.IsEnabled = false);
          var res=  _scoreRuleContract.Insert(scoreRule);
          return Json(res);
        }

        public ActionResult View(int id)
        {
            var enty = _scoreRuleContract.ScoreRules.FirstOrDefault(c => c.Id == id);
            return PartialView(enty);
           
        }
        public ActionResult Enable(int id)
        {
           var res= _scoreRuleContract.Enable(id);
            return Json(res);
        }

        public ActionResult Disable(int id)
        {
            var res = _scoreRuleContract.Disable(id);
            return Json(res);
        }

        public ActionResult Recovery(int id)
        {
            var res = _scoreRuleContract.Recovery(id);
            return Json(res);
        }

        public ActionResult Remove(int id)
        {
            var res = _scoreRuleContract.Remove(id);
            return Json(res);
        }
        [HttpGet]
        public ActionResult Update(int id)
        {
           var score=  _scoreRuleContract.ScoreRules.FirstOrDefault(c => c.Id == id);
            return PartialView(score);
        }
        [HttpPost]
        public ActionResult Update(ScoreRuleDto scoreRule)
        {
          var resul=_scoreRuleContract.Update(scoreRule);
            return Json(resul);
        }
    }
}