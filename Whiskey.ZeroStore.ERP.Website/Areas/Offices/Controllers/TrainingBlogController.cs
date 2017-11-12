using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Data;
using System.Data.Entity;
using System.IO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class TrainingBlogController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ExamController));

        protected readonly ITrainintBlogContract _blogContract;
        protected readonly IExamContract _examContract;


        public TrainingBlogController(
            ITrainintBlogContract blogContract,
            IExamContract examContract)
        {
            _blogContract = blogContract;
            _examContract = examContract;
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
            return PartialView("Edit");
        }

        public ActionResult Upload()
        {
            var files = Request.Files;
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (files.Count == 0)
            {
                return Json(new
                {
                    success = 0,
                    message = "请选择图片",
                    url = string.Empty
                });
            }
            var acceptedExts = (".gif,.jpg,.jpeg,.png,.bmp").Split(',');
            var fileExt = Path.GetExtension(files[0].FileName).ToLower();
            if (String.IsNullOrEmpty(fileExt) || !acceptedExts.Contains(fileExt))
            {
                return Json(new
                {
                    success = 0,
                    message = "图片格式有误",
                    url = string.Empty
                });
            }
            var dir = "/Content/UploadFiles/Training";
            var dirPhysicalPath = Server.MapPath(dir);
            if (!Directory.Exists(dirPhysicalPath))
            {
                Directory.CreateDirectory(dirPhysicalPath);
            }
            string filePath = $"{dir}/{Guid.NewGuid().ToString("N")}{fileExt}";



            files[0].SaveAs(Server.MapPath(filePath));

            return Json(new
            {
                success = 1,
                message = string.Empty,
                url = filePath
            });
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Title,Content,IsTrain")] TrainingBlogEntity entity)
        {
            if (!ModelState.IsValid)
            {
                return View(entity);
            }
            var result = _blogContract.Insert(entity);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([Bind(Include = "Id,Title,Content,IsTrain")]TrainingBlogEntity dto)
        {
            var entity = _blogContract.Entities.FirstOrDefault(e => e.Id == dto.Id);
            entity.Title = dto.Title;
            entity.Content = dto.Content;
            entity.IsTrain = dto.IsTrain;
            var result = _blogContract.Update(new TrainingBlogEntity[] { entity });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _blogContract.Edit(Id);
            return PartialView("Edit", result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult View(int Id)
        {
            var result = _blogContract.Entities.Where(e => e.Id == Id).FirstOrDefault();
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string title, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _blogContract.Entities;

            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(e => e.Title.StartsWith(title));
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
                                e.Title,
                                e.Content,
                                e.CreatedTime,
                                MemberName = e.Operator.Member.MemberName,
                                IsChecked = false,
                                e.ExamId,
                                IsTrain = e.IsTrain ? "是" : "否"
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

        public class PageDto
        {
            public int pageIndex { get; set; }
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public int pageCount
            {
                get
                {
                    return (int)Math.Ceiling(totalCount * 1.0 / pageSize);
                }
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _blogContract.DeleteOrRecovery(true, Id);
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
            var result = _blogContract.DeleteOrRecovery(false, Id);
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
            var result = _blogContract.EnableOrDisable(true, Id);
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
            var result = _blogContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 绑定试卷
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="examId"></param>
        /// <returns></returns>
        public ActionResult SaveBind(int blogId, int examId)
        {
            var blog = _blogContract.View(blogId);
            var exam = _examContract.View(examId);
            if (blog == null || exam == null)
            {
                return Json(OperationResult.Error("信息未找到"));
            }

            blog.ExamId = examId;
            var res = _blogContract.Update(blog);
            return Json(res);
        }
    }
}