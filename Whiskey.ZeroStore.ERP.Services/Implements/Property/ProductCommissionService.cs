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
    public class ProductCommissionService : ServiceBase, IProductCommissionContract
    {
        #region 声明数据层操作对象

        private readonly IRepository<ProductCommission, int> _productCommissionRepository;

        public ProductCommissionService(
            IRepository<ProductCommission, int> productCommissionRepository)
            : base(productCommissionRepository.UnitOfWork)
		{
            _productCommissionRepository = productCommissionRepository;
		}
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Insert(params ProductCommissionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<ProductCommission> listProductCommission = ProductCommissions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listProductCommission.Where(x => x.StoreId == dto.StoreId && x.BrandId == dto.BrandId && x.SeasonId == dto.SeasonId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "已经对该店铺下同品牌同季节添加了提成");
                    }
                    int index = listProductCommission.Where(x => x.CommissionName == dto.CommissionName).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "提成名称已经存在");
                    }
                }
                OperationResult result = _productCommissionRepository.Insert(dtos,
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

        #region 获取编辑对象

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductCommissionDto Edit(int Id)
        {
            var entity = _productCommissionRepository.GetByKey(Id);
            Mapper.CreateMap<ProductCommission, ProductCommissionDto>();
            var dto = Mapper.Map<ProductCommission, ProductCommissionDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>        
        public IQueryable<ProductCommission> ProductCommissions
        {
            get { return _productCommissionRepository.Entities; }
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Update(params ProductCommissionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<ProductCommission> listProductCommission = ProductCommissions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listProductCommission.Where(x => x.BrandId == dto.BrandId && x.StoreId == x.StoreId && x.SeasonId == x.SeasonId && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "已经对该店铺下同品牌同季节添加了提成");
                    }
                    int index = listProductCommission.Where(x => x.CommissionName == dto.CommissionName && x.Id != dto.Id).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在");
                    }
                }
                OperationResult result = _productCommissionRepository.Update(dtos,
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

        #region 查看数据
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductCommission View(int Id)
        {
            ProductCommission productCommission = _productCommissionRepository.GetByKey(Id);
            return productCommission;
        }
        #endregion

        #region 移除数据

        /// <summary>
        ///移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productCommissionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _productCommissionRepository.Update(entity);
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
                var entities = _productCommissionRepository.Entities.Where(m => ids.Contains(m.Id));
                IQueryable<ProductCommission> listProductDis = ProductCommissions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    int count = listProductDis.Where(x => x.BrandId == entity.BrandId && x.StoreId == entity.StoreId && x.SeasonId == entity.SeasonId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "正常数据已经存在同店铺，同品牌和同季节数据，无法恢复");
                    }
                    int index = listProductDis.Where(x => x.CommissionName == entity.CommissionName).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "正常数据已经存在同名称数据，无法恢复");
                    }
                    _productCommissionRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

    }
}
