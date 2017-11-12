using System;
using System.IO;
using System.Web;
using Whiskey.Web.Helper;
using Antlr3.ST;
using Antlr3.ST.Language;
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
using Whiskey.Utility.Extensions;
using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class OrderFoodController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OrderFoodController));

        protected readonly IOrderFoodContract _OrderFoodContract;
        protected readonly IAdministratorContract _AdministratorContract;
        protected readonly IConfigureContract _configureContract;

        public OrderFoodController(
            IOrderFoodContract _OrderFoodContract,
            IAdministratorContract _AdministratorContract,
            IConfigureContract _configureContract
            )
        {
            this._OrderFoodContract = _OrderFoodContract;
            this._AdministratorContract = _AdministratorContract;
            this._configureContract = _configureContract;
        }

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
        public ActionResult Create(OrderFoodDto dto)
        {
            var result = _OrderFoodContract.Insert(dto);
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
        public ActionResult Update(OrderFoodDto dto)
        {
            var result = _OrderFoodContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _OrderFoodContract.Edit(Id);

            ViewBag.Admins = _AdministratorContract.Administrators.Where(w => w.IsEnabled && !w.IsDeleted && result.AdminIds.Contains(w.Id)).Select(s => new SelectListItem
            {
                Text = s.Member.MemberName,
                Value = s.Id + "",
                Selected = true
            }).ToList();

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
            var result = _OrderFoodContract.View(Id);
            var listAdmins = result.Admins.Where(w => w.IsEnabled && !w.IsDeleted);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<OrderFood, bool>> predicate = FilterHelper.GetExpression<OrderFood>(request.FilterGroup);
            var count = 0;

            var list = (from s in _OrderFoodContract.Entities.Where<OrderFood, int>(predicate, request.PageCondition, out count)
                        let depa = s.Admins.Where(w => w.IsEnabled && !w.IsDeleted)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.CreatedTime,
                            OperatorName = s.Operator.Member.RealName,
                            s.smsIsSend,
                            WL = depa.Count(c => c.DepartmentId == 7),//网络部
                            CC = depa.Count(c => c.DepartmentId == 8),//仓储部
                            YY = depa.Count(c => c.DepartmentId == 9),//运营部
                            HG = depa.Count(c => c.DepartmentId == 10),//合规部
                            RS = depa.Count(c => c.DepartmentId == 11),//人事部
                            CW = depa.Count(c => c.DepartmentId == 12),//财务部
                            BJ = depa.Count(c => c.DepartmentId == 13),//编辑部
                            CP = depa.Count(c => c.DepartmentId == 1016),//产品部
                            SC = depa.Count(c => c.DepartmentId == 1025),//市场部
                            XZ = depa.Count(c => c.DepartmentId == 1027),//行政部

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

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
            var result = _OrderFoodContract.DeleteOrRecovery(true, Id);
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
            var result = _OrderFoodContract.DeleteOrRecovery(false, Id);
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
            var result = _OrderFoodContract.EnableOrDisable(true, Id);
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
            var result = _OrderFoodContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export()
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            GridRequest request = new GridRequest(Request);
            Expression<Func<OrderFood, bool>> predicate = FilterHelper.GetExpression<OrderFood>(request.FilterGroup);
            var query = _OrderFoodContract.Entities.Where(predicate);
            var list = (from s in query
                        let depa = s.Admins.Where(w => w.IsEnabled && !w.IsDeleted)
                        select new
                        {
                            s.UpdatedTime,
                            OperatorName = s.Operator.Member.RealName,
                            WL = depa.Count(c => c.DepartmentId == 7),//网络部
                            CC = depa.Count(c => c.DepartmentId == 8),//仓储部
                            YY = depa.Count(c => c.DepartmentId == 9),//运营部
                            HG = depa.Count(c => c.DepartmentId == 10),//合规部
                            RS = depa.Count(c => c.DepartmentId == 11),//人事部
                            CW = depa.Count(c => c.DepartmentId == 12),//财务部
                            BJ = depa.Count(c => c.DepartmentId == 13),//编辑部
                            CP = depa.Count(c => c.DepartmentId == 1016),//产品部
                            SC = depa.Count(c => c.DepartmentId == 1025),//市场部
                            XZ = depa.Count(c => c.DepartmentId == 1027),//行政部
                        }).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "订餐预约管理");
        }

        public ActionResult ViewAdmin()
        {
            return PartialView();
        }

        public JsonResult ViewAdminList(int Id, int DepId)
        {
            var query = _OrderFoodContract.Entities.Where(w => w.Id == Id).SelectMany(s => s.Admins).Where(w => w.IsEnabled && !w.IsDeleted && w.DepartmentId == DepId);
            var data = from s in query
                       select new
                       {
                           AdminId = s.Id,
                           s.Member.MemberName,
                           s.Member.MobilePhone,
                           Gender = s.Member.Gender == 1 ? "男" : "女",
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除某个人的预约订餐信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        public JsonResult DeleteAdmin(int Id, int AdminId)
        {
            var mod = _OrderFoodContract.Entities.FirstOrDefault(w => w.Id == Id);
            var modAdmin = mod.Admins.FirstOrDefault(f => f.Id == AdminId);
            if (modAdmin.IsNotNull())
            {
                mod.Admins.Remove(modAdmin);
            }
            var data = _OrderFoodContract.Update(mod);
            return Json(data);
        }

        #region 选择会员

        public ActionResult VAdmin()
        {
            return PartialView();
        }

        public ActionResult AdminList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var count = 0;
            var depids = new int[] { 7, 8, 9, 10, 11, 12, 13, 1016, 1025, 1027 };
            var list = (from s in _AdministratorContract.Administrators.Where(w => depids.Contains(w.DepartmentId.Value)).Where<Administrator, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.Member.MemberName,
                            s.Member.MobilePhone,
                            //s.Member.UserPhoto,
                            Gender = s.Member.Gender == 1 ? "男" : "女",
                            s.Member.RealName,
                            s.CreatedTime,
                            s.Department.DepartmentName,

                        }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 配置通知人

        public ActionResult SendPeoConfig()
        {
            Utility.XmlHelper helper = new Utility.XmlHelper("OrderFood", "SendPeoConfig");
            ViewBag.Phones = helper.GetElement("Phones")?.Value ?? "";
            ViewBag.SendHour = helper.GetElement("SendHour")?.Value ?? "9";
            ViewBag.SendMinute = helper.GetElement("SendMinute")?.Value ?? "0";
            return PartialView();
        }

        [HttpPost]
        public ActionResult SendPeoConfig(string Phones, string SendHour, string SendMinute)
        {
            var result = OperationHelper.Try((opera) =>
            {
                Utility.XmlHelper helper = new Utility.XmlHelper("OrderFood", "SendPeoConfig", true);
                helper.ModifyElement("Phones", Phones);
                helper.ModifyElement("SendHour", SendHour);
                helper.ModifyElement("SendMinute", SendMinute);
                return OperationHelper.ReturnOperationResult(true, opera);
            }, Operation.Update);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取通知人手机号
        /// </summary>
        /// <returns></returns>
        private List<string> GetSendPeoAdminId()
        {
            Utility.XmlHelper helper = new Utility.XmlHelper("OrderFood", "SendPeoConfig");
            var xmlphones = helper.GetElement("Phones");
            var listphones = (xmlphones?.Value ?? "").Split(new string[] { ",", "，", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.IsMobileNumber(true)).ToList();
            return listphones;
        }

        #endregion
    }
}

