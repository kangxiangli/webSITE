﻿
//   This file was generated by T4 code generator Implement_Creater.tt.



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

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class PurchaseItemService : ServiceBase, IPurchaseItemContract
    {
        #region PurchaseItemService

		private readonly IRepository<PurchaseItem, int> _purchaseitemRepository;

        private readonly IRepository<PurchaseItemProduct, int> _purchaseItemProductRepository;

        private readonly IRepository<Inventory, int> _inventoryRepository;

		public PurchaseItemService(
			IRepository<PurchaseItem, int> purchaseitemRepository,
            IRepository<PurchaseItemProduct, int> purchaseItemProductRepository,
            IRepository<Inventory, int> inventoryRepository
		): base(purchaseitemRepository.UnitOfWork)
		{
			_purchaseitemRepository = purchaseitemRepository;
            _purchaseItemProductRepository = purchaseItemProductRepository;
            _inventoryRepository = inventoryRepository;
		}


        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public PurchaseItem View(int Id){
			var entity=_purchaseitemRepository.GetByKey(Id);
            return entity;
		}


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public PurchaseItemDto Edit(int Id){
			var entity=_purchaseitemRepository.GetByKey(Id);
            Mapper.CreateMap<PurchaseItem, PurchaseItemDto>();
            var dto = Mapper.Map<PurchaseItem, PurchaseItemDto>(entity);
            return dto;
		}


        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<PurchaseItem> PurchaseItems { get { return _purchaseitemRepository.Entities; } }



        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<PurchaseItem, bool>> predicate, int id = 0)
        {
            return _purchaseitemRepository.ExistsCheck(predicate, id);
        }



        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params PurchaseItemDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _purchaseitemRepository.Insert(dtos,
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


        public OperationResult Insert(params PurchaseItem[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _purchaseitemRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params PurchaseItemDto[] dtos)
        {
            try
            {
				dtos.CheckNotNull("dtos");
				OperationResult result = _purchaseitemRepository.Update(dtos,
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

        public OperationResult Update(params PurchaseItem[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _purchaseitemRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
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
				var entities = _purchaseitemRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_purchaseitemRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "移除成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
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
				var entities = _purchaseitemRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsDeleted = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_purchaseitemRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "恢复成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
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
				OperationResult result = _purchaseitemRepository.Delete(ids);
				return result;
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
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
				var entities = _purchaseitemRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = true;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_purchaseitemRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "启用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
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
				var entities = _purchaseitemRepository.Entities.Where(m => ids.Contains(m.Id));
				foreach (var entity in entities) {
					entity.IsEnabled = false;
					entity.UpdatedTime = DateTime.Now;
					entity.OperatorId=AuthorityHelper.OperatorId;
					_purchaseitemRepository.Update(entity);
				}
				return UnitOfWork.SaveChanges() > 0? new OperationResult(OperationResultType.Success, "禁用成功！"): new OperationResult(OperationResultType.NoChanged,"数据没有变化！");
            }catch (Exception ex){
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }


        #region 添加数据-重载
        /// <summary>
        /// 
        /// </summary>
        /// <param name="purchaseItemProduct"></param>
        /// <param name="inventory"></param>
        /// <returns></returns>
        public OperationResult Insert(PurchaseItemProduct[] purchaseItemProduct, Inventory[] inventory)
        {
            UnitOfWork.TransactionEnabled = true;
            _purchaseItemProductRepository.Insert(purchaseItemProduct.AsEnumerable());
            _inventoryRepository.Update(inventory);
            int resultCount = UnitOfWork.SaveChanges();
            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (resultCount==0)
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "添加失败";
            }
            return oper;
        }
        #endregion


        #endregion


        public OperationResult Insert(params PurchaseItemProduct[] pro)
        {
            int resultCount = _purchaseItemProductRepository.Insert(pro.AsEnumerable());
            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (resultCount == 0)
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "添加失败";
            }
            return oper;
        }
    }
}