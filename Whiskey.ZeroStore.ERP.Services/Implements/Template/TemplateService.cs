using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;

using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Security.Cryptography;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Text.RegularExpressions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    /// <summary>
    /// 实现定义的业务操作接口
    /// </summary>
    public class TemplateService : ServiceBase, ITemplateContract
    {
        #region 声明数据层操作对象
        /// <summary>
        /// 声明操作对象
        /// </summary>
        private readonly IRepository<Template, int> _templateRepository;

        private readonly IRepository<Article, int> _articleRepository;
        private readonly IRepository<Department, int> _DepartmentRepository;

        private readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateService)); 
        /// <summary>
        /// 拿到上下文并赋值给操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public TemplateService(IRepository<Template, int> templateRepository,
            IRepository<Department, int> _DepartmentRepository,
            IRepository<Article, int> articleRepository)
            : base(templateRepository.UnitOfWork)
        {
            _templateRepository = templateRepository;
            _articleRepository = articleRepository;
            this._DepartmentRepository = _DepartmentRepository;
        }
        #endregion

        #region 根据Id查看数据
        /// <summary>
        /// 根据Id查看数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        public Template View(int Id)
        {
            var entity = this.Templates.Where(x => x.Id == Id).FirstOrDefault();
            return entity;
        }
        #endregion

        #region 获取要编辑的数据
        /// <summary>
        /// 获取要编辑的数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        public TemplateDto Edit(int Id)
        {
            Template template = _templateRepository.GetByKey(Id);
            var map = Mapper.CreateMap<Template, TemplateDto>();
            var dto = Mapper.Map<Template, TemplateDto>(template);
            return dto;
        }
        #endregion

        #region 获取模版集合
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Template> Templates
        {
            get { return _templateRepository.Entities; }
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params TemplateDto[] dtos)
        {            
            try
            {
                dtos.CheckNotNull("dtos");                
                OperationResult oper = new OperationResult(OperationResultType.Error);
                foreach (var dto in dtos)
                {
                    int count = Templates.Where(x => x.TemplateName == dto.TemplateName && x.TemplateType == dto.TemplateType).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，模版名称已经存在！");
                    }
                    else
                    {
                        Mapper.CreateMap<TemplateDto, Template>();
                        var temp = Mapper.Map<TemplateDto, Template>(dto);
                        temp.CreatedTime = DateTime.Now;                         
                        if (temp.TemplateType == (int)TemplateFlag.Website)
                        {
                            oper = CreateWebsite(temp,false);
                        }
                        else
                        {
                            oper = CreateHtml(temp);
                        }
                        if (oper.ResultType != OperationResultType.Success)
                        {
                            return oper;
                        }
                        else
                        {
                            dto.TemplatePath = oper.Data.ToString();
                        }  
                    }
                }

                OperationResult result = _templateRepository.Insert(dtos,
                dto =>
                {
                
                },
                (dto, entity) =>
                {
                    if (entity.EnabledPerNotifi)
                    {
                        entity.DepartTypeFlags = null;
                    }
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍候重试！");
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Update(params TemplateDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult oper = new OperationResult(OperationResultType.Error);                 
                foreach (var dto in dtos)
                {
                    int count = Templates.Where(x => x.TemplateName == dto.TemplateName && x.TemplateType == dto.TemplateType && x.Id != dto.Id).Count();
                    if (count>0)
                    {
                        return  new OperationResult(OperationResultType.Error, "更新失败，名称已经存在！");
                    }
                    else
                    {
                        Mapper.CreateMap<TemplateDto, Template>();
                        var temp = Mapper.Map<TemplateDto, Template>(dto);
                        temp.CreatedTime = DateTime.Now;
                       
                        if (temp.TemplateType == (int)TemplateFlag.Website || temp.TemplateType == (int)TemplateFlag.Section)
                        {
                            oper = CreateWebsite(temp,false);
                        }
                        else
                        {
                            oper = CreateHtml(temp);
                        }
                        if (oper.ResultType != OperationResultType.Success)
                        {
                            return oper;
                        }
                        else
                        {
                            dto.TemplatePath = oper.Data.ToString();
                        }  
                    }
                }
                var result = _templateRepository.Update(dtos, dto => { }, (dto, entity) =>
                {
                    if (entity.EnabledPerNotifi)
                    {
                        entity.DepartTypeFlags = null;
                    }
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "更新失败，服务器忙，请稍候重试！");
            }
        }

        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">主键Id</param>
        /// <returns></returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _templateRepository.Entities.Where(m => ids.Where(k=>k==m.Id).Count()>0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _templateRepository.Update(entity);
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
        /// <param name="ids">主键Id</param>
        /// <returns></returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _templateRepository.Entities.Where(m => ids.Where(k => k == m.Id).Count() > 0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _templateRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion       

        #region 校验名称
        /// <summary>
        /// 校验模版名称
        /// </summary>
        /// <param name="templateName">模版名称</param>
        /// <returns></returns>
        public int CheckTemplateName(string templateName ,TemplateFlag templateSort)
        {
            int result = 0;  //0表示接受的模版名称为空，1表示模版名称不存在，2表示模版名称已存在
            if (string.IsNullOrEmpty(templateName)) return result;
            var template = _templateRepository.Entities.Where(x => x.TemplateName == templateName && x.TemplateType ==(int)templateSort);
            if (template.Count() > 0)
            {
                return result = 2;
            }
            else
            {
                return result = 1;
            }
        }

        #endregion

        #region 获取键值对
        /// <summary>
        /// 获取键值对集合
        /// </summary>
        /// <param name="title">默认标题</param>
        /// <returns></returns>
        public IEnumerable<KeyValue<string, string>> SelectList(string title)
        {
            var list = new List<KeyValue<string, string>>();
            var categories = _templateRepository.Entities.Where(m => m.IsDeleted == false && m.IsEnabled == true && m.TemplateType ==(int)TemplateFlag.Section  &&  m.ParentId == null).ToList();
            foreach (var parent in categories)
            {
                list.Add(new KeyValue<string, string> { Key = parent.TemplateName, Value = parent.Id.ToString() });                
            }
            list.Insert(0, new KeyValue<string, string> { Key = title, Value = "" });
            return list;
        }

        public List<SelectListItem> SelectList(string title, TemplateFlag templateFlag)
        {
            var list = new List<SelectListItem>();
            List<Template> templates = _templateRepository.Entities.Where(m => m.IsDeleted == false && m.IsEnabled == true && m.TemplateType == (int)templateFlag && m.ParentId == null).ToList();
            foreach (var parent in templates)
            {
                if (parent.IsDefault==true)
                {
                    list.Add(new SelectListItem { Text = parent.TemplateName, Value = parent.Id.ToString(),Selected=true });    
                }
                else
                {
                    list.Add(new SelectListItem { Text = parent.TemplateName, Value = parent.Id.ToString() });    
                }                
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem { Text = title, Value = "" });
            }
            return list; 
        }
        #endregion

        #region 设为默认模板
        /// <summary>
        /// 设为默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="templateFlag"></param>
        /// <returns></returns>
        public OperationResult SetDefault(int Id, TemplateFlag templateFlag, TemplateTypeFlag templateTypeFlag = TemplateTypeFlag.PC)
        {
            try
            {
                List<TemplateDto> listDto = new List<TemplateDto>();
                IQueryable<Template> listEntity=  this.Templates.Where(x => x.TemplateType == (int)templateFlag);
                Template temp = listEntity.Where(x => x.Id == Id).FirstOrDefault();
                if (temp != null)
                {
                    Mapper.CreateMap<Template, TemplateDto>();
                    var dto = Mapper.Map<Template, TemplateDto>(temp);
                    if (templateTypeFlag == TemplateTypeFlag.手机)
                    {
                        dto.IsDefaultPhone = true;
                    }
                    else
                    {
                        dto.IsDefault = true;
                    }
                    listDto.Add(dto);
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "模板不存在");
                }
                IQueryable<Template> listDefault = listEntity.Where(x => x.TemplateNotificationId == temp.TemplateNotificationId);
                if (templateTypeFlag == TemplateTypeFlag.手机)
                {
                    listDefault = listDefault.Where(w => w.IsDefaultPhone);
                }
                else
                {
                    listDefault = listDefault.Where(w => w.IsDefault);
                }
                foreach (var item in listDefault)
                {
                    Mapper.CreateMap<Template, TemplateDto>();
                    var dto = Mapper.Map<Template, TemplateDto>(item);
                    if (templateTypeFlag == TemplateTypeFlag.手机)
                    {
                        dto.IsDefaultPhone = false;
                    }
                    else
                    {
                        dto.IsDefault = false;
                    }
                    listDto.Add(dto);
                }
                OperationResult oper= this.Update(listDto.ToArray());
                return oper;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "设置失败！");
            }
        }
        #endregion

        #region 批量生产静态页
        public OperationResult Build(int[] Id)
        {
            IQueryable<Template> list = this.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            OperationResult oper = new OperationResult(OperationResultType.Success,"生成成功");
            foreach (Template item in list)
            {
                if (item.TemplateType == (int)TemplateFlag.Article || item.TemplateType == (int)TemplateFlag.Product)
                {
                    CreateHtml(item);
                }
                else if (item.TemplateType == (int)TemplateFlag.Website )
                {
                    oper = CreateWebsite(item, false);
                }
                if (oper.ResultType != OperationResultType.Success)
                {
                    return oper;
                }                 
            }
            return oper;
        }
        #endregion

        #region 设为首页
        public OperationResult SetIndex(int Id)
        {
            try
            {
                Template template = this.View(Id);
                OperationResult oper = CreateWebsite(template, true);
                if (oper.ResultType==OperationResultType.Success)
                {
                    template.IsDefault=true;
                    template.TemplatePath = oper.Data.ToString();
                    template.UpdatedTime=DateTime.Now;
                    List<Template> listTemplate = _templateRepository.Entities.Where(x => x.IsDefault == true).ToList();
                    foreach (Template item in listTemplate)
                    {
                        item.IsDefault = false;   
                    }
                    UnitOfWork.TransactionEnabled = true;
                    _templateRepository.Update(listTemplate);
                    _templateRepository.Update(template);                   
                    int count = UnitOfWork.SaveChanges();
                    if (count>0)
                    {
                        oper = new OperationResult(OperationResultType.Success, "设置成功");
                    }
                    else
                    {
                        oper = new OperationResult(OperationResultType.Error, "设置失败");
                    }
                }
                return oper;
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "设置失败");
            }
        }
        #endregion

        #region 生成静态页
        /// <summary>
        /// 生成静态页
        /// </summary>
        /// <returns></returns>
        private OperationResult CreateHtml(Template template)
        {
            try
            {
                string currentPath = this.GetSavePath(template.TemplateType);
                DateTime current = DateTime.Now;
                if (string.IsNullOrEmpty(template.TemplatePath))
                {
                    currentPath = currentPath + current.Year.ToString() + "/" + current.Month.ToString() + "/" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".html";    
                }
                else
                {
                    currentPath = template.TemplatePath;
                }
                string htmlcontent = template.TemplateHtml;
                bool res = FileHelper.SavePath(currentPath, htmlcontent);
                if (res == true)
                {
                    return new OperationResult(OperationResultType.Success, "生成成功", currentPath);
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


        #region 获取保存路径
        private string GetSavePath(int templateType)
        {
            string strSavePath = string.Empty;
            switch (templateType)
            {
                case (int)TemplateFlag.Article:
                    strSavePath = ConfigurationHelper.GetAppSetting("ArticleTemplatePath");
                    break;
                case (int)TemplateFlag.Product:
                    strSavePath = ConfigurationHelper.GetAppSetting("ProductTemplatePath");
                    break;
                case (int)TemplateFlag.ProductList:
                    strSavePath = ConfigurationHelper.GetAppSetting("ProductListTemplatePath");
                    break;
                case (int)TemplateFlag.Section:
                    strSavePath = ConfigurationHelper.GetAppSetting("SectionTemplatePath");
                    break;
                case (int)TemplateFlag.SectionList:
                    strSavePath = ConfigurationHelper.GetAppSetting("SectionListTemplatePath");
                    break;
                case (int)TemplateFlag.Website:
                    strSavePath = ConfigurationHelper.GetAppSetting("WebsiteTemplatePath");
                    break;
                default:
                    strSavePath = ConfigurationHelper.GetAppSetting("ArticleTemplatePath");
                    break;
            }
            return strSavePath;
        }
        #endregion

        #region 生成网站首页
        private OperationResult CreateWebsite(Template template,bool IsIndex)
        {
            try
            {
                string filePath = GetSavePath(template.TemplateType);
                string strRegex = @"\$section_(\d*)_(\d*)_show_(\d)_(\{)([\s\S])*?(\})";
                IQueryable<Article> listArticles = _articleRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Pass).OrderBy(x => x.IsTop || x.IsHot || x.IsShow).ThenByDescending(x => x.CreatedTime);
                string strTemplateHtml = template.TemplateHtml;
                StringBuilder sb = new StringBuilder();
                MatchCollection matcheArticleAttrList = Regex.Matches(strTemplateHtml, strRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);//, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
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
                        IQueryable<Article> listArticle = listArticles.Where(x => x.ArticleItemId == intArticleAttrId).Take(intArticleCount).AsQueryable();
                        string strHtml = match.ToString().Split('{')[1];
                        strHtml = strHtml.Substring(0, strHtml.Length - 1);
                        StringBuilder sbLabel = new StringBuilder();
                        foreach (var article in listArticle)
                        {
                            sbLabel.Append(strHtml.Replace("$section_title", article.Title)
                                .Replace("$section_publisher", article.Operator==null?string.Empty:article.Operator.Member.MemberName)
                                .Replace("$section_time", article.UpdatedTime.ToString("yyyy-MM-dd"))
                                .Replace("$section_content", article.Content)
                                .Replace("$section_hits", article.Hits.ToString())
                                .Replace("$section_img", article.CoverImagePath)
                                .Replace("$section_path", string.IsNullOrEmpty(article.JumpLink) ? article.ArticlePath : article.JumpLink)
                                .Replace("$section_summary", article.Summary == null ? article.Content.Substring(0, article.Content.Length > 100 ? 100 : article.Content.Length) : article.Summary));
                        }
                        strTemplateHtml = strTemplateHtml.Replace(match.ToString(), sbLabel.ToString());
                    }
                }
                if (IsIndex)
                {
                    filePath = ConfigurationHelper.GetAppSetting("WebsitePath");                    
                    template.TemplatePath = filePath + "index.html";
                }
                else
                {
                    if (string.IsNullOrEmpty(template.TemplatePath))
                    {
                        DateTime current = DateTime.Now;
                        template.TemplatePath = filePath + current.Year.ToString() + "/" + current.Month.ToString() + "/" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".html";
                    }
                }                
                bool res = FileHelper.SavePath(template.TemplatePath, strTemplateHtml);
                if (res == true)
                {
                    return new OperationResult(OperationResultType.Success, "生成成功", template.TemplatePath);
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


        public Template GetNotificationTemplate(TemplateNotificationType templateNotiFlag, TemplateFlag tempflag = TemplateFlag.Notification)
        {
            var data = (from s in this.Templates.Where(w => w.IsEnabled && !w.IsDeleted && (w.IsDefault || w.IsDefaultPhone) && w.TemplateNotificationId.HasValue && w.TemplateType == (int)tempflag)
                        orderby s.IsDefaultPhone
                        let tn = s.templateNotification
                        where tn.IsEnabled && !tn.IsDeleted && tn.NotifciationType == templateNotiFlag
                        select s).FirstOrDefault();
            return data;

        }

        public List<int> GetNotificationDepartIds(int Id)
        {
            var list = new List<int>();
            try
            {
                var str = Templates.Where(w => w.Id == Id && !w.EnabledPerNotifi && w.DepartTypeFlags != null).Select(s => s.DepartTypeFlags).FirstOrDefault();
                if (str.IsNotNullAndEmpty())
                {
                    var listdt = str.Split(",", true).Select(s => (DepartmentTypeFlag)s.CastTo<int>()).ToList();
                    var listids = _DepartmentRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && listdt.Contains(w.DepartmentType)).Select(s => s.Id).ToList();
                    return listids;
                }
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }
        }
    }
}
