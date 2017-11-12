
using AutoMapper;
using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class GoodsPurchasingService : ServiceBase, IGoodsPurchasingContract
    {
        private readonly IRepository<GoodsPurchasing, int> _GoodsPurchasingRepository;
        private readonly ICompanyGoodsCategoryContract _companyGoodsCategoryContract;
        public GoodsPurchasingService(
            IRepository<GoodsPurchasing, int> _GoodsPurchasingRepository,
            ICompanyGoodsCategoryContract companyGoodsCategoryContract
            ) : base(_GoodsPurchasingRepository.UnitOfWork)
        {
            this._GoodsPurchasingRepository = _GoodsPurchasingRepository;
            this._companyGoodsCategoryContract = companyGoodsCategoryContract;
        }

        public IQueryable<GoodsPurchasing> Entities
        {
            get
            {
                return _GoodsPurchasingRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _GoodsPurchasingRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params GoodsPurchasing[] entities)
        {
            foreach (var entity in entities)
            {
                var goods = _companyGoodsCategoryContract.Entities.FirstOrDefault(c => c.Id == entity.CompanyGoodsCategoryID && !c.IsDeleted && c.IsEnabled);
                if (goods == null)
                {
                    return new OperationResult(OperationResultType.Error, "物品不存在");
                }

                if ((!goods.IsUniqueness && (goods.ParentId == null || goods.ParentId == 0)) || (goods.IsUniqueness && goods.ParentId > 0))
                {
                    return new OperationResult(OperationResultType.Error, "无法为非唯一性的类别或唯一性物品添加采购记录，请选择非唯一性物品或唯一性类别");
                }

                //if (goods.IsUniqueness)
                //{
                //    return new OperationResult(OperationResultType.Error, "无法为唯一性物品添加采购记录");
                //}
            }

            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _GoodsPurchasingRepository.Insert(entities,
                entity =>
                {
                    var goods = _companyGoodsCategoryContract.Entities.FirstOrDefault(c => c.Id == entity.CompanyGoodsCategoryID && !c.IsDeleted && c.IsEnabled);
                    if (!goods.IsUniqueness)
                    {//只有非唯一性的物品需要相应改变公司物品的总数量
                        int gpsCount = _GoodsPurchasingRepository.Entities.Count(g => g.CompanyGoodsCategoryID == entity.Id);
                        if (gpsCount > 0 || (gpsCount == 0 && goods.TotalQuantity == 0))
                        {//当该物品非第一次进行添加采购记录操作或者该物品第一次进行采购记录添加操作且物品总数量为0时
                            goods.TotalQuantity = (goods.TotalQuantity ?? 0) + entity.Quantity;
                            goods.Parent.TotalQuantity = (goods.Parent.TotalQuantity ?? 0) + entity.Quantity;

                            _companyGoodsCategoryContract.Update(goods);
                        }
                    }

                    entity.AdminId = entity.AdminId == 0 ? (AuthorityHelper.OperatorId ?? 0) : entity.AdminId;
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _GoodsPurchasingRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params GoodsPurchasing[] entities)
        {
            foreach (var entity in entities)
            {
                var old = _GoodsPurchasingRepository.GetByKey(entity.Id);
                if (old == null)
                {
                    return new OperationResult(OperationResultType.Error, "记录不存在");
                }
                var good = _companyGoodsCategoryContract.Edit(entity.CompanyGoodsCategoryID);
                if (good == null)
                {
                    return new OperationResult(OperationResultType.Error, "物品不存在");
                }
                if (!good.IsUniqueness && _companyGoodsCategoryContract.GetFreeNumById(entity.CompanyGoodsCategoryID) < old.Quantity)
                {
                    return new OperationResult(OperationResultType.Error, "该物品已被使用");
                }
            }

            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _GoodsPurchasingRepository.Update(entities,
                entity =>
                {
                    var old = _GoodsPurchasingRepository.GetByKey(entity.Id);

                    var good = _companyGoodsCategoryContract.Edit(entity.CompanyGoodsCategoryID);
                    if (!good.IsUniqueness)
                    {
                        good.TotalQuantity -= old.Quantity;
                        good.TotalQuantity += entity.Quantity;

                        _companyGoodsCategoryContract.Update(good);
                    }

                    entity.AdminId = entity.AdminId > 0 ? old.AdminId : entity.AdminId;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public GoodsPurchasing View(int Id)
        {
            return _GoodsPurchasingRepository.GetByKey(Id);
        }

        public OperationResult Insert(params GoodsPurchasingDto[] dtos)
        {
            foreach (var dto in dtos)
            {
                var goods = _companyGoodsCategoryContract.Entities.FirstOrDefault(c => c.Id == dto.CompanyGoodsCategoryID && !c.IsDeleted && c.IsEnabled);
                if (goods == null)
                {
                    return new OperationResult(OperationResultType.Error, "物品不存在");
                }

                if ((!goods.IsUniqueness && (goods.ParentId == null || goods.ParentId == 0)) || (goods.IsUniqueness && goods.ParentId > 0))
                {
                    return new OperationResult(OperationResultType.Error, "无法为非唯一性的类别或唯一性物品添加采购记录，请选择非唯一性物品或唯一性类别");
                }

                //if (goods.IsUniqueness)
                //{
                //    return new OperationResult(OperationResultType.Error, "无法为唯一性物品添加采购记录");
                //}
            }

            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _GoodsPurchasingRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        var goods = _companyGoodsCategoryContract.Entities.FirstOrDefault(c => c.Id == entity.CompanyGoodsCategoryID && !c.IsDeleted && c.IsEnabled);
                        if (!goods.IsUniqueness)
                        {//只有非唯一性的物品需要相应改变公司物品的总数量
                            int gpsCount = _GoodsPurchasingRepository.Entities.Count(g => g.CompanyGoodsCategoryID == entity.CompanyGoodsCategoryID);
                            if (gpsCount > 0 || (gpsCount == 0 && goods.TotalQuantity == 0))
                            {//当该物品非第一次进行添加采购记录操作或者该物品第一次进行采购记录添加操作且物品总数量为0时
                                goods.TotalQuantity = (goods.TotalQuantity ?? 0) + entity.Quantity;
                                goods.Parent.TotalQuantity = (goods.Parent.TotalQuantity ?? 0) + entity.Quantity;

                                _companyGoodsCategoryContract.Update(goods);
                            }
                        }

                        entity.AdminId = entity.AdminId == 0 ? (AuthorityHelper.OperatorId ?? 0) : entity.AdminId;
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params GoodsPurchasingDto[] dtos)
        {
            foreach (var dto in dtos)
            {
                var old = _GoodsPurchasingRepository.GetByKey(dto.Id);
                if (old == null)
                {
                    return new OperationResult(OperationResultType.Error, "记录不存在");
                }
                var good = _companyGoodsCategoryContract.Edit(dto.CompanyGoodsCategoryID);
                if (good == null)
                {
                    return new OperationResult(OperationResultType.Error, "物品不存在");
                }
                if (!good.IsUniqueness && _companyGoodsCategoryContract.GetFreeNumById(dto.CompanyGoodsCategoryID) < old.Quantity)
                {
                    return new OperationResult(OperationResultType.Error, "该物品已被使用");
                }
            }

            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _GoodsPurchasingRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        var old = _GoodsPurchasingRepository.GetByKey(entity.Id);

                        var good = _companyGoodsCategoryContract.Edit(dto.CompanyGoodsCategoryID);
                        if (!good.IsUniqueness)
                        {
                            good.TotalQuantity -= old.Quantity;
                            good.TotalQuantity += entity.Quantity;

                            _companyGoodsCategoryContract.Update(good);
                        }

                        entity.AdminId = entity.AdminId == 0 ? old.AdminId : entity.AdminId;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public GoodsPurchasingDto Edit(int Id)
        {
            var entity = _GoodsPurchasingRepository.GetByKey(Id);
            Mapper.CreateMap<GoodsPurchasing, GoodsPurchasingDto>();
            var dto = Mapper.Map<GoodsPurchasing, GoodsPurchasingDto>(entity);
            return dto;
        }
    }
}

