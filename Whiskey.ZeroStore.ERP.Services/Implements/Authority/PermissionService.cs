




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
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class PermissionService : ServiceBase, IPermissionContract
    {
        #region PermissionService


		private readonly IRepository<Permission, int> _permissionRepository;


		public PermissionService(
			IRepository<Permission, int> permissionRepository
		): base(permissionRepository.UnitOfWork)
		{
			_permissionRepository = permissionRepository;
		}

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public Permission View(int Id){
			var entity=_permissionRepository.GetByKey(Id);
            return entity;
		}


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public PermissionDto Edit(int Id){
			var entity=_permissionRepository.GetByKey(Id);
            Mapper.CreateMap<Permission, PermissionDto>();
            var dto = Mapper.Map<Permission, PermissionDto>(entity);
            return dto;
		}


        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Permission> Permissions { get { return _permissionRepository.Entities; } }



        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Permission, bool>> predicate, int id = 0)
        {
            return _permissionRepository.ExistsCheck(predicate, id);
        }



        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params PermissionDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _permissionRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message,ex.ToString());
            }
        }



		/// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params PermissionDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _permissionRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message,ex.ToString());
            }
        }



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
				var entities = _permissionRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_permissionRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "移除成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message,ex.ToString());
            }
        }


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
				var entities = _permissionRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_permissionRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "恢复成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message,ex.ToString());
            }
        }


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
				OperationResult result = _permissionRepository.Delete(ids);
				return result;
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message,ex.ToString());
            }

        }


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
				var entities = _permissionRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_permissionRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "启用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message,ex.ToString());
            }
		}


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
				var entities = _permissionRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_permissionRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "禁用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message,ex.ToString());
            }
		}




        #endregion

        #region 获取许可键值对集合
        /// <summary>
        /// 获取许可键值对集合
        /// </summary>
        /// <param name="title">默认显示数据</param>
        /// <returns></returns>
        public List<KeyValue<string, string>> SelectList(string title)
        {
            IQueryable<Permission> listPermission= _permissionRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<KeyValue<string, string>> list = new List<KeyValue<string, string>>();
            foreach (var permission in listPermission)
            {
                list.Add(new KeyValue<string, string>() {  Key=permission.Id.ToString(),Value=permission.PermissionName});
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new KeyValue<string, string>() { Key = "0", Value = title });
            }
            return list;
        }
        #endregion
    }
}
