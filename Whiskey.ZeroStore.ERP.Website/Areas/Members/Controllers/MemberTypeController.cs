using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using Whiskey.ZeroStore.ERP.Transfers;
using System.Text.RegularExpressions;
using System.Text;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{

    [License(CheckMode.Verify)]
    public class MemberTypeController : BaseController
    {

        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberTypeController));

        protected readonly IMemberTypeContract _memberTypeContract;

        public MemberTypeController(IMemberTypeContract memberTypeContract)
        {
            _memberTypeContract = memberTypeContract;
        }
        #endregion

        #region 初始化操作界面
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberType, bool>> predicate = FilterHelper.GetExpression<MemberType>(request.FilterGroup);
            //总页码
            int count = 0;
            var data = await Task.Run(() =>
            {
                var list = _memberTypeContract.MemberTypes.Where<MemberType, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.MemberTypeName,
                    m.MemberTypeDiscount,
                    m.UpdatedTime,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Operator.Member.MemberName
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.Discount = StaticHelper.DiscountList("选择折扣");
            return PartialView();
        }
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(MemberTypeDto dto)
        {
            var result = _memberTypeContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            MemberTypeDto dto = _memberTypeContract.Edit(Id);
            ViewBag.Discount = StaticHelper.DiscountList("选择折扣");
            return PartialView(dto);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(MemberTypeDto dto)
        {
            var result = _memberTypeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            MemberType memberType = _memberTypeContract.View(Id);
            return PartialView(memberType);
        }
        #endregion

        #region 移除和恢复数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _memberTypeContract.Remove(Id);
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
            var result = _memberTypeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _memberTypeContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用和禁用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _memberTypeContract.Enable(Id);
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
            var result = _memberTypeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

       
        public ActionResult EditConfig()
        {
            var id = RedisCacheHelper.Get<int?>(RedisCacheHelper.EnterpriseMemberTypeId);
            ViewBag.MemberTypeId = id.HasValue ? id.Value.ToString() : string.Empty;
            return PartialView();
        }

        [HttpPost]
        public ActionResult EditConfig(int memberTypeId)
        {
            var entity = _memberTypeContract.Edit(memberTypeId);
            if (entity == null)
            {
                return Json(OperationResult.Error("会员类型不存在"));
            }
            var res = RedisCacheHelper.Set(RedisCacheHelper.EnterpriseMemberTypeId, memberTypeId);
            if (!res)
            {
                return Json(OperationResult.Error("绑定失败"));
            }
            return Json(OperationResult.OK());
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberTypeList(string name, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId.Value;
            var query = _memberTypeContract.MemberTypes.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.MemberTypeName.Contains(name));
            }


            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CreatedTime,
                                e.UpdatedTime,
                                e.MemberTypeName,
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }

    }
}