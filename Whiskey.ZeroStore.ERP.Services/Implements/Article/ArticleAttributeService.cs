using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
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
using Whiskey.Utility.Logging;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ArticleAttributeService : ServiceBase,IArticleAttributeContract
    {
        #region 初始化操作对象
                
        /// <summary>
        /// 操作对象
        /// </summary>
        private readonly IRepository<ArticleAttribute, int> _articleAttributeRepository;

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleAttributeService));
        /// <summary>
        /// 拿到上下文并赋值给操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public ArticleAttributeService(IRepository<ArticleAttribute, int> articleAttributeRepository)
            : base(articleAttributeRepository.UnitOfWork)
        {
            _articleAttributeRepository = articleAttributeRepository;
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public ArticleAttribute View(int Id)
        {
            var result = _articleAttributeRepository.GetByKey(Id);
            return result;          
        }
        #endregion

        #region 获取编辑数据
               
        /// <summary>
        /// 获取要编辑的数据
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public ArticleAttributeDto Edit(int Id)
        {
            var entity = _articleAttributeRepository.GetByKey(Id);
            Mapper.CreateMap<ArticleAttribute, ArticleAttributeDto>();
            var dto = Mapper.Map<ArticleAttribute, ArticleAttributeDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
                
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<ArticleAttribute> ArticleAttributes
        {
            get { return _articleAttributeRepository.Entities; }
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ArticleAttributeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<ArticleAttribute> listArticleAttr = ArticleAttributes;
                foreach (var dto in dtos)
                {
                    int count = listArticleAttr.Where(x => x.AttributeName == dto.AttributeName).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在");
                    }
                }
                OperationResult result = _articleAttributeRepository.Insert(dtos,
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
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "添加失败");
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params ArticleAttributeDto[] dtos)
        {
            try
            {

                dtos.CheckNotNull("dtos");
                IQueryable<ArticleAttribute> listArticleAttr = ArticleAttributes;
                foreach (var dto in dtos)
                {
                    int count = listArticleAttr.Where(x => x.AttributeName == dto.AttributeName && x.Id==dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在");
                    }                    
                }
                OperationResult result = _articleAttributeRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        #region 移除数据
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
                var entities = _articleAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleAttributeRepository.Update(entity);
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
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _articleAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
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
                OperationResult result = _articleAttributeRepository.Delete(ids);
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
                var entities = _articleAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleAttributeRepository.Update(entity);
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
                var entities = _articleAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _articleAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
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
            IQueryable<ArticleAttribute> listArt = this.ArticleAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null);
            foreach (var art in listArt)
            {
                list.Add(new SelectListItem() { Value = art.Id.ToString(), Text = art.AttributeName });
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Value = string.Empty, Text = title });
            }
            return list;
        }        
        #endregion


    }
}
