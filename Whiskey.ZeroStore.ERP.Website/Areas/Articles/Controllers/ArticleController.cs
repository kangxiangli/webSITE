using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
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
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.Text;
using Whiskey.Utility.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
namespace Whiskey.ZeroStore.ERP.Website.Areas.Articles.Controllers
{
    [License(CheckMode.Verify)]
    public class ArticleController : BaseController
    {
        #region 声明操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleController));

        /// <summary>
        /// 初始文章分类化对象
        /// </summary>
        protected readonly IArticleItemContract _articleItemContract;
        /// <summary>
        /// 初始化文章操作对象
        /// </summary>
        protected readonly IArticleContract _articleContract;

        protected readonly ITemplateContract _TemplateContract;

        protected readonly IArticleImageContract _ArticleImageContract;
         
        protected readonly ITemplateArticleAttrRelationshipContract _TemArticleAttrContract;
        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public ArticleController(IArticleContract articleContract,
            IArticleItemContract articleItemContract, 
            ITemplateContract templateContract,
            IArticleImageContract articleImageContract,
             
            ITemplateArticleAttrRelationshipContract temArticleAttrContract)
        {
            _articleContract = articleContract;
            _articleItemContract = articleItemContract;
            _TemplateContract = templateContract;
            _ArticleImageContract = articleImageContract;             
            _TemArticleAttrContract = temArticleAttrContract;
            
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

        #region 展示数据列表
        /// <summary>
        /// 展示数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            var searchType = Request["searchType"];             
            Expression<Func<Article, bool>> predicate = FilterHelper.GetExpression<Article>(request.FilterGroup);            
            var data = await Task.Run(() =>
            {
                int count = 0;
                var articles = _articleContract.Articles.Where<Article, int>(predicate, request.PageCondition, out count).Select(x => new {
                    x.Id,
                    x.Title,
                    //art.CreatedTime,
                    x.IsTop,
                    x.IsHot,
                    x.IsRecommend,
                    x.Hits,
                    x.VerifyType,
                    x.CoverImagePath,
                    x.ArticlePath,
                    x.CreatedTime,
                    x.Admin.Member.RealName,
                    AttributeItem=x.ArticleItem==null?string.Empty:x.ArticleItem.ArticleItemName,
                });                
                    return new GridData<object>(articles, count, request.RequestInfo);                                 
            });
            return Json(data,JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加数据页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            string title="请选择";
            var parentItem = _articleItemContract.SelectList(string.Empty);
            int parentId = int.Parse(parentItem.FirstOrDefault().Value);
            var childItem = _articleItemContract.SelectList(string.Empty,parentId);
            ViewBag.ArticleItem = parentItem;
            ViewBag.ChildItem = childItem;                       
            ViewBag.ArticleTemplate = _TemplateContract.SelectList(title, TemplateFlag.Article);             
            return PartialView(); 
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="article">数据实体</param>
        /// <returns></returns>        
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(ArticleDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId;
            OperationResult oper = _articleContract.Insert(dto);
            return Json(oper);
        }

        #region 注释代码
        //public JsonResult Add(ArticleDto article)
        //{

        //    IQueryable<Article> listArt = _articleContract.Articles;
        //    if (article.IsTop)
        //    {
        //        var temp = listArt.Where(x => x.IsTop == true && x.TemplateId == article.TemplateId);
        //        if (temp.Count() > 0)
        //        {
        //            foreach (var item in temp)
        //            {
        //                item.IsTop = false;
        //                _articleContract.Update(item);
        //            }
        //        }
        //    }
        //    var tempArticle = listArt.Where(x => x.Title == article.Title);
        //    if (tempArticle.Count() > 0)
        //    {
        //        return Json(new OperationResult(OperationResultType.Error,"添加失败，文章标题已经存在！"), JsonRequestBehavior.AllowGet);
        //    }
        //    var articleAttr = _articleItemContract.ArticleItems.Where(x => x.Id == article.ArticleItemId).FirstOrDefault();
        //    string savePath = articleAttr.ArticleItemPath + "Article/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
        //    string fullPath = FileHelper.UrlToPath(savePath);
        //    Random random = new Random();
        //    string fileName = DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.Second + random.Next(100).ToString();
        //    string strRegexLast = @"\$last(\{)([\s\S])*?(\})";
        //    string strRegexNext = @"\$next(\{)([\s\S])*?(\})";
        //    string suffix = ".html";
        //    article.Publisher = AuthorityHelper.AdminName;
        //    article.FileName = fileName + suffix;
        //    article.ArticlePath = savePath + fileName + suffix;
        //    if (article.TemplateId != null)
        //    {
        //        int templateId = (int)article.TemplateId;
        //        var template = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == templateId).FirstOrDefault();
        //        var result = _articleContract.Insert(article);
        //        if (result.ResultType!=OperationResultType.Success)
        //        {
        //            TemplateHelper.DelTemplate(fullPath + fileName + suffix);
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }
        //        string htmlcontent = template.TemplateHtml;
        //        htmlcontent = htmlcontent.Replace("$title", article.Title).Replace("$publisher", article.Publisher).Replace("$hits", article.Hits.ToString()).Replace("$time",DateTime.Now.ToString("yyyy-MM-dd")).Replace("$content", article.Content);
        //        if (template.TemplateHeaderId != null)
        //        {
        //            var header = _TemplateHeaderContract.TemplateHeaders.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == template.TemplateHeaderId).FirstOrDefault();
        //            htmlcontent = htmlcontent.Replace("$header", header.HeaderHtml);
        //        }
        //        if (template.TemplateFooterId != null)
        //        {
        //            var header = _TemplateFooterContract.TemplateFooters.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == template.TemplateFooterId).FirstOrDefault();
        //            htmlcontent = htmlcontent.Replace("$footer", header.FooterHtml);
        //        }
        //        var listArticle = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ApprovalStatus == 2  ); //0表示审核中，1表示审核不通过，2表示审核通过
        //        var listArticleLast = listArticle.Where(x => x.Id > article.Id).OrderBy(x => x.Id);
        //        var listArticleNext = listArticle.Where(x => x.Id < article.Id).OrderByDescending(x => x.Id);
        //        //匹配上一页
        //        Match matchLast = Regex.Match(htmlcontent, strRegexLast, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        //        //匹配下一页
        //        Match matchNext = Regex.Match(htmlcontent, strRegexNext, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        //        if (!string.IsNullOrEmpty(matchLast.Value) && !string.IsNullOrEmpty(matchNext.Value))
        //        {
        //            string strLast = matchLast.Value.Split('{')[1];
        //            strLast = strLast.Substring(0, strLast.Length - 1);
        //            string strNext = matchNext.Value.Split('{')[1];
        //            strNext = strNext.Substring(0, strNext.Length - 1);
        //            if (listArticleLast.Count() > 0)
        //            {
        //                strLast = strLast.Replace("$last_name", listArticleLast.ToList()[0].Title)
        //                    .Replace("$last_path", listArticleLast.ToList()[0].ArticlePath);
        //                htmlcontent = htmlcontent.Replace(matchLast.Value.ToString(), strLast);
        //            }
        //            else
        //            {
        //                htmlcontent = htmlcontent.Replace(matchLast.Value, "");
        //            }
        //            if (listArticleNext.Count() > 0)
        //            {
        //                strNext = strNext.Replace("$next_name", listArticleNext.ToList()[0].Title)
        //                    .Replace("$next_path", listArticleNext.ToList()[0].ArticlePath);
        //                htmlcontent = htmlcontent.Replace(matchNext.Value.ToString(), strNext);
        //            }
        //            else
        //            {
        //                htmlcontent = htmlcontent.Replace(matchNext.Value, "");
        //            }
        //        }
        //        htmlcontent = htmlcontent.Replace("$articleId", article.Id.ToString());
        //        if (!Directory.Exists(fullPath))
        //        {
        //            Directory.CreateDirectory(fullPath);
        //        }
        //        TemplateHelper.SaveTemplate(fullPath, fileName, suffix, htmlcontent);
        //        return Json(new OperationResult(OperationResultType.Success, "添加成功！"), JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        var result = _articleContract.Insert(article);
        //        string hitsScriptPath = ConfigurationHelper.GetAppSetting("HitsScriptPath");
        //        hitsScriptPath = AppDomain.CurrentDomain.BaseDirectory + hitsScriptPath;
        //        XElement xEle = XElement.Load(hitsScriptPath);
        //        IEnumerable<XElement> elements = from ele in xEle.Elements("hitsScript")
        //                                         select ele;
        //        foreach (var element in elements)
        //        {
        //            string xmlDoc = element.Element("script").Value;
        //            xmlDoc = xmlDoc.Replace("$articleId", article.Id.ToString());
        //            article.Content = article.Content + xmlDoc;
        //        }
        //        if (result.ResultType != OperationResultType.Success)
        //        {                     
        //            return Json(count, JsonRequestBehavior.AllowGet);
        //        }
        //        if (!Directory.Exists(fullPath))
        //        {
        //            Directory.CreateDirectory(fullPath);
        //        }
        //        TemplateHelper.SaveTemplate(fullPath, fileName, suffix, article.Content);
        //        return Json(count, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion

        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _articleContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 审核数据
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [License(CheckMode.Check)]
        public ActionResult Verify(int[] Id)
        {
            int approvalStatus = (int)VerifyFlag.Pass;
            var result = _articleContract.Verify(approvalStatus, Id);
            //BuildIndex();
            //BuildSection();
            //BuildSectionList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            string title = "请选择";
            var result = _articleContract.Edit(Id);
            ArticleItem item = _articleItemContract.ArticleItems.FirstOrDefault(x => x.Id == result.ArticleItemId);
            var parentItem = _articleItemContract.SelectList(string.Empty);
            int parentId = item.Parent.Id;
            result.ParentArticleItemId = parentId;
            var childItem = _articleItemContract.SelectList(string.Empty, parentId);
            ViewBag.ArticleItem = parentItem;
            ViewBag.ChildItem = childItem;
            List<SelectListItem> listItem = _TemplateContract.SelectList(title, TemplateFlag.Article);
            if (result.TemplateId!=null)
            {
                foreach (SelectListItem selectItem in listItem)
                {
                    selectItem.Selected = false;
                }
            }
            ViewBag.ArticleTemplate = _TemplateContract.SelectList(title, TemplateFlag.Article);
            
            return PartialView(result);
        }

        /// <summary>
        /// 提交修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(ArticleDto dto)
        {
            var res = _articleContract.Update(dto);
            return Json(res);
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
            var result = _articleContract.Remove(Id);
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
            var result = _articleContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 生成静态页面
        /// <summary>
        /// 批量生成数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Product(int[] Id)
        {
            var res = _articleContract.Product(Id);
            return Json(res);
        }

        /// <summary>
        /// 全部生成
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductAll()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请添加文章");
            var list = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            if (list != null && list.Count() > 0)
            {
                var ids= list.Select(x => x.Id).ToList();
                oper= _articleContract.Product(ids.ToArray());                
            }
            return Json(oper);
        }
        #endregion

        #region 设置基本属性
        /// <summary>
        ///  初始化设置属性页面
        /// </summary>
        /// <returns></returns>
        public ActionResult SetAttrubute()
        {
            return PartialView();
        }

        /// <summary>
        /// 提交属性数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [Log]
        public JsonResult SetAttrubute(Article article)
        {
            string strIds = Request["ids"];
            var result = _articleContract.SetAttrubute(strIds, article);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取模版集合
        /// <summary>
        /// 获取模版集合
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTemplateList()
        {
            var result = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == (int)TemplateFlag.Article).Select(x => new
            {
                x.TemplateName,
                x.Id,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取文章栏目
        /// <summary>
        /// 获取文章分类集合
        /// </summary>
        /// <returns></returns>

        public JsonResult GetArticleItemList()
        {
            var result=_articleItemContract.ArticleItems.Where(x=>x.IsDeleted==false && x.IsEnabled==true && x.ParentId!=null).Select(x => new
            {
                x.ArticleItemName,
                x.Id,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传封面图片
        /// <summary>
        /// 上传封面图片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public JsonResult AddCoverImage(HttpPostedFileBase[] file)
        {
            List<OperationResult> listResultType = new List<OperationResult>();                         
            foreach (HttpPostedFileBase hpf in file)
            {
                #region                                               
                //获取模版保存路径
                string filePath = ConfigurationHelper.GetAppSetting("ArticleImagePath");                
                int lastIndex = hpf.FileName.IndexOf('.');
                string suffix = hpf.FileName.Substring(lastIndex + 1);
                string fileName = DateTime.Now.ToString("yyyyMMdd") +"/"+ DateTime.Now.ToString("yyyyMMddhhmmsss") + "." + suffix;
                filePath=filePath+fileName;
                string saveResult = ImageHelper.MakeThumbnail(hpf.InputStream, filePath, 750, 450, "H", "Jpg");
                var data = new {
                    Name = hpf.FileName,
                    FilePath=filePath,
                };
                if (string.IsNullOrEmpty(saveResult))
                {
                    listResultType.Add(new OperationResult(OperationResultType.Error, "添加失败", data));
                }
                else
                {
                    listResultType.Add(new OperationResult(OperationResultType.Success,"添加成功",data));
                }
                #endregion
                
            }
            return Json(listResultType, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public JsonResult AddImage()
        {
            HttpFileCollectionBase files = Request.Files;
            if (files!=null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    #region
                    //获取后缀名
                    string configSuffix = ConfigurationHelper.GetAppSetting("ImageSuffix");
                    //获取模版保存路径
                    string filePath = ConfigurationHelper.GetAppSetting("ArticleImagePath");
                    string partPath = string.Empty;
                    string[] suffixs = configSuffix.Split(',');
                    int lastIndex = files[i].FileName.IndexOf('.');
                    string suffix = files[i].FileName.Substring(lastIndex + 1);
                    var keyword = suffixs.Where(x => x.Equals(suffix));
                    if (keyword.Count() > 0)
                    {
                        DateTime current = DateTime.Now;
                        string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/"  + DateTime.Now.ToString("HH") + "/";
                        Guid gId = Guid.NewGuid();
                        string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                        partPath = filePath + strDate + fileName;
                        string saveResult = ImageHelper.MakeThumbnail(files[i].InputStream, partPath, 670, 1000, "W", "Jpg");
                        if (!string.IsNullOrEmpty(saveResult))
                        {
                            return Json(new OperationResult(OperationResultType.Success, "添加成功", saveResult), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new OperationResult(OperationResultType.Error,"添加失败", ""),JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                         return Json(new OperationResult(OperationResultType.Error,"添加失败", ""),JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                }
                return null;
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "添加失败", ""), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取图片
        /// <summary>
        /// 获取图片集合
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImageList() 
        {
            var result = _ArticleImageContract.ArticleImages.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ImageStatus == 1);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 生成静态页
        /// <summary>
        /// 生成首页静态页
        /// </summary>
        public void BuildIndex()
        {
            
        }

        /// <summary>
        /// 生成栏目静态页
        /// </summary>
        public void BuildSection()
        {
            List<ArticleItem> listArticleAttr= _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null).ToList();
            
            //List<int> listTemplateId = listArticleAttr.Select(x => x.TemplateId).ToList();
            var listTemplate = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.TemplateType == (int)TemplateFlag.Section).ToList();
            string strTemp = string.Empty;
            //获取模版保存路径
            //string filePath = ConfigurationHelper.GetAppSetting("SectionTemplatePath");
            string filePath = string.Empty;
            //获取模版保存后缀名
            string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
            string templatePath = AppDomain.CurrentDomain.BaseDirectory ;
            string strRegex = @"\$section_(\d)_(\d)_show_(\d)(\{)([\s\S])*?(\})";
            string strRegexArticleAttr = @"\$section_(\d)_(\d)_show_(\d)";
            var articles = _articleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType ==(int)VerifyFlag.Pass).ToList(); // ApprovalStatus 0表示审核中，1表示审核不通过，2表示审核通过
            foreach (var articleAttr in listArticleAttr)
            {
                filePath =  articleAttr.ArticleItemPath;
                int id = articleAttr.TemplateId;
                var template = listTemplate.Where(x => x.Id == id).FirstOrDefault();
                string strTemplateHtml = template.TemplateHtml;
                StringBuilder sb = new StringBuilder();
                MatchCollection matcheArticleAttrList = Regex.Matches(strTemplateHtml, strRegexArticleAttr, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
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
                        List<Article> listArticle = articles .OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).Take(intArticleCount).ToList();
                        string strHtml = match.ToString().Split('{')[1];
                        strHtml = strHtml.Substring(0, strHtml.Length - 1);
                        StringBuilder sbLabel = new StringBuilder();
                        foreach (var article in listArticle)
                        {
                            sbLabel.Append(strHtml.Replace("$section_title", article.Title)
                                .Replace("$section_publisher", "")
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

                
                bool result=true;
                if (string.IsNullOrEmpty(articleAttr.HtmlPath))
                {
                    Random random = new Random();
                    int num = random.Next(1000000);
                    string fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Second.ToString() + num.ToString();
                    string rootPath = templatePath + articleAttr.ArticleItemPath;
                    result = TemplateHelper.SaveTemplate(rootPath, fileName, suffix, strTemplateHtml);
                    if (result)
                    {
                        articleAttr.HtmlPath = articleAttr.ArticleItemPath + fileName + suffix;
                        //result=_articleItemContract.Update(articleAttr);
                    } 
                }
                else
                {
                    string rootPath = templatePath + articleAttr.HtmlPath;
                    result = Whiskey.Web.Helper.FileHelper.SavePath(rootPath, strTemplateHtml);
                }                
                
            }
            
            
        }

        public void BuildSectionList()
        {
            IQueryable<Article> listArticle = _articleContract.Articles.Where(x => x.IsDeleted == false && x.VerifyType ==(int)VerifyFlag.Pass);//0表示审核中，1表示审核不通过，2表示审核通
            if (listArticle==null || listArticle.Count()==0)
            {
                return;
            }
            List<ArticleItem> listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null).ToList();
            IQueryable<Template> listTemplate = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == (int)TemplateFlag.SectionList);
            //获取模版保存路径
            //string filePath = ConfigurationHelper.GetAppSetting("SectionListTemplatePath");
            //获取模版保存后缀名
            string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
            string rootPath = AppDomain.CurrentDomain.BaseDirectory ;
            string strRegex = @"\$article(\{)([\s\S])*?(\})";
            string strRegexShowCount = @"\$article_show_(\d)";
            string strRegexPage = @"\$article_page_(\d)_(\{)([\s\S])*?(\})";
            foreach (var articleAttr in listArticleAttr)
            {
                string filePath=articleAttr.ArticleItemPath;
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
                List<Article> articles = listArticle .OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).ToList();
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
                                .Replace("$article_publisher", "")
                                .Replace("$article_time", article.UpdatedTime.ToString("yyyy-MM-dd"))
                                .Replace("$article_content", article.Content)
                                .Replace("$article_hits", article.Hits.ToString())
                                .Replace("$article_img", article.CoverImagePath.ToString())
                                .Replace("$article_path", article.ArticlePath)
                                .Replace("$article_summary", article.Summary == null ? article.Content.Substring(0, article.Content.Length > 100 ? 100 : article.Content.Length) : article.Summary));
                        }
                        strTemplateHtml = strTemplateHtml.Replace(matcheArticle.ToString(), sbLabel.ToString());
                    }
                    
                    if (pageCount<intPage)
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
                    //tempArtAttr.ArticleArticleItemPath = filePath + template.HtmlName + pageIndex.ToString() + suffix;
                    //tempArtAttr.TemplateId = template.Id;
                    //tempArtAttr.OperatorId = AuthorityHelper.OperatorId;
                    //_TemArticleAttrContract.Insert(tempArtAttr);
                }
                
            }
        }
        #endregion

        #region 文章点击率
        public JsonResult AddHits()
        {
            string strId = Request["id"];
            OperationResult oper = new OperationResult(OperationResultType.Error, "添加失败");
            if (!string.IsNullOrEmpty(strId))
            {
                int id = int.Parse(strId);
                var entity = _articleContract.Edit(id);
                entity.Hits = entity.Hits + 1;
               oper= _articleContract.Update(entity);                
            }
            return Json(oper);
        }
        #endregion

        #region 删除图片
        /// <summary>
        /// 删除图片
        /// </summary>
        /// <returns></returns>
        public JsonResult DelImage()
        { 
           string path=Request["ImagePath"];
           if (!string.IsNullOrEmpty(path))
           {
               bool isDel = true;//FileHelper.Delete(path);
               if (isDel)
               {
                   return Json(new OperationResult(OperationResultType.Success, "删除成功"));
               }
               else
               {
                   return Json(new OperationResult(OperationResultType.Error, "删除失败"));
               }
           }
           else
           {
               return Json(new OperationResult(OperationResultType.Error, "删除失败"));
           }
        }
        #endregion

    }
}