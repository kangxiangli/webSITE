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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    //[License(CheckMode.Verify)]
    public class CircleController : Controller
    {
        #region 声业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CircleController));

        protected readonly ICircleContract _circleContract;

        protected readonly ITopicContract _topicContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IMemberContract _memberContract;
        public CircleController(ICircleContract circleContract,
            ITopicContract topicContract,
            ICommentContract commentContract,
            IMemberContract memberContract)
        {
            _circleContract = circleContract;
            _topicContract = topicContract;
            _commentContract = commentContract;
            _memberContract = memberContract;
        }
        #endregion

        #region 初始化界面
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
            Expression<Func<Circle, bool>> predicate = FilterHelper.GetExpression<Circle>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _circleContract.Circles.Where<Circle, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.CircleName,
                    x.IconPath,
                    MemberQuantity = x.Members.Count(),
                    TopicQuantity =  x.Topics.Count(),
                    x.IsDeleted,
                    x.IsEnabled,                     
                });
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
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(CircleDto dto)
        {
            OperationResult oper = _circleContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            CircleDto dto = _circleContract.Edit(Id);
            return PartialView(dto);
        }

        /// <summary>
        /// 提交修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(CircleDto dto)
        {
            OperationResult oper = _circleContract.Update(dto);
            return Json(oper);
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
            var result = _circleContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据
        ///// <summary>
        ///// 删除数据
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //[Log]
        //[HttpPost]
        //public ActionResult Delete(int[] Id)
        //{
        //    var result = ""//_circleContract.Delete(Id);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
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
            var result = _circleContract.Recovery(Id);
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
            var result = _circleContract.Enable(Id);
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
            var result = _circleContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印和导出数据
        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _circleContract.Circles.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _circleContract.Circles.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            DateTime current = DateTime.Now;
            string savePath = ConfigurationHelper.GetAppSetting("CirclePath");
            string strDate = current.Year.ToString()+current.Month.ToString()+ DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
            savePath = savePath + strDate;
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                result = FileHelper.SaveUpload(file[i].InputStream, savePath);
            }
            if (result)
            {                
                return Json(new { ResultType = OperationResultType.Success, Path = savePath }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 查看详情

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _circleContract.View(Id);
            return PartialView(result);
        }

        #endregion

        #region 获取圈子里的会员
        /// <summary>
        /// 获取圈子里的会员
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Members(int Id)
        {
            ViewBag.CircleId = Id;
            return PartialView();
        }
        #endregion

        #region 获取会员数据列表
        public async Task<ActionResult> MemberList()
        {
            GridRequest request = new GridRequest(Request);
            
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                var list = _memberContract.Members.Where<Member, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.MemberName,                                        
                    x.IsDeleted,
                    x.IsEnabled,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}