using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using System.Text;
using System.Text.RegularExpressions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
   //[License(CheckMode.Verify)]
    public class ProductListTemplateController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateController));

        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
         protected readonly ITemplateContract _templateContract;

        protected readonly IHtmlItemContract _htmlItemContract;

        protected readonly IHtmlPartContract _htmlPartContract;

        protected readonly IArticleContract _ArticleContract;

        protected readonly IArticleItemContract _articleItemContract;
        /// <summary>
        /// 构造函数-初始化操作对象 
        /// </summary>
        /// <param name="templateContract">业务层操作对象</param>
        public ProductListTemplateController(ITemplateContract templateContract,
            IHtmlItemContract htmlItemContract,
            IHtmlPartContract htmlPartContract,
            IArticleContract articleContract,
            IArticleItemContract articleItemContract)
        {
            _templateContract = templateContract;
            _htmlItemContract = htmlItemContract;
            _htmlPartContract = htmlPartContract;
            _ArticleContract = articleContract;
            _articleItemContract = articleItemContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化模版界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取栏目模版列表
        /// <summary>
        /// 根据条件获取模版数据集
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Template, bool>> predicate = FilterHelper.GetExpression<Template>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                IQueryable<Template> listTemplate = _templateContract.Templates.Where(m => m.TemplateType == (int)TemplateFlag.ProductList);
                var list = listTemplate.Where(predicate).Select(m => new
                {
                    m.Id,
                    m.TemplateName,
                    m.TemplatePath,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.IsDefault,
                    m.UpdatedTime,
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                }).ToList();
                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加栏目模版
        /// <summary>
        /// 初始化创建模版界面
        /// </summary>        
        /// <returns></returns>
        
        public ActionResult Create()
        {
            string title = "请选择";
            var listHtmlPart = _htmlPartContract.SelectList(title);
            ViewBag.HtmlPart = listHtmlPart;
            ViewBag.ArticleItem = _articleItemContract.SelectList(title);
            return PartialView();
        }
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.ProductList;
            var res = _templateContract.Insert(dto);
            return Json(res);

        }
        #endregion

        #region 修改栏目模版
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            string title = "请选择";
            var listHtmlPart = _htmlPartContract.SelectList(title);
            ViewBag.HtmlPart = listHtmlPart;
            ViewBag.ArticleItem = _articleItemContract.SelectList(title);
            var result = _templateContract.Edit(Id);
            return PartialView(result);
        }
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.ProductList;
            var res = _templateContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 生成静态页
        /// <summary>
        /// 初始化生成静态页面界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Build(int[] Id)
        {
            var listTemplate = _templateContract.Templates.Where(x => x.IsDeleted == false && x.TemplateType == (int)TemplateFlag.SectionList);
            string strTemp = string.Empty;
            //获取模版保存路径
            string filePath = ConfigurationHelper.GetAppSetting("SectionListTemplatePath");
            //获取模版保存后缀名
            string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
            string templatePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
            string strRegex = @"\$article(\{)([\s\S])*?(\})";
            string strRegexShowCount = @"\$article_show_(\d)";
            string strRegexPage = @"\$article_page_(\d)_(\{)([\s\S])*?(\})";
            //string strRegexPagePath = @"\$page_path";
            var articles = _ArticleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true); // ApprovalStatus 0表示审核中，1表示审核不通过，2表示审核通过
            //添加次数，模版的路径只添加分页列表的第一页路径
            int addCount = 0;
            for (int i = 0; i < Id.Length; i++)
            {
                int id = Id[i];
                var template = listTemplate.Where(x => x.Id == id).FirstOrDefault();
                string strTemplateHtml = template.TemplateHtml;
                StringBuilder sb = new StringBuilder();
                //匹配显示文章数量标签
                MatchCollection matcheShowCount = Regex.Matches(strTemplateHtml, strRegexShowCount, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                if (matcheShowCount.Count == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "提供参数不全"));
                }
                else if (matcheShowCount.Count > 1)
                {
                    return Json(new OperationResult(OperationResultType.Error, "提供参数过多"));
                }
                //匹配显示页码数标签
                MatchCollection matchePage = Regex.Matches(strTemplateHtml, strRegexPage, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                if (matchePage.Count == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "提供参数不全"));
                }
                else if (matchePage.Count > 1)
                {
                    return Json(new OperationResult(OperationResultType.Error, "提供参数过多"));
                }
                string strShowCount=matcheShowCount[0].Value.Split('_')[2];
                int intShowCount = int.Parse(strShowCount);
                //获取页码
                string strPage = matchePage[0].Value.Split('_')[2];
                int intPage = int.Parse(strPage);
                //Match macthPagePath = Regex.Match(strTemplateHtml, strRegexPagePath, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                //获取页码路径标签
                string[] strMatchePage = matchePage[0].Value.Split('{');
                string strPagePath = strMatchePage[1];
                strPagePath = strPagePath.Substring(0, strPagePath.Length - 1);
                StringBuilder sbPath = new StringBuilder();
                for (int j = 0; j < intPage; j++)
                {
                    sbPath.Append(strPagePath.Replace("$page_path", j.ToString()));
                }
                strTemplateHtml = strTemplateHtml.Replace(matchePage[0].Value, sbPath.ToString());
                MatchCollection matcheArticleList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                foreach (var matcheArticle in matcheArticleList)
                {
                    string strHtml = matcheArticle.ToString().Split('{')[1];
                    strHtml = strHtml.Substring(0, strHtml.Length - 1);
                    IQueryable<Article> listArticle = articles.OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).Take(intShowCount);

                    StringBuilder sbLabel = new StringBuilder();
                    foreach (var article in listArticle)
                    {
                        sbLabel.Append(strHtml.Replace("$article_title", article.Title)
                            //.Replace("$article_publisher", article.Member.MemberName)
                            .Replace("$article_time", article.UpdatedTime.ToString("yyyy-MM-dd"))
                            .Replace("$article_content", article.Content)
                            .Replace("$article_hits", article.Hits.ToString())
                            .Replace("$article_img", article.CoverImagePath.ToString())
                            .Replace("$article_path", article.ArticlePath)
                            .Replace("$article_summary", article.Summary == null ? article.Content.Substring(0, article.Content.Length > 100 ? 100 : article.Content.Length) : article.Summary));
                    }
                    strTemplateHtml = strTemplateHtml.Replace(matcheArticle.ToString(), sbLabel.ToString());
                }
                
                 
                bool result = TemplateHelper.SaveTemplate(templatePath+template.TemplateName+"/", template.HtmlName, suffix, strTemplateHtml);
                if (!result)
                {
                    strTemp = template.TemplateName + ",";
                }
                addCount++;
                //if (result && addCount ==0)
                //{
                //    template.TemplatePath = filePath + template.TemplateName+"/" +template.HtmlName + suffix;
                //    _templateContract.Update(template);
                //}
            }
            if (string.IsNullOrEmpty(strTemp))
            {
                return Json(new OperationResult(OperationResultType.Success, "生成成功！"));
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Success, strTemp + "生成失败，其他生成成功"));
            }
        }
        #endregion

        #region 查看模版详情
        public ActionResult View(int Id)
        {
            string strId = Request["Id"];
            var result = _templateContract.Templates.Where(x => x.Id == Id).FirstOrDefault();
            return PartialView(result);
        }
        #endregion

        #region 校验模版名称
        /// <summary>
        /// 校验模版名称是否重复
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckTemplateName()
        {
            string templateName = Request["templateName"];
            int result = 0;//0表示接受的模版名称为空，1表示模版名称不存在，2表示模版名称已存在
            if (string.IsNullOrEmpty(templateName))
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = _templateContract.CheckTemplateName(templateName, TemplateFlag.Article);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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
            var result = _templateContract.Remove(Id);
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
            var result = _templateContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取文章二级栏目
        /// <summary>
        /// 获取文章栏目二级栏目列表
        /// </summary>
        /// <returns></returns>
        //public JsonResult GetArticleAttrChild()
        //{
        //    int Id = int.Parse(Request["Id"]);
        //    var listArticleAttr = _ArticleAttributeContract.ArticleAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == Id).Select(x => new { 
        //      x.Id,
        //      x.AttributeName
        //    });
        //    return Json(listArticleAttr,JsonRequestBehavior.AllowGet);
        //}
        #endregion
    }
}