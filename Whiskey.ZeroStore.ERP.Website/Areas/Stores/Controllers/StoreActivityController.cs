using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{

    [License(CheckMode.Verify)]
    public class StoreActivityController : BaseController
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreController));

        protected readonly IStoreActivityContract _storeActivityContract;
        protected readonly IStoreContract _storeContract;

        protected readonly IModuleContract _moduleContract;

        protected readonly IAdministratorContract _adminContract;

        protected readonly IDepartmentContract _departContract;
        protected readonly IMemberContract _memberContract;

        protected readonly IMemberTypeContract _memberTypeContract;
        protected readonly IStorageContract _storageContract;


        public StoreActivityController(IStoreActivityContract storeActivityContract,
            IModuleContract moduleContract,
            IDepartmentContract departContract,
            IAdministratorContract adminContract, IMemberContract memberContract,
            IMemberTypeContract memberTypeContract, IStoreContract storeContract, IStorageContract storageContract)
        {
            _storeActivityContract = storeActivityContract;
            _moduleContract = moduleContract;
            _departContract = departContract;
            _adminContract = adminContract;
            _memberContract = memberContract;
            _memberTypeContract = memberTypeContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
           
            ViewBag.memberTypeDic = GetMemberTypeDic();
            ViewBag.storeDic = GetStoreDic();
            return View();
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var memberTypeList = _memberTypeContract.SelectList(string.Empty);
            memberTypeList.Add(new SelectListItem() { Text = "非会员", Value = "-1" });
            ViewBag.MemberType = memberTypeList;
            var activity = new StoreActivity();
            return PartialView(activity);
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(StoreActivity dto)
        {
            var storeIds = Request.Params["StoreIds"];
            var memberTypes = Request.Params["MemberTypes"];
            var onlyOncePerMember = Request.Params["OnlyOncePerMember"];
            if (onlyOncePerMember != null && onlyOncePerMember == "on")
            {
                dto.OnlyOncePerMember = true;
            }
            else
            {
                dto.OnlyOncePerMember = false;
            }
            if (dto.MinConsume < dto.DiscountMoney)
            {
                return Json(new OperationResult(OperationResultType.Error,"折扣金额不可高于最低消费金额"));
            }
            dto.StoreIds = storeIds;
            dto.MemberTypes = memberTypes;
            dto.OperatorId = AuthorityHelper.OperatorId;
            var result = _storeActivityContract.Insert(dto);
            return Json(result);
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(UpdateStoreActivityDto dto)
        {
            var entity = _storeActivityContract.StoreActivities.FirstOrDefault(s => s.Id == dto.Id);
            var onlyOncePerMember = Request.Params["OnlyOncePerMember"];
            if (onlyOncePerMember != null && onlyOncePerMember == "on")
            {
                entity.OnlyOncePerMember = true;
            }
            else
            {
                entity.OnlyOncePerMember = false;
            }
            if (dto.MinConsume < dto.DiscountMoney)
            {
                return Json(new OperationResult(OperationResultType.Error, "折扣金额不可高于最低消费金额"));
            }
            entity.ActivityName = dto.ActivityName;
            entity.DiscountMoney = dto.DiscountMoney;
            entity.EndDate = dto.EndDate;
            entity.StartDate = dto.StartDate;
            entity.StoreIds = string.Join(",", dto.StoreIds);
            entity.MinConsume = dto.MinConsume;
            entity.MemberTypes = string.Join(",", dto.MemberTypes);
            entity.Notes = dto.Notes;
            var result = _storeActivityContract.Update(entity);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var memberTypeList = _memberTypeContract.SelectList(string.Empty);
            memberTypeList.Add(new SelectListItem() { Text = "非会员", Value = "-1" });
            ViewBag.MemberType = memberTypeList;
            var result = _storeActivityContract.Edit(Id);
            if (!result.OnlyOncePerMember.HasValue)
            {
                result.OnlyOncePerMember = false;
            }
            return PartialView(result);
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _storeActivityContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List(int? storeId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<StoreActivity, bool>> predicate = FilterHelper.GetExpression<StoreActivity>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var query = _storeActivityContract.StoreActivities;
                if (storeId.HasValue)
                {
                    query = query.Where(s => s.StoreIds.Contains(storeId.Value.ToString()));
                }

                var list = query.Where<StoreActivity, int>(predicate, request.PageCondition, out count)
                .OrderByDescending(c => c.CreatedTime).Select(m => new
                {
                    m.Id,
                    m.ActivityName,
                    m.CreatedTime,
                    AdminName = m.Operator.Member.MemberName,
                    m.StartDate,
                    m.EndDate,
                    m.UpdatedTime,
                    m.MinConsume,
                    m.DiscountMoney,
                    m.MemberTypes,
                    m.StoreIds,
                    m.IsEnabled,
                    m.IsDeleted
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 移除数据

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _storeActivityContract.Remove(Id);
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
            var result = _storeActivityContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _storeActivityContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _storeActivityContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _storeActivityContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据

        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storeActivityContract.StoreActivities.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _storeActivityContract.StoreActivities.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private string GetMemberTypeDic()
        {
            return JsonConvert.SerializeObject(_memberTypeContract.MemberTypes.Where(m => !m.IsDeleted && m.IsEnabled).Select(m => new { m.Id, m.MemberTypeName }).ToList());

        }

        private string GetStoreDic()
        {
            return JsonConvert.SerializeObject(_storeContract.QueryAllStore().Select(m => new { m.Id, m.StoreName }).ToList());
        }



    }
}
