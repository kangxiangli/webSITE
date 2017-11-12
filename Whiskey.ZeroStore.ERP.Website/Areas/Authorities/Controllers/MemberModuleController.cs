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
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberModuleController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberModuleController));

        protected readonly IMemberModuleContract _MemberModuleContract;
        protected readonly IMemberRoleContract _MemberRoleContract;

        public MemberModuleController(
            IMemberModuleContract _MemberModuleContract
            , IMemberRoleContract _MemberRoleContract
            )
        {
            this._MemberModuleContract = _MemberModuleContract;
            this._MemberRoleContract = _MemberRoleContract;
        }

        [Layout]
        public ActionResult Index()
        {
            ViewBag.ListModule = _MemberModuleContract.SelectList(true);
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.ListModule = _MemberModuleContract.SelectList(true);
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
        public ActionResult Create(MemberModuleDto dto)
        {
            var result = _MemberModuleContract.Insert(dto);
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
        public ActionResult Update(MemberModuleDto dto)
        {
            var result = _MemberModuleContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _MemberModuleContract.Edit(Id);
            ViewBag.ListModule = _MemberModuleContract.SelectList(true);
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
            var result = _MemberModuleContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? ParentId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberModule, bool>> predicate = FilterHelper.GetExpression<MemberModule>(request.FilterGroup);
            var count = 0;

            List<dynamic> listData = new List<dynamic>();

            var queryOrg = _MemberModuleContract.Entities;
            var query = _MemberModuleContract.Entities.Where(predicate);
            var queryPar = _MemberModuleContract.Entities.Where(predicate);
            if (ParentId.HasValue)
            {
                query = query.Where(w => w.ParentId == ParentId);
                queryPar = queryPar.Where(w => w.Id == ParentId);
            }
            else {
                query = query.Where(w => w.ParentId == null);
            }

            #region 选择器

            Expression<Func<MemberModule, dynamic>> selector = s => new
            {
                s.Id,
                s.ParentId,
                s.IsDeleted,
                s.IsEnabled,
                s.ModuleName,
                s.Icon,
                s.Description,
                s.PageUrl,
                s.PageArea,
                s.PageController,
                s.Sequence,
                s.UpdatedTime,
                s.CreatedTime,
                AdminName = s.Operator.Member.RealName,
            };

            #endregion

            if (!ParentId.HasValue)
            {
                #region 根据父级模块分页

                count = query.Count();

                var list = query.OrderBy(x => x.Sequence).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(selector).ToList();
                foreach (var item in list)
                {
                    listData.Add(item);
                    int pid = item.Id;

                    var child = queryPar.Where(w => w.ParentId == pid).OrderBy(o => o.Sequence).Select(selector).ToList();
                    listData.AddRange(child);
                }
                #endregion
            }
            else {

                #region 根据ParentId分页

                count = query.Count();
                if (count > 0)
                {
                    var parent = queryOrg.Where(w => w.Id == ParentId).Select(selector).FirstOrDefault();
                    if (parent != null)
                    {
                        listData.Add(parent);
                        var child = query.OrderBy(x => x.Sequence).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(selector).ToList();
                        listData.AddRange(child);
                    }
                }

                #endregion
            }

            var data = new GridData<object>(listData, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
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
            var result = _MemberModuleContract.DeleteOrRecovery(true, Id);
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
            var result = _MemberModuleContract.DeleteOrRecovery(false, Id);
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
            var result = _MemberModuleContract.EnableOrDisable(true, Id);
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
            var result = _MemberModuleContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 排序设置
        public JsonResult SetSeq(int Id, int SequenceType)
        {
            var oper = _MemberModuleContract.SetSeq(Id, SequenceType);
            return Json(oper);
        }
        #endregion

        #region 获取模块树

        public ActionResult PermissCell()
        {
            return PartialView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Rid">会员角色Id</param>
        /// <returns></returns>
        public ActionResult GetTree(int? Rid)
        {
            var model = new List<MemberModule>();

            var list = _MemberModuleContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).ToList();
            var retree = new Models.RightTree()
            {
                id = "0",
                _checked = false,
                text = "请选择",
                url = "",
            };
            var checkids = new List<int>();
            if (Rid.HasValue)
            {
                var mod = _MemberRoleContract.Entities.FirstOrDefault(f => f.Id == Rid);
                if (mod.IsNotNull())
                {
                    checkids = mod.MemberModules.Select(s => s.Id).ToList();
                }
            }

            retree = GetNode(retree, list, checkids);
            retree._checked = retree.children.Any(a => a._checked);


            Models.ResJson json = new Models.ResJson()
            {
                msg = "测试",
                obj = retree,
                success = true,
                type = "json"
            };
            return Json(json);
        }

        private Models.RightTree GetNode(Models.RightTree parent, List<MemberModule> children, List<int> checkids)
        {
            foreach (var item in children)
            {
                var par = new Models.RightTree()
                {
                    id = item.Id + "",
                    text = item.ModuleName,
                    _checked = checkids.Exists(a => a == item.Id),
                    _isShow = false,
                    url = "",
                };

                var child = GetNode(par, item.Children.ToList(), checkids);

                parent.children.Add(child);
            }
            return parent;
        }

        #endregion

    }
}

