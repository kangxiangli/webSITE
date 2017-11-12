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
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Helper;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberActivityController : BaseController
    {
        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberActivityController));
        //声明业务层操作对象
        protected readonly IMemberActivityContract _memberActivityContract;

        protected readonly IMemberTypeContract _memberTypeContract;
        protected readonly IStoreContract _storeContract;

        //构造函数-初始化业务层操作对象
        public MemberActivityController(IMemberActivityContract memberActivityContract,
            IMemberTypeContract memberTypeContract,
            IStoreContract storeContract)
        {
            _memberActivityContract = memberActivityContract;
            _memberTypeContract = memberTypeContract;
            _storeContract = storeContract;
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
            ViewBag.MemberType = _memberTypeContract.SelectList(string.Empty);
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
        public ActionResult Create(MemberActivityDto dto, params int[] storeIds)
        {
            if (dto.IsAllStore)
            {
                dto.StoreIds = string.Empty;
            }
            else
            {
                if (storeIds != null && storeIds.Length == 0)
                {
                    return Json(OperationResult.Error("请选择活动店铺"));
                }
                dto.StoreIds = string.Join(",", storeIds);
            }

            string strId = Request["MemberTypeId"];
            if (!string.IsNullOrEmpty(strId))
            {
                string[] arrId = strId.Split(",");
                dto.MemberTypes = _memberTypeContract.MemberTypes.Where(x => arrId.Where(k => k == x.Id.ToString()).Count() > 0).ToList();
            }
            var result = _memberActivityContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 修改数据

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            MemberActivityDto memberActDto = _memberActivityContract.Edit(Id);
            List<MemberType> listMember = memberActDto.MemberTypes.ToList();
            List<SelectListItem> listSel = _memberTypeContract.SelectList(string.Empty);
            if (listMember != null && listMember.Count() > 0)
            {
                foreach (var selItem in listSel)
                {
                    int count = listMember.Where(x => x.Id.ToString() == selItem.Value).Count();
                    if (count > 0)
                    {
                        selItem.Selected = true;
                    }

                }
            }
            ViewBag.MemberType = listSel;
            return PartialView(memberActDto);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MemberActivityDto dto, params int[] storeIds)
        {
            if (dto.IsAllStore)
            {
                dto.StoreIds = string.Empty;
            }
            else
            {
                if (storeIds != null && storeIds.Length == 0)
                {
                    return Json(OperationResult.Error("请选择活动店铺"));
                }
                dto.StoreIds = string.Join(",", storeIds);
            }
            string strId = Request["MemberTypeId"];
            if (!string.IsNullOrEmpty(strId))
            {
                string[] arrId = strId.Split(",");
                IQueryable<MemberType> list = _memberTypeContract.MemberTypes.Where(x => arrId.Where(k => k == x.Id.ToString()).Count() > 0);
                var entity = _memberActivityContract.MemberActivitys.Where(x => x.Id == dto.Id).FirstOrDefault();
                TryUpdateModel(entity);
                dto.MemberTypes = list.ToList();
            }
            var result = _memberActivityContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _memberActivityContract.View(Id);
            return PartialView(result);
        }

        #endregion

        #region 获取数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberActivity, bool>> predicate = FilterHelper.GetExpression<MemberActivity>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberActivityContract.MemberActivitys.Where<MemberActivity, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.ActivityName,
                    Name = m.MemberTypes.Select(x => x.MemberTypeName),
                    m.Price,
                    m.RewardMoney,
                    m.Score,
                    m.StartDate,
                    m.EndDate,
                    m.IsForever,
                    m.Notes,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.StoreIds,
                    m.IsAllStore
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
            var result = _memberActivityContract.Remove(Id);
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
            var result = _memberActivityContract.Delete(Id);
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
            var result = _memberActivityContract.Recovery(Id);
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
            var result = _memberActivityContract.Enable(Id);
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
            var result = _memberActivityContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActivityStore(int Id)
        {
            var activity = _memberActivityContract.View(Id);
            if (activity == null)
            {
                return Json(OperationResult.Error("未找到活动"));
            }
            var storeIds = new List<int>();
            if (activity.IsAllStore)
            {
                storeIds = _storeContract.QueryAllStore().Select(s => s.Id).ToList();
            }
            else
            {
                storeIds = activity.StoreIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            }
            ViewBag.Stores = _storeContract.QueryAllStore().Where(s => storeIds.Contains(s.Id)).Select(s => new SelectListItem()
            {
                Text = s.StoreName,
                Value = s.Id.ToString()
            }).ToList();
            return PartialView();
        }

        public ActionResult ViewMemberType(int Id)
        {
            var activity = _memberActivityContract.View(Id);
            if (activity == null)
            {
                return Json(OperationResult.Error("未找到活动"));
            }
            var memberTypes = activity.MemberTypes.Select(m => new
            {
                m.Id,
                m.MemberTypeName
            }).ToList();

            ViewBag.MemberTypes = memberTypes.Select(s => new SelectListItem()
            {
                Text = s.MemberTypeName,
                Value = s.Id.ToString()
            }).ToList();
            return PartialView();
        }
        #endregion
    }
}