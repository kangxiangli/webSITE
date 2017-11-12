﻿using System;
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
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppArticleService: ServiceBase,IAppArticleContract
    {
        #region 初始化数据层操作对象

        private readonly IRepository<AppArticle, int> _appArticleRepository;
        public AppArticleService(IRepository<AppArticle, int> appArticleRepository)
            : base(appArticleRepository.UnitOfWork)
        {
            _appArticleRepository = appArticleRepository;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public AppArticle View(int Id)
        {
            var entity = _appArticleRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public AppArticleDto Edit(int Id)
        {
            var entity = _appArticleRepository.GetByKey(Id);
            Mapper.CreateMap<AppArticle, AppArticleDto>();
            var dto = Mapper.Map<AppArticle, AppArticleDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<AppArticle> AppArticles { get { return _appArticleRepository.Entities; } }
        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<AppArticle, bool>> predicate, int id = 0)
        {
            return _appArticleRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AppArticleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<AppArticle> listAppArticle = AppArticles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listAppArticle.Where(x => x.ArticleTitle == dto.ArticleTitle && x.MemberId == dto.MemberId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，文章标题已经存在");
                    }                    
                }
                OperationResult result = _appArticleRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params AppArticleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<AppArticle> listAppArticle = AppArticles.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                foreach (var dto in dtos)
                {
                    int count = listAppArticle.Where(x => x.ArticleTitle == dto.ArticleTitle && x.MemberId==dto.MemberId &&x.Id!=dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "编辑失败，文章标题已经存在");
                    } 
                }
                OperationResult result = _appArticleRepository.Update(dtos,
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
                var entities = _appArticleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _appArticleRepository.Update(entity);
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
                var entities = _appArticleRepository.Entities.Where(m => ids.Contains(m.Id));
                var listAppArticle = this.AppArticles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                foreach (var entity in entities)
                {
                    int count = listAppArticle.Where(x => x.ArticleTitle == entity.ArticleTitle).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "恢复失败,和正常的数据名字有重复");
                    }
                    else
                    {
                        entity.IsDeleted = false;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _appArticleRepository.Update(entity);
                    }
                   
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
                OperationResult result = _appArticleRepository.Delete(ids);
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
                var listAppArticle = this.AppArticles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                var entities = _appArticleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    int count = listAppArticle.Where(x => x.ArticleTitle == entity.ArticleTitle).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "启用失败,和已经启用的数据名字有重复");
                    }
                    else
                    {
                        entity.IsEnabled = true;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _appArticleRepository.Update(entity);
                    }                    
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
                var entities = _appArticleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _appArticleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据键值对
        /// <summary>
        /// 获取数据键值对
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<AppArticle> listMember = AppArticles.Where(x => x.IsEnabled == true && x.IsDeleted == false);
            if (listMember.Count() > 0)
            {

                foreach (var memeber in listMember)
                {
                    list.Add(new SelectListItem() { Text = memeber.ArticleTitle, Value = memeber.Id.ToString() });
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = ""});    
            }            
            return list;
        }
        #endregion

        #region 设为推荐
        public OperationResult Recommend(int[] Ids)
        {
            try
            {
                Ids.CheckNotNull("Ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _appArticleRepository.Entities.Where(m => Ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsRecommend = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _appArticleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "推荐成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "推荐失败！");
            }
        }

        #endregion

        #region 取消推荐
        public OperationResult CancleRecommend(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _appArticleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsRecommend = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _appArticleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "取消推荐成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception)
            {
                return new OperationResult(OperationResultType.Error, "取消推荐失败！");
            }
        }
        #endregion
         
    }
}