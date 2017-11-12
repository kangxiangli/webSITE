using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
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
    public class ArticleItemService : ServiceBase, IArticleItemContract
    {
        #region 声明数据层对象

        private readonly IRepository<ArticleItem, int> _articleItemRepository;

        private readonly IRepository<Article, int> _articleRepository;

        private readonly IRepository<Template, int> _templateRepository; 

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleItemService));

        public ArticleItemService(IRepository<ArticleItem, int> articleItemRepository,
            IRepository<Template, int> templateRepository,
            IRepository<Article, int> articleRepository)
            : base(articleItemRepository.UnitOfWork)
        {
            _articleItemRepository = articleItemRepository;
            _templateRepository = templateRepository;
            _articleRepository = articleRepository;
        }
        #endregion

        #region 查看数据
                
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ArticleItem View(int Id)
        {
            var entity = _articleItemRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑数据对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ArticleItemDto Edit(int Id)
        {
            var entity = _articleItemRepository.GetByKey(Id);
            Mapper.CreateMap<ArticleItem, ArticleItemDto>();
            var dto = Mapper.Map<ArticleItem, ArticleItemDto>(entity);
            return dto;
        }
        #endregion

        #region 获取优惠卷数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<ArticleItem> ArticleItems { get { return _articleItemRepository.Entities; } }
        #endregion         

        #region 按条件检查数据是否存在

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<ArticleItem, bool>> predicate, int id = 0)
        {
            return _articleItemRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
                
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ArticleItemDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult oper = new OperationResult(OperationResultType.Success);
                IQueryable<ArticleItem> listArticleItem = ArticleItems.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                foreach (var dto in dtos)
                {
                    int count = listArticleItem.Where(x => x.ArticleItemName == dto.ArticleItemName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在");
                    }
                    count = listArticleItem.Where(x => x.ArticleItemPath == dto.ArticleItemPath).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "路径已经存在");
                    }
                    Mapper.CreateMap<ArticleItemDto, ArticleItem>();
                    var temp = Mapper.Map<ArticleItemDto, ArticleItem>(dto);
                    temp.CreatedTime = DateTime.Now;
                    oper = CreateHtml(temp);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return oper;
                    }
                    else
                    {
                        dto.HtmlPath = oper.Data.ToString();
                    }  
                }
                OperationResult result = _articleItemRepository.Insert(dtos,
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
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion
 
        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params ArticleItemDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled=true;
                OperationResult oper = new OperationResult(OperationResultType.Success);
                IQueryable<ArticleItem> listArticleItem = ArticleItems.Where(x=>x.IsDeleted==false && x.IsEnabled==true);                
                foreach (var dto in dtos)
                {
                    int count = listArticleItem.Where(x => x.ArticleItemName == dto.ArticleItemName && x.Id != dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,名称已经存在！");
                    }
                    Mapper.CreateMap<ArticleItemDto, ArticleItem>();
                    var temp = Mapper.Map<ArticleItemDto, ArticleItem>(dto);
                    temp.CreatedTime = DateTime.Now;
                    oper = CreateHtml(temp);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return oper;
                    }
                    else
                    {
                        dto.HtmlPath = oper.Data.ToString();
                    }    
                }                 
                OperationResult result = _articleItemRepository.Update(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                int resCount =UnitOfWork.SaveChanges();
                return resCount > 0 ? new OperationResult(OperationResultType.Success, "修改成功") : new OperationResult(OperationResultType.Error, "修改失败！");

            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion
 
        #region 移除数据

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据
                
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据
                
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _articleItemRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }
        #endregion

        #region 启用数据
                
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据
                
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 设为APP展示
        /// <summary>
        /// 设为APP展示
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OperationResult SetShow(int Id)
        {
            var entity = this.ArticleItems.Where(x => x.Id == Id).FirstOrDefault();
            if (entity==null )
            {
                return new OperationResult(OperationResultType.Error, "数据不存在");
            }
            else
            {
                entity.IsApp = true;
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = AuthorityHelper.OperatorId;
                int count = _articleItemRepository.Update(entity);
                return count>0?new OperationResult(OperationResultType.Success,"设置成功"):new OperationResult(OperationResultType.Error, "设置失败");
            }
         }
        #endregion

        #region 选取键值对集合
        /// <summary>
        /// 选取键值对集合
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<ArticleItem> listArt = this.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null);
            foreach (var art in listArt)
            {
                list.Add(new SelectListItem() { Value=art.Id.ToString(),Text=art.ArticleItemName});
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Value=string.Empty,Text=title});
            }
            return list;
        }

        public List<SelectListItem> SelectList(string title,int parentId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<ArticleItem> listArt = this.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == parentId);
            foreach (var art in listArt)
            {
                list.Add(new SelectListItem() { Value = art.Id.ToString(), Text = art.ArticleItemName });
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Value = string.Empty, Text = title });
            }
            return list;
        }

        public List<SelectListItem> SelectAllList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<ArticleItem> listArt = this.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            foreach (var art in listArt)
            {
                if (art.ParentId==null)
                {
                    list.Add(new SelectListItem() { Value = "0", Text = art.ArticleItemName });
                }
                else
                {
                    list.Add(new SelectListItem() { Value = art.Id.ToString(), Text = " ->" + art.ArticleItemName });
                }                
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Value = string.Empty, Text = title });
            }
            return list;
        }
        #endregion

        #region 生成静态页
        private OperationResult CreateHtml(ArticleItem articleItem)
        {
            try
            {
                Template template = _templateRepository.GetByKey(articleItem.TemplateId);
                OperationResult oper = new OperationResult(OperationResultType.Error);
                if (template==null)
                {
                    oper.Message = "静态页不存在";
                    return oper;
                }
                else
                {
                    IQueryable<Article> listArticles = _articleRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass);                                        
                    string strRegex = @"\$section_(\d)_(\d)_show_(\d)(\{)([\s\S])*?(\})";                    
                    string strTemplateHtml = template.TemplateHtml;
                    StringBuilder sb = new StringBuilder();
                    MatchCollection matcheArticleList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );                        
                    foreach (var matcheArticle in matcheArticleList)
                    {
                        string[] strMatches = matcheArticle.ToString().Split('_');
                        int intParentId = int.Parse(strMatches[1]);
                        int intArticleAttrId = int.Parse(strMatches[2]);
                        int intArticleCount = int.Parse(strMatches[4]);
                        strRegex = @"\$section_" + intParentId + "_" + intArticleAttrId + "_show_" + intArticleCount + @"(\{)([\s\S])*?(\})";
                        MatchCollection matches = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );                                
                        foreach (var match in matches)
                        {
                            List<Article> listArticle = listArticles.Where(x=>x.ArticleItemId==articleItem.Id).OrderBy(x => x.IsTop == true && x.IsHot == true && x.IsRecommend == true).Take(intArticleCount).ToList();
                            string strHtml = match.ToString().Split('{')[1];
                            strHtml = strHtml.Substring(0, strHtml.Length - 1);
                            StringBuilder sbLabel = new StringBuilder();
                            foreach (var article in listArticle)
                            {
                                sbLabel.Append(strHtml.Replace("$section_title", article.Title)
                                    .Replace("$section_publisher", article.Admin==null?string.Empty:article.Admin.Member.MemberName)
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
                    if (string.IsNullOrEmpty(articleItem.HtmlPath))
                    {
                        articleItem.HtmlPath = articleItem.ArticleItemPath + DateTime.Now.ToString("yyyyMMddhhmm") + ".html"; ;
                    }                     
                    bool res = FileHelper.SavePath(articleItem.HtmlPath, strTemplateHtml);
                    if (res == true)
                    {
                        return new OperationResult(OperationResultType.Success, "生成成功", articleItem.HtmlPath);
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Success, "静态页生成失败");
                    }       
                }
            
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Success, "静态页生成失败");                
            }
        }
        #endregion
    }
}
