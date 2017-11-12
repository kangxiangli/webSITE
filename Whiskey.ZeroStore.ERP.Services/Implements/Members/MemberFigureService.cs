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

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MemberFigureService : ServiceBase, IMemberFigureContract
    {
                 

        #region 声明数据层操作对象
        private readonly IRepository<MemberFigure, int> _memberFigureRepository;

        public MemberFigureService(
            IRepository<MemberFigure, int> memberFigureRepository)
            : base(memberFigureRepository.UnitOfWork)
		{
            _memberFigureRepository = memberFigureRepository;
		}
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public MemberFigure View(int Id)
        {
            var entity = _memberFigureRepository.GetByKey(Id);
            return entity;
		}
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public MemberFigureDto Edit(int Id)
        {
            var entity = _memberFigureRepository.GetByKey(Id);
            Mapper.CreateMap<MemberFigure, MemberFigureDto>();
            var dto = Mapper.Map<MemberFigure, MemberFigureDto>(entity);
            return dto;
		}
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<MemberFigure> MemberFigures { get { return _memberFigureRepository.Entities; } }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MemberFigureDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");                                
                OperationResult result = _memberFigureRepository.Insert(dtos,
				dto =>
				{
                    
				},
				(dto, entity) =>
				{
					entity.CreatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;                    
					return entity;
				});
				return result;
            }catch (Exception ex){
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
        public OperationResult Update(params MemberFigureDto[] dtos)
        {
            try
            {
               
				dtos.CheckNotNull("dtos");
                IQueryable<MemberFigure> listMemeber = MemberFigures;                               
                OperationResult result = _memberFigureRepository.Update(dtos,
					dto =>
					{

					},
					(dto, entity) => {
						entity.UpdatedTime = DateTime.Now;
						entity.OperatorId=AuthorityHelper.OperatorId;                        
						return entity;
					});
				return result;
            }catch (Exception ex){
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
                var entities = _memberFigureRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
                    _memberFigureRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "移除成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
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
                var entities = _memberFigureRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
                    _memberFigureRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "恢复成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
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
                OperationResult result = _memberFigureRepository.Delete(ids);
				return result;
            }catch (Exception ex){
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
                var entities = _memberFigureRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
                    _memberFigureRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "启用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
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
                var entities = _memberFigureRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
                    _memberFigureRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "禁用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion
        

    }
}
