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
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Collections.Generic;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class ExamRecordController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ExamRecordController));

        protected readonly IExamContract _examContract;
        protected readonly IExamQuestionContract _examQuestionContract;
        protected readonly IAdministratorContract _AdministratorContract;
        protected readonly IStoreContract _StoreContract;
        protected readonly IExamRecordContract _examRecordContract;


        public ExamRecordController(
            IExamContract examContract,
            IExamRecordContract examRecordContract,
            IExamQuestionContract examQuestionContract,
            IAdministratorContract _AdministratorContract,
            IStoreContract _StoreContract

            )
        {
            _examContract = examContract;
            _examQuestionContract = examQuestionContract;
            _examRecordContract = examRecordContract;
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
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult View(int Id)
        {
            var result = _examContract.Entities.Where(e => e.Id == Id).Include(e => e.Questions).FirstOrDefault();
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string name, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _examRecordContract.Entities;
            query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.Exam.Name.StartsWith(name));
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
                                e.Admin.Member.MemberName,
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.Exam.Name,
                                e.GetScore,
                                e.IsPass,
                                e.CreatedTime,
                                e.UpdatedTime,
                                State = e.State.ToString(),
                                IsChecked = false
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

        

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _examContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _examContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _examContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _examContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



    }
}