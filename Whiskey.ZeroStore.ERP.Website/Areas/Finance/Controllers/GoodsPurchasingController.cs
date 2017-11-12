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
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;
using System.Collections.Generic;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class GoodsPurchasingController : BaseController
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GoodsPurchasingController));

        protected readonly IGoodsPurchasingContract _GoodsPurchasingContract;

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;

        public GoodsPurchasingController(
            IGoodsPurchasingContract _GoodsPurchasingContract,
            ICompanyGoodsCategoryContract CompanyGoodsCategoryContract
            )
        {
            this._GoodsPurchasingContract = _GoodsPurchasingContract;
            this._CompanyGoodsCategoryContract = CompanyGoodsCategoryContract;
        }
        #endregion

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            #region 获取类型
            var companyGoodsCategoryTypeFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryTypeFlag)))
            {
                companyGoodsCategoryTypeFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryTypeFlagList = companyGoodsCategoryTypeFlagList;
            #endregion
            return PartialView();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(GoodsPurchasingDto dto)
        {
            var result = _GoodsPurchasingContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(GoodsPurchasingDto dto)
        {
            var result = _GoodsPurchasingContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _GoodsPurchasingContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _GoodsPurchasingContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            try
            {
                GridRequest request = new GridRequest(Request);

                var rule_CompanyGoodsCategoryName = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "CompanyGoodsCategoryName_Search");
                if (rule_CompanyGoodsCategoryName != null)
                {
                    request.FilterGroup.Rules.Remove(rule_CompanyGoodsCategoryName);
                }

                Expression<Func<GoodsPurchasing, bool>> predicate = FilterHelper.GetExpression<GoodsPurchasing>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    if (rule_CompanyGoodsCategoryName != null && rule_CompanyGoodsCategoryName.Value.ToString().Trim() != "")
                    {
                        string name = rule_CompanyGoodsCategoryName.Value.ToString();
                        predicate = predicate.And(p => p.CompanyGoodsCategory.CompanyGoodsCategoryName.Contains(name));
                    }

                    var count = 0;

                    var list = _GoodsPurchasingContract.Entities.Where<GoodsPurchasing, int>(predicate, request.PageCondition, out count).ToList().OrderBy(c => c.CreatedTime).
                                Select(s => new
                                {
                                    s.Id,
                                    s.CompanyGoodsCategory.CompanyGoodsCategoryName,
                                    s.CompanyGoodsCategory.ImgAddress,
                                    s.TotalAmount,
                                    s.Quantity,
                                    AdminName = s.Admin == null ? "" : s.Admin.Member.RealName,
                                    s.IsDeleted,
                                    s.IsEnabled,
                                    s.CreatedTime,
                                    OperatorName = s.Operator.Member.RealName,
                                }).ToList();

                    return new GridData<object>(list, count, request.RequestInfo);
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, ex.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _GoodsPurchasingContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _GoodsPurchasingContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _GoodsPurchasingContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _GoodsPurchasingContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 物品选择页面
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult CompanyGoodsCategory(int type)
        {
            ViewBag.Type = type;
            return PartialView();
        }

        [HttpPost]
        public async Task<ActionResult> GetCompanyGoodsCategoryList()
        {
            try
            {
                GridRequest request = new GridRequest(Request);
                Expression<Func<CompanyGoodsCategory, bool>> predicate = FilterHelper.GetExpression<CompanyGoodsCategory>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    predicate = predicate.And(p => (p.IsUniqueness && (p.ParentId == 0 || p.ParentId == null)) || !p.IsUniqueness);

                    Func<ICollection<CompanyGoodsCategory>, List<CompanyGoodsCategory>> getTree = null;
                    getTree = (source) =>
                    {
                        var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                        List<CompanyGoodsCategory> tree = new List<CompanyGoodsCategory>();
                        foreach (var child in children)
                        {
                            tree.Add(child);
                            tree.AddRange(getTree(child.Children));
                        }
                        return tree;
                    };

                    var count = 0;

                    var list = _CompanyGoodsCategoryContract.Entities.Where<CompanyGoodsCategory, int>(predicate, request.PageCondition, out count).ToList().OrderBy(c => c.CreatedTime).
                                Select(s => new
                                {
                                    s.Id,
                                    s.CompanyGoodsCategoryName,
                                    s.IsUniqueness,
                                    s.ImgAddress,
                                    ParentId = s.ParentId ?? 0
                                }).ToList();

                    return new GridData<object>(list, count, request.RequestInfo);
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "网络异常，请稍候再试"));
            }
        }

    }
}

