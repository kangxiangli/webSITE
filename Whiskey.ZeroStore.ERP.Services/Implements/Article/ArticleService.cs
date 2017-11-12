using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ArticleService : ServiceBase, IArticleContract
    {
        #region 声明业务层操作对象
                
        /// <summary>
        /// 操作对象
        /// </summary>
        private readonly IRepository<Article, int> _articleRepository;

        private readonly IRepository<Template, int> _templateRepository;

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleService));
        /// <summary>
        /// 拿到上下文并赋值给操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public ArticleService(IRepository<Article, int> articleRepository,
            IRepository<Template, int> templateRepository)
            : base(articleRepository.UnitOfWork)
        {
            _articleRepository = articleRepository;
            _templateRepository = templateRepository;            
        }
        #endregion

        #region 获取数据详情
                
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public Article View(int Id)
        {
            var entity = _articleRepository.GetByKey(Id);
            return entity;
        }

        #endregion

        #region 获取数据集
                
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Article> Articles
        {
            get { return _articleRepository.Entities; }
        }
        #endregion

        #region 移除和恢复数据
                
        /// <summary>
        /// 逻辑删除数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }

        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 启用和禁用数据
                
        public  OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }

        public  OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 审核数据
        
        
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="verifyType">审核状态</param>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        public OperationResult Verify(int verifyType, params int[] ids)
        {
            try
            {
                
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.VerifyType = verifyType;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "审核成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "审核失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
        #endregion
        
        #region 添加数据
         
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Insert(params ArticleDto[] dtos)
        {
            dtos.CheckNotNull("dtos");
            IQueryable<Article> listArt = this.Articles.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
            foreach (var dto in dtos)
            {
                int count =listArt.Where(x => x.Title == dto.Title && x.ArticleItemId == dto.ArticleItemId).Count();
                if (count>0)
                {
                    return new OperationResult(OperationResultType.Error, "文章标题已经存在");
                }
                else
                {
                    Mapper.CreateMap<ArticleDto, Article>();
                    var entity = Mapper.Map<ArticleDto, Article>(dto);
                    entity.CreatedTime = DateTime.Now;
                    var oper= CreateHtml(entity);
                    if (oper.ResultType!=OperationResultType.Success)
                    {
                        return oper;
                    }
                    else
                    {
                        dto.ArticlePath = oper.Data.ToString();
                    }
                }
            }
            OperationResult result = _articleRepository.Insert(dtos,
               dto =>
               {

               },
               (dto, entity) =>
               {
                   entity.CreatedTime = DateTime.Now;
                   entity.OperatorId = AuthorityHelper.OperatorId;                   
                   return entity;
               });
            return result;
        }
        #endregion

        #region 生成静态页
        /// <summary>
        /// 生成静态页
        /// </summary>
        /// <returns></returns>
        private OperationResult CreateHtml(Article article)
        {
            try
            {                 
                string strRegexLast = @"\$last(\{)([\s\S])*?(\})";
                string strRegexNext = @"\$next(\{)([\s\S])*?(\})";
                string currentPath = ConfigurationHelper.GetAppSetting("ArticleSavePath");
                string htmlcontent = string.Empty;
                if (article.TemplateId != null)
                {
                    var template = _templateRepository.Entities.Where(x => x.Id == article.TemplateId).FirstOrDefault();
                    htmlcontent = template.TemplateHtml;
                    string strName =AuthorityHelper.AdminName;
                    htmlcontent = htmlcontent.Replace("$title", article.Title).Replace("$publisher", strName).Replace("$time", article.CreatedTime.ToString("yyyy-MM-dd")).Replace("$content", article.Content)
                        .Replace("$hits", article.Hits.ToString());
                     
                    var listArticle = _articleRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass);
                    var listArticleLast = listArticle.Where(x => x.Id > article.Id).OrderBy(x => x.Id);
                    var listArticleNext = listArticle.Where(x => x.Id < article.Id).OrderByDescending(x => x.Id);
                    Match matchLast = Regex.Match(htmlcontent, strRegexLast, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    Match matchNext = Regex.Match(htmlcontent, strRegexNext, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    if (!string.IsNullOrEmpty(matchLast.Value) && !string.IsNullOrEmpty(matchNext.Value))
                    {
                        string strLast = matchLast.Value.Split('{')[1];
                        strLast = strLast.Substring(0, strLast.Length - 1);
                        string strNext = matchNext.Value.Split('{')[1];
                        strNext = strNext.Substring(0, strNext.Length - 1);
                        if (listArticleLast.Count() > 0)
                        {
                            strLast = strLast.Replace("$last_name", listArticleLast.ToList()[0].Title)
                                .Replace("$last_path", listArticleLast.ToList()[0].ArticlePath);
                            htmlcontent = htmlcontent.Replace(matchLast.Value.ToString(), strLast);
                        }
                        else
                        {
                            htmlcontent = htmlcontent.Replace(matchLast.Value, "");
                        }
                        if (listArticleNext.Count() > 0)
                        {
                            strNext = strNext.Replace("$next_name", listArticleNext.ToList()[0].Title)
                                .Replace("$next_path", listArticleNext.ToList()[0].ArticlePath);
                            htmlcontent = htmlcontent.Replace(matchNext.Value.ToString(), strNext);
                        }
                        else
                        {
                            htmlcontent = htmlcontent.Replace(matchNext.Value, "");
                        }
                    }
                }
                else
                {
                    htmlcontent = article.Content;
                }
                if (string.IsNullOrEmpty(article.ArticlePath))
                {
                    article.ArticlePath = currentPath + DateTime.Now.ToString("yyyyMMdd") + "/" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".html";
                }
                bool res = FileHelper.SavePath(article.ArticlePath, htmlcontent);
                if (res == true)
                {
                    return new OperationResult(OperationResultType.Success, "生成成功", article.ArticlePath);
                }
                else
                {
                    return new OperationResult(OperationResultType.Success, "静态页生成失败");
                }
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Success, "静态页生成失败");
            }
        }
        #endregion

        #region 更新数据
                
        public OperationResult Update(params ArticleDto[] dtos)
        {
            dtos.CheckNotNull("dtos");
            IQueryable<Article> listArt = this.Articles.Where(x => x.IsDeleted == false && x.IsEnabled == true);           
            foreach (var dto in dtos)
            {
                
                int count = listArt.Where(x => x.Title == dto.Title && x.ArticleItemId == dto.ArticleItemId && x.Id!=dto.Id).Count();
                if (count > 0)
                {
                    return new OperationResult(OperationResultType.Error, "文章标题已经存在");
                }
                else
                {
                    var entity = this.Articles.FirstOrDefault(x => x.Id == dto.Id);
                    if (entity!=null)
                    {
                        dto.Hits = entity.Hits;                         
                    }
                    Mapper.CreateMap<ArticleDto, Article>();
                    var temp = Mapper.Map<ArticleDto, Article>(dto);
                    temp.CreatedTime = DateTime.Now;
                    var oper = CreateHtml(temp);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return oper;
                    }
                    else
                    {
                        dto.ArticlePath = oper.Data.ToString();
                    }
                }
            }
            OperationResult result = _articleRepository.Update(dtos,
               dto =>
               {

               },
               (dto, entity) =>
               {
                   entity.CreatedTime = DateTime.Now;
                   entity.OperatorId = AuthorityHelper.OperatorId;
                   return entity;
               });
            return result;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public ArticleDto Edit(int Id)
        {

            var entity = _articleRepository.GetByKey(Id);
            Mapper.CreateMap<Article, ArticleDto>();
            var dto = Mapper.Map<Article, ArticleDto>(entity);
            return dto;
        }
        #endregion

        #region 批量设置属性

        
        /// <summary>
        /// 批量设置属性
        /// </summary>
        /// <param name="strIds">主键ID</param>
        /// <param name="article">属性的在载体</param>
        /// <returns></returns>
        public OperationResult SetAttrubute(string strIds, Article article)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                if (string.IsNullOrEmpty(strIds))
                {
                    return new OperationResult(OperationResultType.NoChanged, "修改失败！");
                }
                else
                {
                    string[] strId = strIds.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    IQueryable<Article> listArt = Articles;                     
                    for (int i = 0; i < strId.Length; i++)
                    {
                        int intId= int.Parse(strId[i]);
                        var result = listArt.Where(x => x.Id == intId).FirstOrDefault();
                        result.Hits = article.Hits;
                        result.IsRecommend = article.IsRecommend;
                        result.IsHot = article.IsHot;
                        result.VerifyType = article.VerifyType;
                        result.UpdatedTime = DateTime.Now;
                        result.IsShow = article.IsShow;
                        _articleRepository.Update(result);
                    }
                    return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "修改成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                }
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.NoChanged, "修改失败！");                
            }
        }
        #endregion

        #region 根据Id生成静态页
        public OperationResult Product(int[] Id)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success,"生成成功");
            var list = this.Articles.Where(x => Id.Contains(x.Id));
            foreach (var item in list)
            {
                oper=this.CreateHtml(item);
                if (oper.ResultType!=OperationResultType.Success)
                {
                    return oper;
                }
            }
            return oper;
        }
        #endregion
    }
}
