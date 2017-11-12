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
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using System.Text.RegularExpressions;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Articles.Controllers
{
    [License(CheckMode.Verify)]
    public class ArticleItemController : BaseController
    {
        #region 初始化操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleItemController));         
        protected readonly ITemplateContract _templateContract;
        protected readonly IArticleItemContract _articleItemContract;
        protected readonly IArticleContract _articleContract;             

        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public ArticleItemController(IArticleItemContract articleAttributeContract,
            ITemplateContract templateContract,
            IArticleItemContract articleItemContracy,
            IArticleContract articleContract,            
        ITemplateArticleAttrRelationshipContract temArticleAttrContract)
        {             
            _templateContract = templateContract;
            _articleItemContract = articleItemContracy;
            _articleContract = articleContract;                     
		}
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取栏目列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ArticleItem, bool>> predicate = FilterHelper.GetExpression<ArticleItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<ArticleItem>, List<ArticleItem>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<ArticleItem> tree = new List<ArticleItem>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };                
                var parents = _articleItemContract.ArticleItems.Where(m => m.ParentId == null).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.ArticleItemName,                                        
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                    m.HtmlPath
                });
                return new GridData<object>(list, list.Count(), request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加栏目
        /// <summary>
        /// 初始化添加数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {            
            var sectionTemplate = _templateContract.SelectList(string.Empty, TemplateFlag.Section);            
            var ArticleItem = _articleItemContract.SelectList("请选择");
            ViewBag.SectionTemplate = sectionTemplate;
            ViewBag.ArticleItem = ArticleItem;
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]        
        public ActionResult Create(ArticleItemDto dto)
        {
            var result = _articleItemContract.Insert(dto);
            return Json(result);
        }
        #endregion

        #region 查看栏目详情
        /// <summary>
        /// 查看数据详情
        /// </summary>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var result = _articleItemContract.View(Id);             
            return PartialView(result);
        }
        #endregion

        #region 修改栏目
        /// <summary>
        /// 初始化修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {             
            IQueryable<Template> listTemplate =_templateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var result = _articleItemContract.Edit(Id);
            Template template = listTemplate.Where(x=>x.Id==result.TemplateId).FirstOrDefault();
            List<SelectListItem> list= new List<SelectListItem>();
            if(template==null)
            {
                list = _templateContract.SelectList(string.Empty,TemplateFlag.Section);
            }
            else
            {
                list = _templateContract.SelectList(string.Empty, (TemplateFlag)template.TemplateType);
            }
            ViewBag.Template = list;
            var ArticleItem = _articleItemContract.SelectList("请选择");
            ViewBag.ArticleItem = ArticleItem;
            return PartialView(result);
        }

        /// <summary>
        /// 提交修改数据
        /// </summary>
        /// <param name="articleAttribute">数据实体</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(ArticleItemDto dto)
        {
            var result = _articleItemContract.Update(dto);
            return Json(result);             
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
            var result = _articleItemContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 注释生成静态页面
        //public ActionResult Build(int[] Id) 
        //{
        //    var listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true);
        //    var listSetionTemplate = _templateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType==(int)TemplateType.SectionTemplate);
        //    var listTemplateArticleAttr = _TemplateArticleAttrContract.templateArticleAttrs.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId==null);
        //    var listArticle = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ApprovalStatus == 2); // ApprovalStatus 0表示审核中，1表示审核不通过，2表示审核通过
        //    //获取栏目保存路径
        //    string filePath = ConfigurationHelper.GetAppSetting("ArticleAttrPath");
        //    //获取栏目保存后缀名
        //    string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
        //    bool isBuild = true;
        //    OperationResult oper = null;
        //    if (Id!=null)
        //    {
               
        //        for (int i = 0; i < Id.Length; i++)
        //        {
        //            int id=Id[i];
        //            string articleAttrPath = AppDomain.CurrentDomain.BaseDirectory + filePath;
        //            var articleAttr = listArticleAttr.Where(x => x.Id == id).FirstOrDefault();
        //            var templateArticleAttr = listTemplateArticleAttr.Where(x => x.ArticleAttrId == id).FirstOrDefault();
        //            var setionTemplate = listSetionTemplate.Where(x => x.Id == templateArticleAttr.TemplateId).FirstOrDefault();
        //            oper=BuildHtmls(id, filePath, articleAttr, setionTemplate, listArticle, templateArticleAttr, suffix, out isBuild);
        //            if (!isBuild)
        //            {
        //                break;
        //            }
        //            if (setionTemplate.Children.Count()!=0)
        //            {
        //                foreach (var item in setionTemplate.Children)
        //                {
        //                    oper=BuildHtmls(id, filePath, articleAttr, item, listArticle, templateArticleAttr, suffix, out isBuild);
        //                    if (!isBuild)
        //                    {
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return Json(new OperationResult(OperationResultType.Success, "生成失败,请选择要生成栏目。"));
        //    }
        //    if (isBuild)
        //    {
        //        return Json(new OperationResult(OperationResultType.Success, "生成成功"));
        //    }
        //    else
        //    {
        //        return Json(oper);
        //    }
        //}

        //public OperationResult BuildHtmls(int id, string filePath, ArticleItem articleAttr, Template setionTemplate, IQueryable<Article> listArticle, TemplateArticleAttrRelationship templateArticleAttr, string suffix, out bool isBuild)
        //{
        //    isBuild = true;
        //    string articleAttrPath = AppDomain.CurrentDomain.BaseDirectory + filePath;
        //    articleAttrPath = articleAttrPath + articleAttr.AttributeName + "/";
        //    StringBuilder sbHtml = new StringBuilder(setionTemplate.TemplateHtml);
        //    Match matcheShow = Regex.Match(sbHtml.ToString(), "\\$show_(\\d)_(\\d)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        //    if (string.IsNullOrEmpty(matcheShow.Value))
        //    {
        //        return new OperationResult(OperationResultType.Error, "提供参数不全");
        //    }
        //    string[] arrMatch = matcheShow.Value.Split('_');
        //    int pageIndex = int.Parse(arrMatch[1]);
        //    int pageCount = int.Parse(arrMatch[2]);
        //    var listSectionArticle = listArticle.Where(x => x.ArticleItemId == id).OrderBy(x => x.IsTop || x.IsRecommend || x.IsHot).Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList();
        //    if (listSectionArticle.Count == 0)
        //    {
        //        return new OperationResult(OperationResultType.Error, "请添加审核通过的文章");
        //    }
        //    int count = 0;
        //    StringBuilder sb = new StringBuilder();
        //    for (int j = 0; j < listSectionArticle.Count(); j++)
        //    {
        //        count = count + 1;
        //        sb.Append(sbHtml.ToString().Replace("$show_" + pageIndex.ToString() + "_" + pageCount.ToString(), "").Replace(
        //            "$title_" + count.ToString(), listSectionArticle[j].Title).Replace(
        //            "$publisher_" + (count).ToString(), listSectionArticle[j].Publisher).Replace(
        //            "$time_" + (count).ToString(), listSectionArticle[j].UpdatedTime.ToString("yyyy-MM-dd")).Replace(
        //            "$hits_" + (count).ToString(), listSectionArticle[j].Hits.ToString()).Replace(
        //            "$img_" + (count).ToString(), listSectionArticle[j].CoverImagePath).Replace(
        //            "$summary_" + (count).ToString(), listSectionArticle[j].Summary == null ? listSectionArticle[j].Content.Substring(0, listSectionArticle[j].Content.Length > 100 ? 100 : listSectionArticle[j].Content.Length) : listSectionArticle[j].Summary).Replace(
        //            "$content_" + (count).ToString(), listSectionArticle[j].Content));
        //    }
        //    StringBuilder sbHeader = new StringBuilder();
        //    sbHeader = sb;
        //    if (setionTemplate.TemplateHeaderId != null)
        //    {
        //        var headerHtml = _templateHeaderContract.TemplateHeaders.Where(x => x.Id == setionTemplate.TemplateHeaderId).FirstOrDefault().HeaderHtml;
        //        sb.Clear();
        //        sb.Append(sbHeader.ToString().Replace("$header", headerHtml));
        //    }
        //    StringBuilder sbFooter = new StringBuilder();
        //    sbFooter = sb;
        //    if (setionTemplate.TemplateFooterId != null)
        //    {
        //        var footerHtml = _templateFooterContract.TemplateFooters.Where(x => x.Id == setionTemplate.TemplateHeaderId).FirstOrDefault().FooterHtml;
        //        sb.Clear();
        //        sb.Append(sbFooter.ToString().Replace("$footer", footerHtml));
        //    }

        //    bool result = TemplateHelper.SaveTemplate(articleAttrPath, setionTemplate.TemplateName, suffix, sb.ToString());
        //    if (result)
        //    {
        //        templateArticleAttr.ArticleAttrPath = filePath + articleAttr.AttributeName + "/" + setionTemplate.TemplateName + suffix;
        //        templateArticleAttr.OperatorId = AuthorityHelper.OperatorId;
        //        templateArticleAttr.UpdatedTime = DateTime.Now;
        //        bool isUpdate = _TemplateArticleAttrContract.Update(templateArticleAttr);
        //        if (!isUpdate)
        //        {
        //            isBuild = false;
        //            return new OperationResult(OperationResultType.Error, "生成失败");
        //        }

        //    }
        //    else
        //    {
        //        isBuild = false;
        //        return new OperationResult(OperationResultType.Error, "生成失败");
        //    }
        //    return new OperationResult(OperationResultType.Success, "生成成功");
        //}

        #endregion

        #region 注释代码
        ///// <summary>
        ///// 初始化添加二级分类页面
        ///// </summary>
        ///// <param name="Id">一级主键Id</param>
        ///// <returns></returns>
        //public ActionResult AddSecond(int Id)
        //{
        //    var result = _articleItemContract.View(Id);
        //    ArticleItem articleAttribute = new ArticleItem();
        //    articleAttribute.ParentId = result.Id;
        //    ViewBag.Name = result.AttributeName;
        //    return PartialView(articleAttribute);
        //}

        ///// <summary>
        ///// 条件添加二级分类数据
        ///// </summary>
        ///// <param name="articleAttribute"></param>
        ///// <returns></returns>
        //[Log]
        //[HttpPost]
        //[ValidateInput(false)]
        //public JsonResult AddSecond(ArticleItem articleAttribute)
        //{
        //    articleAttribute.OperatorId = AuthorityHelper.OperatorId;
        //    articleAttribute.UpdatedTime = DateTime.Now;
        //    articleAttribute.CreatedTime = DateTime.Now;
        //    var result = _articleItemContract.Insert(articleAttribute);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        
        #region 获取分页模版集合
        /// <summary>
        /// 获取分页模版集合
        /// </summary>
        /// <returns></returns>
        public JsonResult GetSectionTemplateList() 
        {
            string strType = Request["type"];
            int intType = int.Parse(strType);
            var list = _templateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == intType).Select(x => new
            { 
              x.Id,
              x.TemplateName
            });
            string path = string.Empty;
            string parentPath = string.Empty;
            if (intType==3)
            {
                int id=int.Parse(Request["Id"]);
                var articleAttr= _articleItemContract.ArticleItems.Where(x => x.Id == id).FirstOrDefault();
                path=articleAttr.ArticleItemPath;
            }
            else if (intType == 2)
            {

            }
            return Json(new { list, path, parentPath }, JsonRequestBehavior.AllowGet);
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
            var result = _articleItemContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 创建父级栏目时生成静态页
        /// <summary>
        /// 生成栏目静态页
        /// </summary>
        public void BuildSection()
        {
            List<ArticleItem> listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null).ToList();
            
            //List<int> listTemplateId = listArticleAttr.Select(x => x.TemplateId).ToList();
            var listTemplate = _templateContract.Templates.Where(x => x.IsDeleted == false && x.TemplateType == (int)TemplateFlag.Section ).ToList();
            string strTemp = string.Empty;
            //获取模版保存路径
            //string filePath = ConfigurationHelper.GetAppSetting("SectionTemplatePath");
            string filePath = string.Empty;
            //获取模版保存后缀名
            string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
            string templatePath = AppDomain.CurrentDomain.BaseDirectory;
            string strRegex = @"\$section_(\d)_(\d)_show_(\d)_(\{)([\s\S])*?(\})";
            //string strRegexArticleAttr = @"\$section_(\d)_(\d)_show_(\d)";
            var articles = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass).ToList(); // ApprovalStatus 0表示审核中，1表示审核不通过，2表示审核通过
            if (articles == null || articles.Count() == 0)
            {
                return;
            }
            foreach (var articleAttr in listArticleAttr)
            {
                filePath = articleAttr.ArticleItemPath;
                int id = articleAttr.TemplateId;
                var template = listTemplate.Where(x => x.Id == id).FirstOrDefault();
                string strTemplateHtml = template.TemplateHtml;
                StringBuilder sb = new StringBuilder();
                MatchCollection matcheArticleAttrList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                if (matcheArticleAttrList == null || matcheArticleAttrList.Count == 0)
                {
                    continue;
                }
                foreach (var matcheArticleAttrId in matcheArticleAttrList)
                {
                    string[] strMatches = matcheArticleAttrId.ToString().Split('_');
                    int intParentId = int.Parse(strMatches[1]);
                    int intArticleAttrId = int.Parse(strMatches[2]);
                    int intArticleCount = int.Parse(strMatches[4]);
                    strRegex = @"\$section_" + intParentId + "_" + intArticleAttrId + "_show_" + intArticleCount + @"(\{)([\s\S])*?(\})";
                    MatchCollection matches = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );                                
                    foreach (var match in matches)
                    {
                        List<Article> listArticle = articles.OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).Take(intArticleCount).ToList();
                        if (listArticle==null || listArticle.Count==0)
                        {
                            continue;
                        }
                        string strHtml = match.ToString().Split('{')[1];
                        strHtml = strHtml.Substring(0, strHtml.Length - 1);
                        StringBuilder sbLabel = new StringBuilder();
                        foreach (var article in listArticle)
                        {
                            sbLabel.Append(strHtml.Replace("$section_title", article.Title)
                                .Replace("$section_publisher", article.Admin == null ? string.Empty : article.Admin.Member.MemberName)
                                .Replace("$section_time", article.UpdatedTime.ToString("yyyy-MM-dd"))
                                .Replace("$section_content", article.Content)
                                .Replace("$section_hits", article.Hits.ToString())
                                .Replace("$section_img", article.CoverImagePath.ToString())
                                .Replace("$section_path", article.ArticlePath)
                                .Replace("$section_summary", article.Summary == null ? article.Content.Substring(0, article.Content.Length > 100 ? 100 : article.Content.Length) : article.Summary));
                        }
                        strTemplateHtml = strTemplateHtml.Replace(match.ToString(), sbLabel.ToString());
                    }
                }

                 
                bool result = true;
                if (string.IsNullOrEmpty(articleAttr.HtmlPath))
                {
                    Random random = new Random();
                    int num = random.Next(1000000);
                    string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Second.ToString() + num.ToString();
                    string rootPath = templatePath + articleAttr;
                    result = TemplateHelper.SaveTemplate(rootPath, fileName, suffix, strTemplateHtml);
                    if (result)
                    {
                        articleAttr.HtmlPath =  fileName + suffix;
                        
                    }
                }
                else
                {
                    string rootPath = templatePath + articleAttr.HtmlPath;
                    result = Whiskey.Web.Helper.FileHelper.SavePath(rootPath, strTemplateHtml);
                }

            }


        }

        #endregion

        #region 创建子级栏目时生成静态页
        public void BuildSectionList()
        {
            IQueryable<Article> listArticle = _articleContract.Articles.Where(x => x.IsDeleted == false && x.VerifyType == (int)VerifyFlag.Pass);
            if (listArticle==null || listArticle.Count()==0)
            {
                return;
            }
            List<ArticleItem> listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null).ToList();
            IQueryable<Template> listTemplate = _templateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == (int)TemplateFlag.SectionList );
            //获取模版保存路径
            //string filePath = ConfigurationHelper.GetAppSetting("SectionListTemplatePath");
            //获取模版保存后缀名
            string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string strRegex = @"\$article(\{)([\s\S])*?(\})";
            string strRegexShowCount = @"\$article_show_(\d)";
            string strRegexPage = @"\$article_page_(\d)_(\{)([\s\S])*?(\})";
            foreach (var articleAttr in listArticleAttr)
            {
                string filePath = articleAttr.ArticleItemPath;
                string savePath = rootPath + filePath;
                Template template = listTemplate.Where(x => x.Id == articleAttr.TemplateId).FirstOrDefault();
                string strTemplateHtml = template.TemplateHtml;
                //匹配显示文章数量标签
                MatchCollection matcheShowCount = Regex.Matches(strTemplateHtml, strRegexShowCount, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                if (matcheShowCount.Count == 0)
                {
                    continue;
                }
                else if (matcheShowCount.Count > 1)
                {
                    continue;
                }
                //匹配显示页码数标签
                MatchCollection matchePage = Regex.Matches(strTemplateHtml, strRegexPage, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                if (matchePage.Count == 0)
                {
                    continue;
                }
                else if (matchePage.Count > 1)
                {
                    continue;
                }
                //获取每页文章数量
                string strShowCount = matcheShowCount[0].Value.Split('_')[2];
                int intShowCount = int.Parse(strShowCount);
                //获取页码
                string strPage = matchePage[0].Value.Split('_')[2];
                int intPage = int.Parse(strPage);
                List<Article> articles = listArticle.OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).ToList();
                if (articles.Count() == 0 || articles == null)
                {
                    continue;
                }
                int pageCount = Convert.ToInt32(Math.Ceiling((double)articles.Count() / intShowCount));
                int pageIndex = 1;
                string[] strMatchePage = matchePage[0].Value.Split('{');
                string strPagePath = strMatchePage[1];
                strPagePath = strPagePath.Substring(0, strPagePath.Length - 1);
                StringBuilder sbPath = new StringBuilder();
                int startCount = 0;
                for (int i = 0; i < pageCount; i++)
                {
                    var tempArticles = articles.Skip((pageIndex - 1) * intShowCount).Take(intShowCount);
                    MatchCollection matcheArticleList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                    foreach (var matcheArticle in matcheArticleList)
                    {
                        string strHtml = matcheArticle.ToString().Split('{')[1];
                        strHtml = strHtml.Substring(0, strHtml.Length - 1);
                        StringBuilder sbLabel = new StringBuilder();
                        foreach (var article in articles)
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

                     
                    if (pageCount < intPage)
                    {
                        for (int j = 0; j < pageCount; j++)
                        {
                            ++startCount;
                            sbPath.Append(strPagePath.Replace("$page_path", filePath + template.HtmlName + startCount.ToString() + suffix));
                        }
                    }
                    else
                    {
                        for (int j = 0; j < intPage; j++)
                        {
                            ++startCount;
                            sbPath.Append(strPagePath.Replace("$page_path", filePath + template.HtmlName + startCount.ToString() + suffix));
                        }
                    }

                    strTemplateHtml = strTemplateHtml.Replace(matchePage[0].Value, sbPath.ToString());
                    bool result = TemplateHelper.SaveTemplate(savePath, template.HtmlName + pageIndex.ToString(), suffix, strTemplateHtml);
                    if (pageIndex == 1)
                    {
                        articleAttr.HtmlPath = filePath + template.HtmlName + pageIndex.ToString() + suffix;
                        //_articleItemContract.Update(articleAttr);
                    }
                    ++pageIndex;
                    //TemplateArticleAttrRelationship tempArtAttr = new TemplateArticleAttrRelationship();
                    //tempArtAttr.ArticleAttrId = articleAttr.Id;
                    //tempArtAttr.ArticleAttrPath = filePath + template.HtmlName + pageIndex.ToString() + suffix;
                    //tempArtAttr.TemplateId = template.Id;
                    //tempArtAttr.OperatorId = AuthorityHelper.OperatorId;
                    //_TemArticleAttrContract.Insert(tempArtAttr);
                }

            }
        }

        #endregion

        #region 生成静态页
        public ActionResult Build()
        {
            try
            {
                BuildSection();
                BuildSectionList();
                return Json(new OperationResult(OperationResultType.Success, "生成成功！"),JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "生成失败！"), JsonRequestBehavior.AllowGet);                
            }
        }
        #endregion

        #region 设为app显示
        public JsonResult SetShow(int Id)
        {
            var res = _articleItemContract.SetShow(Id);
            return Json(res);
        }
        #endregion

        #region 获取文章二级栏目
        /// <summary>
        /// 获取文章栏目二级栏目列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetChild()
        {
            int Id = int.Parse(Request["Id"]);
            var listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == Id).Select(x => new
            {
                x.Id,
                Name = x.ArticleItemName,
            });
            return Json(listArticleAttr, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取一级栏目
        //public 
        #endregion

    }
}