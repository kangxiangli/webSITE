using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core.Data.Extensions;
using System.Linq.Expressions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.MobileApi.Areas.Articles.Controllers
{
    public class ArticleController : Controller
    {
        #region 初始化操作对象
                
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleController));
        //声明业务层操作对象
        protected readonly IArticleContract _articleContract;

        protected readonly IArticleAttributeContract _articleAttrContract;

        public ArticleController(IArticleContract articleContract,
            IArticleAttributeContract articleAttrContract)
        {
            _articleContract = articleContract;
            _articleAttrContract = articleAttrContract;
        }
        #endregion

        #region 获取文章
                
        /// <summary>
        /// 获取文章
        /// </summary>
        /// <param name="PageCount"></param>
        /// <returns></returns>
        public JsonResult Get(int PageCount = 1)
        {
            string strClassificationId= Request["ClassificationId"];
            string strIsShow = Request["IsShow"];
            //获取文章集合
            //IQueryable<Article> listArticle=  _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ApprovalStatus == (int)ArticleStatus.AuditThrough);
            if(!string.IsNullOrEmpty(strClassificationId))
            {
               int classificationId=int.Parse(strClassificationId);
               //获取文章栏目
               ArticleAttribute articleAttr = _articleAttrContract.ArticleAttributes.Where(x => x.Id == classificationId && x.ParentId==null).FirstOrDefault();
               if (articleAttr==null)
               {
                   return Json(new { ResultType = OperationResultType.Error, Message="服务器忙，请稍候重试！"}, JsonRequestBehavior.AllowGet);
               }
               List<ArticleAttribute> listArticleAttr=articleAttr.Children.ToList();
               List<Article> listArticle = new List<Article>();
               foreach (var item in listArticleAttr)
               {
                   listArticle.AddRange(item.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass).ToList());
               }
               if (strIsShow=="true")
               {
                   listArticle = listArticle.Where(x => x.IsShow == true).ToList();
               }
               string classificationName = articleAttr.AttributeName;
               string classificationNotes = articleAttr.Description;
               if(listArticle.Count()>0)
               {
                   listArticle = listArticle.OrderBy(x => x.IsTop && x.IsHot && x.IsRecommend).Take(PageCount).ToList();
                   var result= listArticle.Select(x=> new {
                       ArticleId=x.Id,
                       Title=x.Title,
                       Summary=x.Summary,
                       CoverImage=x.CoverImagePath,
                       JumpLink=x.JumpLink
                   });
                   return Json(new { ResultType = OperationResultType.Success, Message = "获取成功！", ClassificationName = classificationName, ClassificationNotes = classificationNotes, ListArticle = result }, JsonRequestBehavior.AllowGet);
               }
               else
               {
                   return Json(new { ResultType = OperationResultType.Success, Message = "获取成功！", ClassificationName = classificationName, ClassificationNotes = classificationNotes, ListArticle = "" }, JsonRequestBehavior.AllowGet);
               }
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, Message="服务器忙，请稍候重试！"}, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 添加文章
        public JsonResult Add()
        {
            try
            {
                string strTitle = Request["Title"];
                string strContent = Request["Content"];
                
                //_articleContract.Insert()
            }
            catch (Exception)
            {
                
                throw;
            }
            return null;
        }
        #endregion


    }
}