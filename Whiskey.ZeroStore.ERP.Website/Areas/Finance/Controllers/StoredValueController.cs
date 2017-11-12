using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class StoredValueController : BaseController
    {
        protected readonly IStoreValueRuleContract _storeValueRuleContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberCollocationContract _memberCollocationContract;
        protected readonly IRechargeOrderContract _rechargeOrderContract;
        protected readonly IMemberTypeContract _memberTypeContract;
        public StoredValueController(IAdministratorContract administratorContract, IStoreValueRuleContract storeValueRuleContract
            , IMemberContract memberContract,
            IMemberCollocationContract memberCollocationContract,
            IRechargeOrderContract rechargeOrderContract,
            IMemberTypeContract memberTypeContract)
        {
            _administratorContract = administratorContract;
            _storeValueRuleContract = storeValueRuleContract;
            _memberContract = memberContract;
            _memberCollocationContract = memberCollocationContract;
            _rechargeOrderContract = rechargeOrderContract;
            _memberTypeContract = memberTypeContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);
            Expression<Func<StoreValueRule, bool>> predicate = FilterHelper.GetExpression<StoreValueRule>(request.FilterGroup);
            var allScore = _storeValueRuleContract.StoreValueRules.Where(predicate);
            var da = allScore.OrderByDescending(c => c.CreatedTime).ThenByDescending(c => c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(c => new
            {
                c.StoreValueName,
                Name = c.MemberTypes.Select(x => x.MemberTypeName),
                c.Price,
                c.RewardMoney,
                c.Score,
                c.StartDate,
                c.EndDate,
                c.IsForever,
                c.Descrip,
                c.Id,
                c.IsDeleted,
                c.IsEnabled,
                c.Sequence,
                c.UpdatedTime,
                c.Operator.Member.MemberName

            }).ToList();
            GridData<object> data = new GridData<object>(da, allScore.Count(), request.RequestInfo);
            return Json(data);
        }
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MemberType = _memberTypeContract.SelectList(string.Empty);
            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(StoreValueRuleDto svr, string MemberTypeId)
        {
            if (!string.IsNullOrEmpty(MemberTypeId))
            {
                string[] arrId = MemberTypeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                svr.MemberTypes = _memberTypeContract.MemberTypes.Where(x => arrId.Where(k => k == x.Id.ToString()).Count() > 0).ToList();
            }
            var res = _storeValueRuleContract.Insert(svr.RuleType, svr);
            return Json(res);
        }

        public ActionResult View(int id)
        {
            var enty = _storeValueRuleContract.StoreValueRules.Where(c => c.Id == id).FirstOrDefault();
            var names = _storeValueRuleContract.StoreValueRules.Where(c => c.Id == id).Select(c =>
                 c.MemberTypes.Select(x => x.MemberTypeName)
            ).ToList();
            if (names.Count > 0)
            {
                ViewData["names"] = names[0];
            }

            return PartialView(enty);

        }
        public ActionResult Enable(int id, int RuleType)
        {
            var res = _storeValueRuleContract.Enable(id, RuleType);
            return Json(res);
        }

        public ActionResult Disable(int id)
        {
            var res = _storeValueRuleContract.Disable(id);
            return Json(res);
        }

        public ActionResult Recovery(int id)
        {
            var res = _storeValueRuleContract.Recovery(id);
            return Json(res);
        }

        public ActionResult Remove(int id)
        {
            var res = _storeValueRuleContract.Remove(id);
            return Json(res);
        }
        public ActionResult RemoveAll(int[] id)
        {
            var res = _storeValueRuleContract.Remove(id);
            return Json(res);
        }
        [HttpGet]
        public ActionResult Update(int id)
        {

            var score = _storeValueRuleContract.Edit(id);
            List<SelectListItem> listSel = _memberTypeContract.SelectList(string.Empty);
            var names = _storeValueRuleContract.StoreValueRules.Where(c => c.Id == id).Select(c =>
     c.MemberTypes.Select(x => x.Id)
).ToList();
            if (names != null && names.Count > 0)
            {
                foreach (var selItem in listSel)
                {
                    foreach (int item in names[0])
                    {
                        if (item.ToString() == selItem.Value)
                        {
                            selItem.Selected = true;
                        }
                    }
                }
            }
            ViewBag.MemberType = listSel;
            ViewBag.ruletype = score.RuleType;
            return PartialView(score);
        }
        [HttpPost]
        public ActionResult Update(StoreValueRuleDto scoreRule)
        {
            if (scoreRule.ImageUrl == null || scoreRule.ImageUrl == "")
            {
                scoreRule.ImageUrl = _storeValueRuleContract.View(scoreRule.Id).ImageUrl;
            }
            var resul = _storeValueRuleContract.Update(scoreRule);
            return Json(resul);
        }
    }
}