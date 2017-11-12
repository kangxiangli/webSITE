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
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Controllers
{
    /// <summary>
    /// 单品
    /// </summary>
    [License(CheckMode.Verify)]
    public class MemberSingleProductController : BaseController
    {

        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberSingleProductController));
        //声明业务层操作对象
        protected readonly IMemberSingleProductContract _memberSingleProductContract;

        protected readonly IMemberContract _memberContract;

        protected readonly ICommentContract _singleProductCommentContract;

        protected readonly IColorContract _colorContract;

        protected readonly ISeasonContract _seasonContract;

        protected readonly ISizeContract _sizeContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IProductAttributeContract _productAttrContract;

        protected readonly IApprovalContract _productApprovalContract;
        //构造函数-初始化业务层操作对象
        public MemberSingleProductController(IMemberSingleProductContract memberSingleProductContract,
            IMemberContract memberContract,
            ICommentContract singleProductCommentContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ISizeContract sizeContract,
            ICategoryContract categoryContract, 
            IProductAttributeContract productAttrContract,
            IApprovalContract productApprovalContract)
        {
			_memberSingleProductContract=memberSingleProductContract;
            _memberContract = memberContract;
            _singleProductCommentContract = singleProductCommentContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _categoryContract = categoryContract;
            _productAttrContract = productAttrContract;
            _productApprovalContract = productApprovalContract;
		}
        #endregion

        #region 初始化会员管理界面
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
        public ActionResult Create(MemberSingleProductDto dto)
        {
            var result = _memberSingleProductContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 修改信息

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _memberSingleProductContract.Edit(Id);            
            return PartialView(result);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MemberSingleProductDto dto)
        {
            var result = _memberSingleProductContract.Update(dto);
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
            var result = _memberSingleProductContract.View(Id);
            Color color= _colorContract.Colors.Where(x=>x.Id==result.ColorId).FirstOrDefault();
            Season season = _seasonContract.Seasons.Where(x => x.Id == result.SeasonId).FirstOrDefault();
            Size size =_sizeContract.Sizes.Where(x=>x.Id==result.SizeId).FirstOrDefault();
            Category category = _categoryContract.Categorys.Where(x => x.Id == result.CategoryId).FirstOrDefault();
          
            ViewBag.Color= color==null ? "无":color.ColorName;
            ViewBag.Season = season == null ? "无" : season.SeasonName;
            ViewBag.Size = size == null ? "无" : size.SizeName;
            ViewBag.Category = category == null ? "无" : category.CategoryName;
            ViewBag.ProductAttr = _memberSingleProductContract.GetAttrNames(result.ProductAttrId);
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
            Expression<Func<MemberSingleProduct, bool>> predicate = FilterHelper.GetExpression<MemberSingleProduct>(request.FilterGroup);
            string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
            string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberSingleProductContract.MemberSingleProducts.Where<MemberSingleProduct, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Member.MemberName,
                    m.ProductName,
                    m.Price,
                    CoverImage = strApiUrl + m.CoverImage,
                    Image = strApiUrl+ m.Image,                    
                    m.Notes,                                        
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.CreatedTime,
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
            var result = _memberSingleProductContract.Remove(Id);
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
            var result = _memberSingleProductContract.Delete(Id);
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
            var result = _memberSingleProductContract.Recovery(Id);
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
            var result = _memberSingleProductContract.Enable(Id);
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
            var result = _memberSingleProductContract.Disable(Id);
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
            var list = _memberSingleProductContract.MemberSingleProducts.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _memberSingleProductContract.MemberSingleProducts.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传照片
        /// <summary>
        /// 上传照片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            string conPath = ConfigurationHelper.GetAppSetting("SaveMemberSingleProduct");
            string strDate=DateTime.Now.Year+"/"+DateTime.Now.Month+"/"+DateTime.Now.Day+"/";
            string rootPath = AppDomain.CurrentDomain.BaseDirectory + conPath + strDate;
            Guid gid = Guid.NewGuid();
            string fileName = gid.ToString();
            fileName = fileName.Substring(0, 15);
            var listFile = Request.Files;
            bool result = true;
            for (int i = 0; i < listFile.Count; i++)
            {
                string name = listFile[i].FileName;
                int index = name.LastIndexOf('.');
                name = name.Substring(index);
                fileName = fileName + name;
                result = TemplateHelper.SaveUploadFile(listFile[i].InputStream, rootPath, fileName);
            }
            if (result)
            {
                return Json(new { ResultType = OperationResultType.Success, Path = conPath + strDate + fileName }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 单品评论
        /// <summary>
        /// 初始化单品评论界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Comment(int Id)
        {
            ViewBag.SingleProId = Id;
            return PartialView();
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CommentList()
        {
            GridRequest request = new GridRequest(Request); 
            //获取要搜索的字段
            string strMemberName=request.FilterGroup.Rules.Where(x=>x.Field=="MemberName").FirstOrDefault().Value.ToString();
            string strComment=request.FilterGroup.Rules.Where(x=>x.Field=="Comment").FirstOrDefault().Value.ToString();
            //单品Id
            string strId = request.FilterGroup.Rules.Where(x => x.Field == "Id").FirstOrDefault().Value.ToString();
            int id = int.Parse(strId);
            //获取分页信息
            int pageIndex=request.PageCondition.PageIndex;
            int pageSize=request.PageCondition.PageSize;
            //Expression<Func<SingleProductComment, bool>> predicate = FilterHelper.GetExpression<SingleProductComment>(request.FilterGroup);
            var data = await Task.Run(() =>
            {                 
                var count = 0;
                var list = _singleProductCommentContract.Comments.Where(x=>x.SourceId==id && x.CommentSource==(int)CommentSourceFlag.MemberSingleProduct).Select(m => new
                {
                    Comment=m.Content,
                    m.Id,
                    m.MemberId,
                    m.ReplyId,                     
                    m.CreatedTime,
                });
                IQueryable<Member> listMember = _memberContract.Members;
                var comments = (from l in list
                                join
                                m in listMember
                                on
                                l.MemberId equals m.Id
                                select new  
                                {                                   
                                   m.MemberName,
                                   l.Id,
                                   l.Comment,
                                   l.CreatedTime,
                                });                
                var listComm= comments.Where(x => x.MemberName.Contains(strMemberName) || x.Comment.Contains(strComment));
                count = comments == null ? count : listComm.Count();
                var temp = listComm.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new GridData<object>(temp, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除评论
        
        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DeleteComment(int Id)
        {
            var res= _singleProductCommentContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);            
        }

        #endregion

        #region 赞
        public ActionResult Approval(int Id)
        {
            ViewBag.SingleProId = Id;
            return PartialView();
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ApprovalList()
        {
            GridRequest request = new GridRequest(Request);
            //获取要搜索的字段
            string strMemberName = request.FilterGroup.Rules.Where(x => x.Field == "MemberName").FirstOrDefault().Value.ToString();
            //单品Id
            string strId = request.FilterGroup.Rules.Where(x => x.Field == "Id").FirstOrDefault().Value.ToString();
            int id = int.Parse(strId);
            //获取分页信息
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;
            //Expression<Func<SingleProductComment, bool>> predicate = FilterHelper.GetExpression<SingleProductComment>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _productApprovalContract.Approvals.Where(x => x.SourceId == id && x.ApprovalSource == (int)CommentSourceFlag.MemberSingleProduct).Select(m => new
                {
                    m.Id,
                    m.MemberId,
                    m.CreatedTime,
                });
                IQueryable<Member> listMember = _memberContract.Members;
                var comments = (from l in list
                                join
                                m in listMember
                                on
                                l.MemberId equals m.Id
                                select new
                                {
                                    m.MemberName,
                                    l.Id,
                                    l.CreatedTime,
                                });
                var listComm = comments.Where(x => x.MemberName.Contains(strMemberName));
                count = comments == null ? count : listComm.Count();
                var temp = listComm.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new GridData<object>(temp, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除赞
        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DeleteApproval(int Id)
        {
            var res = _productApprovalContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}