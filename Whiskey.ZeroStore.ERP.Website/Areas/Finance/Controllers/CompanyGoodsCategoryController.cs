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
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class CompanyGoodsCategoryController : BaseController
    {
        //protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CompanyGoodsCategoryController));

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;

        public CompanyGoodsCategoryController(
            ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract
            )
        {
            this._CompanyGoodsCategoryContract = _CompanyGoodsCategoryContract;
        }

        [Layout]
        public ActionResult Index()
        {
            #region 获取类型
            var companyGoodsCategoryTypeFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryTypeFlag)))
            {
                companyGoodsCategoryTypeFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryTypeFlagList = companyGoodsCategoryTypeFlagList;
            #endregion

            #region 获取状态
            var companyGoodsCategoryStatusFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryStatusFlag)))
            {
                companyGoodsCategoryStatusFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryStatusFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryStatusFlagList = companyGoodsCategoryStatusFlagList;
            #endregion
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

            #region 获取类别
            var companyGoodsCategoryList = new List<SelectListItem>();

            companyGoodsCategoryList.Add(new SelectListItem() { Value = "", Text = "请选择" });
            ViewBag.CompanyGoodsCategoryList = companyGoodsCategoryList;
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
        public ActionResult Create(CompanyGoodsCategoryDto dto)
        {
            var result = _CompanyGoodsCategoryContract.Insert(dto);
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
        public ActionResult Update(CompanyGoodsCategoryDto dto)
        {
            var result = _CompanyGoodsCategoryContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            #region 获取类型
            var companyGoodsCategoryTypeFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryTypeFlag)))
            {
                companyGoodsCategoryTypeFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryTypeFlagList = companyGoodsCategoryTypeFlagList;
            #endregion

            #region 获取状态
            var companyGoodsCategoryStatusFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryStatusFlag)))
            {
                if (Convert.ToInt32(value) == 1)
                {
                    continue;
                }
                companyGoodsCategoryStatusFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryStatusFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryStatusFlagList = companyGoodsCategoryStatusFlagList;
            #endregion

            var result = _CompanyGoodsCategoryContract.Edit(Id);

            ViewBag.ParentName = result.ParentId == null || result.ParentId == 0 ? "" : _CompanyGoodsCategoryContract.Edit(result.ParentId ?? 0).CompanyGoodsCategoryName;
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
            var result = _CompanyGoodsCategoryContract.View(Id);
            ViewBag.TypeName = GetTypeName(result.Type);
            ViewBag.StatusName = GetStatusName(result.Status, result.IsUniqueness, result.ParentId ?? 0);
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

                var rule_Type = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "Type_Search");
                if (rule_Type != null)
                {
                    request.FilterGroup.Rules.Remove(rule_Type);
                }

                var rule_IsUniqueness = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "IsUniqueness_Search");
                if (rule_IsUniqueness != null)
                {
                    request.FilterGroup.Rules.Remove(rule_IsUniqueness);
                }

                var rule_CompanyGoodsCategoryName = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "CompanyGoodsCategoryName_Search");
                if (rule_CompanyGoodsCategoryName != null)
                {
                    request.FilterGroup.Rules.Remove(rule_CompanyGoodsCategoryName);
                }

                var rule_Status = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "Status_Search");
                if (rule_Status != null)
                {
                    request.FilterGroup.Rules.Remove(rule_Status);
                }

                Expression<Func<CompanyGoodsCategory, bool>> predicate = FilterHelper.GetExpression<CompanyGoodsCategory>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    if (rule_Type != null && rule_Type.Value.ToString().Trim() != "")
                    {
                        int type = 0;
                        int.TryParse(rule_Type.Value.ToString(), out type);
                        predicate = predicate.And(p => p.Type == type);
                    }

                    if (rule_IsUniqueness != null && rule_IsUniqueness.Value.ToString().Trim() != "")
                    {
                        int isUniqueness = 0;
                        int.TryParse(rule_IsUniqueness.Value.ToString(), out isUniqueness);
                        predicate = predicate.And(p => p.IsUniqueness == (isUniqueness == 1 ? true : false));
                    }

                    if (rule_CompanyGoodsCategoryName != null && rule_CompanyGoodsCategoryName.Value.ToString().Trim() != "")
                    {
                        string name = rule_CompanyGoodsCategoryName.Value.ToString();
                        predicate = predicate.And(p => p.CompanyGoodsCategoryName.Contains(name));
                    }

                    if (rule_Status != null && rule_Status.Value.ToString().Trim() != "")
                    {
                        int status = 0;
                        int.TryParse(rule_Status.Value.ToString(), out status);
                        if (status == (int)CompanyGoodsCategoryStatusFlag.Free)
                        {
                            predicate = predicate.And(p => (p.IsUniqueness && p.Status == status) || (!p.IsUniqueness && p.TotalQuantity > p.UsedQuantity));
                        }
                        else if (status == (int)CompanyGoodsCategoryStatusFlag.Use)
                        {
                            predicate = predicate.And(p => (p.IsUniqueness && p.Status == status) || (!p.IsUniqueness && p.UsedQuantity > 0));
                        }
                        else
                        {
                            predicate = predicate.And(p => p.Status == status);
                        }
                    }

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
                                    s.ImgAddress,
                                    IsUniqueness = s.IsUniqueness ? "是" : "否",
                                    TypeName = GetTypeName(s.Type),
                                    TotalQuantity = s.TotalQuantity ?? 0,
                                    UsedQuantity = s.UsedQuantity ?? 0,
                                    s.UniqueIdentification,
                                    StatusName = GetStatusName(s.Status, s.IsUniqueness, s.ParentId ?? 0),
                                    Price = s.Price ?? 0,
                                    ParentId = s.ParentId ?? 0,
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
            var result = _CompanyGoodsCategoryContract.DeleteOrRecovery(true, Id);
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
            var result = _CompanyGoodsCategoryContract.DeleteOrRecovery(false, Id);
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
            var result = _CompanyGoodsCategoryContract.EnableOrDisable(true, Id);
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
            var result = _CompanyGoodsCategoryContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

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
                int count = 0;
                GridRequest request = new GridRequest(Request);
                Expression<Func<CompanyGoodsCategory, bool>> predicate = FilterHelper.GetExpression<CompanyGoodsCategory>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    predicate = predicate.And(c => c.ParentId == null || c.ParentId == 0);
                    var list = _CompanyGoodsCategoryContract.Entities.Where<CompanyGoodsCategory, int>(predicate, request.PageCondition, out count).Select(x => new
                    {
                        x.Id,
                        x.CompanyGoodsCategoryName,
                        x.IsUniqueness,
                        x.ImgAddress
                    });
                    return new GridData<object>(list, count, request.RequestInfo);
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "网络异常，请稍候再试"));
            }
        }

        /// <summary>
        /// 获取类型名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeName(int type)
        {
            try
            {
                return EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)type);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 获取状态名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetStatusName(int type, bool isUniqueness, int parentId)
        {
            try
            {
                if (!isUniqueness || isUniqueness && parentId == 0)
                {
                    return "";
                }
                return EnumHelper.GetValue<string>((CompanyGoodsCategoryStatusFlag)type);
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}

