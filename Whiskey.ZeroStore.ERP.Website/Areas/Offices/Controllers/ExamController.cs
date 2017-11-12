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
    public class ExamController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ExamController));

        protected readonly IExamContract _examContract;
        protected readonly IExamQuestionContract _examQuestionContract;
        protected readonly IAdministratorContract _AdministratorContract;
        protected readonly IStoreContract _StoreContract;


        public ExamController(
            IExamContract examContract,
            IStoreTypeContract _StoreTypeContract,
            IRoleContract _roleContract,
            IPartnerManageCheckContract _PartnerManageCheckContract

            , IExamQuestionContract examQuestionContract
            , IAdministratorContract _AdministratorContract
            , IStoreContract _StoreContract
            , IDepartmentContract _DepartmentContract
            , IJobPositionContract _JobPositionContract
            , IStorageContract _StorageContract
            )
        {
            _examContract = examContract;
            _examQuestionContract = examQuestionContract;

            this._AdministratorContract = _AdministratorContract;
            this._StoreContract = _StoreContract;

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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Name,PassLine,RetryCostScore,MinutesLimit,RewardMemberScore")] ExamEntity entity)
        {
            if (!ModelState.IsValid)
            {
                return View(entity);
            }
            var result = _examContract.Insert(entity);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([Bind(Include = "Id,Name,PassLine,RetryCostScore,MinutesLimit,RewardMemberScore")]ExamEntity dto)
        {
            var entity = _examContract.Entities.FirstOrDefault(e => e.Id == dto.Id);
            entity.Name = dto.Name;
            entity.PassLine = dto.PassLine;
            entity.RetryCostScore = dto.RetryCostScore;
            entity.RewardMemberScore = dto.RewardMemberScore;
            var result = _examContract.Update(new ExamEntity[] { entity });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _examContract.Edit(Id);
            return PartialView(result);
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

            var query = _examContract.Entities;

            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.Name.StartsWith(name));
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
                                e.Name,
                                e.TotalScore,
                                e.RetryCostScore,
                                e.QuestionCount,
                                e.Operator.Member.MemberName,
                                e.PassLine,
                                e.UpdatedTime,
                                e.CreatedTime,
                                IsChecked = false,
                                e.RewardMemberScore,
                                QuestionIds = e.Questions.Select(q => q.Id).ToList()
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

        [HttpGet]
        public ActionResult GetQuestions(int pageIndex = 1, int pageSize = 10)
        {

            //var ids = _examContract.Entities.Where(e => e.Id == id).SelectMany(e => e.Questions.Select(q => q.Id)).ToList();

            var query = _examQuestionContract.Entities;
            var list = query.OrderByDescending(e => e.UpdatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.IsDeleted,
                    e.IsEnabled,
                    e.Title,
                    e.IsMulti,
                    e.AnswerOptions,
                    e.Score,
                    e.Operator.Member.MemberName,
                    e.UpdatedTime,
                    e.CreatedTime
                }).ToList()
               .Select(e => new
               {
                   e.Id,
                   e.IsDeleted,
                   e.IsEnabled,
                   e.Title,
                   e.IsMulti,
                   e.AnswerOptions,
                   e.Score,
                   e.MemberName,
                   e.UpdatedTime,
                   e.CreatedTime,
                   IsChecked = false
               }).ToList();


            var res = new
            {
                pageInfo = new PageDto { pageIndex = pageIndex, pageSize = pageSize, totalCount = query.Count() },
                pageData = list
            };

            return Json(new OperationResult(OperationResultType.Success, string.Empty, res), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult SetupQuestion(int examId, params int[] questionIds)
        {
            var entity = _examContract.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == examId).Include(e => e.Questions).FirstOrDefault();
            if (entity == null)
            {
                return Json(OperationResult.Error("数据不存在"));
            }

            List<ExamQuestionEntity> questions;
            if (questionIds == null || questionIds.Length <= 0)
            {
                questions = new List<ExamQuestionEntity>();
                entity.TotalScore = 0;
                entity.QuestionCount = 0;
            }
            else
            {
                questions = _examQuestionContract.Entities.Where(q => !q.IsDeleted && q.IsEnabled && questionIds.Contains(q.Id)).ToList();
                if (!questions.Any())
                {
                    return Json(OperationResult.Error("数据不存在"));
                }
                entity.Questions.Clear();
                foreach (var item in questions)
                {
                    entity.Questions.Add(item);
                }
                entity.QuestionCount = entity.Questions.Count;
                entity.TotalScore = entity.Questions.Sum(q => q.Score);
            }


            if (entity.TotalScore < entity.PassLine)
            {
                return Json(OperationResult.Error("总分不能低于及格线"));
            }
            var res = _examContract.Update(new ExamEntity[] { entity });
            return Json(res);

        }


    }
}