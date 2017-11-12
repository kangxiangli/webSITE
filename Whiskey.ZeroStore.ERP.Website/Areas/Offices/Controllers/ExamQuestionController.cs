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
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class ExamQuestionController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ExamController));

        protected readonly IExamQuestionContract _examContract;
        protected readonly IAdministratorContract _AdministratorContract;
        protected readonly IStoreContract _StoreContract;


        public ExamQuestionController(
            IExamQuestionContract examContract,
            IStoreTypeContract _StoreTypeContract,
            IRoleContract _roleContract,
            IPartnerManageCheckContract _PartnerManageCheckContract


            , IAdministratorContract _AdministratorContract
            , IStoreContract _StoreContract
            , IDepartmentContract _DepartmentContract
            , IJobPositionContract _JobPositionContract
            , IStorageContract _StorageContract
            )
        {
            _examContract = examContract;


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
        public ActionResult Create(List<ExamQuestionDTO> entity)
        {
            if (!ModelState.IsValid)
            {
                return Json(OperationResult.Error("提交信息有误"));
            }
            if (entity == null || entity.Count <= 0)
            {
                return Json(OperationResult.Error("数据不可为空"));
            }
            var entities = new List<ExamQuestionEntity>();

            entity.ForEach(d =>
            {
                if (!string.IsNullOrEmpty(d.ImgUrl))
                {
                    d.ImgUrl = SaveImg(d.ImgUrl);
                }

                d.AnswerOptions.ForEach(o =>
                {
                    if (!string.IsNullOrEmpty(o.ImgUrl))
                    {
                        o.ImgUrl = SaveImg(o.ImgUrl);
                    }
                });
                var e = Mapper.DynamicMap<ExamQuestionEntity>(d);
                e.AnswerOptions = JsonHelper.ToJson(d.AnswerOptions);
                e.RightAnswer = string.Join(",", d.RightAnswer);
                entities.Add(e);
            });

            var result = _examContract.Insert(entities.ToArray());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string SaveImg(string imgBase64Str)
        {
            var mimeType = imgBase64Str.Substring(0, imgBase64Str.IndexOf(';'));
            var extention = mimeType.Substring(mimeType.IndexOf('/') + 1);
            var serverPath = "/Content/Images/Exam/" + Guid.NewGuid().ToString("N") + "." + extention;
            bool res = ImageHelper.SaveBase64Image(imgBase64Str, serverPath);
            return serverPath;
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(ExamQuestionDTO dto)
        {
            if (dto.RightAnswer == null || dto.RightAnswer.Length <= 0)
            {
                return Json(OperationResult.Error("请勾选正确答案"));
            }
            if (dto.RightAnswer.Any(s=>string.IsNullOrWhiteSpace(s)))
            {
                return Json(OperationResult.Error("请勾选正确答案"));
            }

            var entity = _examContract.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == dto.Id).FirstOrDefault();
            entity.Title = dto.Title;
            entity.Score = dto.Score;
            if (!string.IsNullOrEmpty(dto.ImgUrl) && dto.ImgUrl.IndexOf("base64") > 0)
            {
                entity.ImgUrl = SaveImg(dto.ImgUrl);
            }
            dto.AnswerOptions.ForEach(o =>
            {
                if (!string.IsNullOrEmpty(o.ImgUrl) && o.ImgUrl.IndexOf("base64") > 0)
                {
                    o.ImgUrl = SaveImg(o.ImgUrl);
                }

            });
            entity.AnswerOptions = JsonHelper.ToJson(dto.AnswerOptions);
            entity.RightAnswer = string.Join(",", dto.RightAnswer);

            var result = _examContract.Update(new ExamQuestionEntity[] { entity });


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
            var dto = Mapper.DynamicMap<ExamQuestionDTO>(result);
            dto.AnswerOptions = JsonHelper.FromJson<AnswerOptionEntry[]>(result.AnswerOptions).ToList();
            dto.RightAnswer = result.RightAnswer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            ViewBag.dto = JsonHelper.ToJson(dto);


            return PartialView();
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult View(int Id)
        {
            var result = _examContract.Entities.Where(e => e.Id == Id).FirstOrDefault();

            ViewBag.AnswerOptions = JsonHelper.FromJson<AnswerOptionEntry[]>(result.AnswerOptions).ToList();
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            var predicate = FilterHelper.GetExpression<ExamQuestionEntity>(request.FilterGroup);
            var count = 0;

            var list = _examContract.Entities.Where<ExamQuestionEntity, int>(predicate, request.PageCondition, out count)
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
                }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
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


        public ActionResult SetupQuestion(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }


    }
}