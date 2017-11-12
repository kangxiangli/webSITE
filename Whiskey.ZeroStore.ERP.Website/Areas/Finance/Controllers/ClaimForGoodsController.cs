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
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Web.Helper;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class ClaimForGoodsController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ClaimForGoodsController));

        protected readonly IClaimForGoodsContract _ClaimForGoodsContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IDepartmentContract _departmentContract;

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;

        public ClaimForGoodsController(
            IClaimForGoodsContract _ClaimForGoodsContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            ICompanyGoodsCategoryContract CompanyGoodsCategoryContract
            )
        {
            this._ClaimForGoodsContract = _ClaimForGoodsContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContract;
            this._CompanyGoodsCategoryContract = CompanyGoodsCategoryContract;
        }

        [Layout]
        public ActionResult Index()
        {
            #region 部门
            var li = CacheAccess.GetDepartmentListItem(_departmentContract, true, DepartmentTypeFlag.公司, DepartmentTypeFlag.店铺);

            ViewBag.Departments = li;
            #endregion

            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            #region 物品类型
            var companyGoodsCategoryTypeFlagList = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "请选择" } };

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryTypeFlag)))
            {
                companyGoodsCategoryTypeFlagList.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)value)) });
            }
            ViewBag.CompanyGoodsCategoryTypeFlagList = companyGoodsCategoryTypeFlagList;
            #endregion

            #region 部门
            var li = CacheAccess.GetDepartmentListItem(_departmentContract, true, DepartmentTypeFlag.公司, DepartmentTypeFlag.店铺);

            ViewBag.Departments = li;
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
        public ActionResult Create(ClaimForGoodsDto dto)
        {
            var result = _ClaimForGoodsContract.Apply(dto);
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
        public ActionResult Update(ClaimForGoodsDto dto)
        {
            var result = _ClaimForGoodsContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _ClaimForGoodsContract.Edit(Id);
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
            var result = _ClaimForGoodsContract.View(Id);
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

                var rule_Departments = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "Departments_Search");
                if (rule_Departments != null)
                {
                    request.FilterGroup.Rules.Remove(rule_Departments);
                }

                var rule_IsReturn = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "IsReturn_Search");
                if (rule_IsReturn != null)
                {
                    request.FilterGroup.Rules.Remove(rule_IsReturn);
                }

                var rule_ReturnTimeLimit = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "ReturnTimeLimit_Search");
                if (rule_ReturnTimeLimit != null)
                {
                    request.FilterGroup.Rules.Remove(rule_ReturnTimeLimit);
                }

                var rule_ReturnStatus = request.FilterGroup.Rules.FirstOrDefault(r => r.Field == "ReturnStatus_Search");
                if (rule_ReturnStatus != null)
                {
                    request.FilterGroup.Rules.Remove(rule_ReturnStatus);
                }

                Expression<Func<ClaimForGoods, bool>> predicate = FilterHelper.GetExpression<ClaimForGoods>(request.FilterGroup);

                var data = await Task.Run(() =>
                {
                    if (rule_Departments != null && rule_Departments.Value.ToString().Trim() != "")
                    {
                        int departmentId = 0;
                        int.TryParse(rule_Departments.Value.ToString(), out departmentId);
                        predicate = predicate.And(p => p.DepartmentId == departmentId);
                    }

                    if (rule_IsReturn != null && rule_IsReturn.Value.ToString().Trim() != "")
                    {
                        int isReturn = 0;
                        int.TryParse(rule_IsReturn.Value.ToString(), out isReturn);
                        predicate = predicate.And(p => p.IsReturn == (isReturn == 1 ? true : false));
                    }

                    if (rule_ReturnTimeLimit != null && rule_ReturnTimeLimit.Value.ToString().Trim() != "")
                    {
                        int returnTimeLimit = 0;
                        int.TryParse(rule_ReturnTimeLimit.Value.ToString(), out returnTimeLimit);
                        predicate = predicate.And(p => p.ReturnTimeLimit == (returnTimeLimit == 1 ? true : false));
                    }

                    if (rule_ReturnStatus != null && rule_ReturnStatus.Value.ToString().Trim() != "")
                    {
                        int returnStatus = 0;
                        int.TryParse(rule_ReturnStatus.Value.ToString(), out returnStatus);
                        predicate = predicate.And(p => p.ReturnStatus == returnStatus);
                    }

                    var count = 0;

                    var list = (from s in _ClaimForGoodsContract.Entities.Where<ClaimForGoods, int>(predicate, request.PageCondition, out count)
                                select new
                                {
                                    s.Id,
                                    CompanyGoodsCategoryName = s.CompanyGoodsCategory.CompanyGoodsCategoryName,
                                    ImgAddress = s.CompanyGoodsCategory.ImgAddress,
                                    ApplicantName = s.Applicant.Member.RealName,
                                    DepartmentName = s.Department.DepartmentName,
                                    s.IsReturn,
                                    s.ReturnStatus,
                                    s.EstimateReturnTime,
                                    s.ReturnTime,
                                    s.IsDeleted,
                                    s.IsEnabled,
                                    s.CreatedTime,
                                    OperatorName = s.Operator.Member.RealName,
                                    IsReminder = s.ReturnStatus == 1 && s.IsReturn && s.ReturnTimeLimit && s.EstimateReturnTime <= DateTime.Now ? 1 : 2
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
            var result = _ClaimForGoodsContract.DeleteOrRecovery(true, Id);
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
            var result = _ClaimForGoodsContract.DeleteOrRecovery(false, Id);
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
            var result = _ClaimForGoodsContract.EnableOrDisable(true, Id);
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
            var result = _ClaimForGoodsContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin(int departId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            //list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            var data = _administratorContract.Administrators.Where(a => a.DepartmentId == departId && !a.IsDeleted && a.IsEnabled).Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Member.RealName });
            list.AddRange(data);
            ViewBag.depList = list;
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.Where(r => r.Field == "RealName").FirstOrDefault();
            if (rule != null)
            {
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            if (rule != null)
            {
                string realName = rule.Value.ToString();
                predicate = predicate.And(a => a.Member.RealName.Contains(realName));
            }
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _administratorContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(a => new
                {
                    a.Id,
                    a.Member.RealName
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAdminListByDepartmentId(int DepartmentId)
        {
            if (DepartmentId > 0)
            {
                var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled && a.DepartmentId == DepartmentId).Select(m => new
                {
                    m.Id,
                    m.Member.RealName,
                }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(m => new
                {
                    m.Id,
                    m.Member.RealName,
                }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


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

        /// <summary>
        /// 归还提醒
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult ReturnReminder(int[] Id)
        {
            var result = _ClaimForGoodsContract.ReturnReminder(Id, sendNotificationAction);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 归还物品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult ReturnGoods(int[] Id)
        {
            var result = _ClaimForGoodsContract.ReturnGoods(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}

