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
using System.Web.Caching;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{

    [License(CheckMode.Verify)]
    public class SectionTemplateController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateController));

        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly ITemplateContract _TemplateContract;

        protected readonly IHtmlItemContract _htmlItemContract;

        protected readonly IHtmlPartContract _htmlPartContract;

        protected readonly IArticleContract _ArticleContract;

        protected readonly IArticleItemContract _articleItemContract;
        /// <summary>
        /// 构造函数-初始化操作对象 
        /// </summary>
        /// <param name="templateContract">业务层操作对象</param>
        public SectionTemplateController(ITemplateContract templateContract,
            IHtmlItemContract htmlItemContract,
            IHtmlPartContract htmlPartContract,
            IArticleContract articleContract,
            IArticleItemContract articleItemContract)
        {
            _TemplateContract = templateContract;
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
                Func<ICollection<Template>, List<Template>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<Template> tree = new List<Template>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
                var parents = _TemplateContract.Templates.Where(m => m.ParentId == null && m.TemplateType == (int)TemplateFlag.Section).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.Id,
                    m.TemplateName,                    
                    m.UpdatedTime,
                    m.TemplatePath,
                    m.HtmlName
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
            dto.TemplateType = (int)TemplateFlag.Section;
            var res = _TemplateContract.Insert(dto);
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
            var result = _TemplateContract.Edit(Id);
            return PartialView(result);
        }
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Section;
            var res = _TemplateContract.Update(dto);
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
            try
            {
                var listTemplate = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.TemplateType == (int)TemplateFlag.Section);
                string strTemp = string.Empty;
                //获取模版保存路径
                string filePath = ConfigurationHelper.GetAppSetting("SectionTemplatePath");
                //获取模版保存后缀名
                string suffix = ConfigurationHelper.GetAppSetting("TemplateSuffix");
                string templatePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
                string strRegex = @"\$section_(\d)_(\d)_show_(\d)_(\{)([\s\S])*?(\})";
                //string strRegexArticleAttr = @"\$section_(\d)_(\d)_show_(\d)";
                var articles = _ArticleContract.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass); 
                for (int i = 0; i < Id.Length; i++)
                {
                    int id = Id[i];
                    var template = listTemplate.Where(x => x.Id == id).FirstOrDefault();
                    string strTemplateHtml = template.TemplateHtml;
                    StringBuilder sb = new StringBuilder();
                    MatchCollection matcheArticleAttrList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
                    //if (matcheArticleAttrList == null || matcheArticleAttrList.Count == 0)
                    //{
                    //    return Json(new OperationResult(OperationResultType.Error, "提供参数不全"));
                    //}
                    foreach (var matcheArticleAttrId in matcheArticleAttrList)
                    {
                        string[] strMatches = matcheArticleAttrId.ToString().Split('_');
                        int intParentId = int.Parse(strMatches[1]);
                        int intArticleAttrId = int.Parse(strMatches[2]);
                        int intArticleCount = int.Parse(strMatches[4]);
                        string strReg = @"\$section_" + intParentId + "_" + intArticleAttrId + "_show_" + intArticleCount + @"_(\{)([\s\S])*?(\})";
                        MatchCollection matches = Regex.Matches(strTemplateHtml, strReg, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );                                
                        foreach (var match in matches)
                        {
                            IQueryable<Article> listArticle = articles .OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).Take(intArticleCount).AsQueryable();
                            string strHtml = match.ToString().Split('{')[1];
                            strHtml = strHtml.Substring(0, strHtml.Length - 1);
                            StringBuilder sbLabel = new StringBuilder();
                            foreach (var article in listArticle)
                            {
                                sbLabel.Append(strHtml.Replace("$section_title", article.Title)
                                    //.Replace("$section_publisher", article.Member.MemberName)
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

                   
                   
                    bool result = TemplateHelper.SaveTemplate(templatePath, template.HtmlName, suffix, strTemplateHtml);
                    if (!result)
                    {
                        strTemp = template.TemplateName + ",";
                    }
                    if (result)
                    {
                        template.TemplatePath = filePath + "/" + template.HtmlName + suffix;
                        //_TemplateContract.Update(template);
                    }
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
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                throw;
            }
            
        }
        #endregion

        #region 查看模版详情
        public ActionResult View(int Id)
        {
            string strId = Request["Id"];
            var result = _TemplateContract.View(Id);
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
                result = _TemplateContract.CheckTemplateName(templateName, TemplateFlag.Article);
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
            var result = _TemplateContract.Remove(Id);
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
            var result = _TemplateContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

     }
}