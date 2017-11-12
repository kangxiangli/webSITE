
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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class CompanyGoodsCategoryService : ServiceBase, ICompanyGoodsCategoryContract
    {
        private readonly IRepository<CompanyGoodsCategory, int> _CompanyGoodsCategoryRepository;
        public CompanyGoodsCategoryService(
            IRepository<CompanyGoodsCategory, int> _CompanyGoodsCategoryRepository
            ) : base(_CompanyGoodsCategoryRepository.UnitOfWork)
        {
            this._CompanyGoodsCategoryRepository = _CompanyGoodsCategoryRepository;
        }

        public IQueryable<CompanyGoodsCategory> Entities
        {
            get
            {
                return _CompanyGoodsCategoryRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            ids.CheckNotNull("ids");
            var entities = _CompanyGoodsCategoryRepository.Entities.Where(m => ids.Contains(m.Id));

            foreach (var entity in entities)
            {
                if (entity.ParentId != null && entity.ParentId > 0)
                {//如果是子类
                    if (entity.Status == (int)CompanyGoodsCategoryStatusFlag.Use)
                    {//使用中不允许进行禁用操作
                        return new OperationResult(OperationResultType.Error, "使用中不允许进行禁用操作");
                    }
                    if (entity.Parent == null)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }
                }
                else
                {
                    if (!enable)
                    {
                        if (_CompanyGoodsCategoryRepository.Entities.Count(g => g.ParentId == entity.Id && g.IsEnabled && g.Status == (int)CompanyGoodsCategoryStatusFlag.Use) > 0)
                        {
                            return new OperationResult(OperationResultType.Error, "该类别存在使用中的物品，无法进行禁用操作");
                        }
                    }
                }
            }

            return OperationHelper.Try((opera) =>
        {
            UnitOfWork.TransactionEnabled = true;
            foreach (var entity in entities)
            {
                if (entity.ParentId != null && entity.ParentId > 0)
                {//如果是子类
                    var parent = _CompanyGoodsCategoryRepository.Entities.FirstOrDefault(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled);
                    int totalQuantity = parent.TotalQuantity ?? 0;
                    if (enable)
                    {//如果为启用操作
                        parent.TotalQuantity += entity.TotalQuantity;             //父类总数量增加相应数量
                    }
                    else
                    {//如果为禁用操作
                        parent.TotalQuantity -= entity.TotalQuantity;             //父类总数量减去相应数量
                    }
                    parent.TotalQuantity = totalQuantity;

                    this.Update(parent);
                }
                else
                {
                    if (!enable)
                    {
                        var subs = _CompanyGoodsCategoryRepository.Entities.Where(g => g.ParentId == entity.Id && g.IsEnabled);

                        foreach (var sub in subs)
                        {
                            entity.TotalQuantity = 0;
                            entity.UsedQuantity = 0;

                            entity.IsEnabled = enable;
                            entity.UpdatedTime = DateTime.Now;
                            entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        }
                    }
                }

                entity.IsEnabled = enable;
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
            }
            return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
        }, enable ? Operation.Enable : Operation.Disable);
        }

        #region 添加数据
        public OperationResult Insert(params CompanyGoodsCategory[] entities)
        {
            foreach (var entity in entities)
            {
                if (entity.ParentId != null && entity.ParentId > 0)
                {//如果是子类
                    if (this.Entities.Count(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled) == 0)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }
                }
            }

            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _CompanyGoodsCategoryRepository.Insert(entities,
                entity =>
                {
                    if (entity.ParentId != null && entity.ParentId > 0)
                    {//如果是子类
                        var parent = this.Entities.FirstOrDefault(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled);

                        entity.IsUniqueness = parent.IsUniqueness;
                        entity.Type = parent.Type;
                        if (entity.IsUniqueness)
                        {//且为唯一性物品时
                            entity.TotalQuantity = 1;         //数量固定为1
                            entity.ImgAddress = parent.ImgAddress;            //当为唯一性物品时，物品图片等于父类的图片
                        }

                        parent.TotalQuantity = (parent.TotalQuantity ?? 0) + entity.TotalQuantity;             //添加子类时父类总数量增加

                        this.Update(parent);
                    }
                    else
                    {//如果是父类
                        entity.ParentId = null;
                        entity.Price = 0;     //单价为0
                        entity.TotalQuantity = 0;         //父类初始总数量为0
                    }

                    entity.Status = (int)CompanyGoodsCategoryStatusFlag.Free;            //物品初始状态为0（空闲状态）
                    entity.UsedQuantity = 0;          //物品初始使用数量为0

                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }
        #endregion

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            ids.CheckNotNull("ids");
            var entities = _CompanyGoodsCategoryRepository.Entities.Where(m => ids.Contains(m.Id));

            foreach (var entity in entities)
            {
                if (entity.ParentId != null && entity.ParentId > 0)
                {//如果是子类
                    if (entity.Status == (int)CompanyGoodsCategoryStatusFlag.Use)
                    {//使用中不允许进行禁用操作
                        return new OperationResult(OperationResultType.Error, "使用中不允许进行删除操作");
                    }
                    if (entity.Parent == null)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }
                }
                else
                {
                    if (delete)
                    {
                        if (_CompanyGoodsCategoryRepository.Entities.Count(g => g.ParentId == entity.Id && !g.IsDeleted && g.Status == (int)CompanyGoodsCategoryStatusFlag.Use) > 0)
                        {
                            return new OperationResult(OperationResultType.Error, "该类别存在使用中的物品，无法进行删除操作");
                        }
                    }
                }
            }

            return OperationHelper.Try((opera) =>
            {
                UnitOfWork.TransactionEnabled = true;
                foreach (var entity in entities)
                {
                    if (entity.ParentId != null && entity.ParentId > 0)
                    {//如果是子类
                        var parent = _CompanyGoodsCategoryRepository.Entities.FirstOrDefault(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled);

                        if (delete)
                        {//如果为删除操作
                            parent.TotalQuantity = (parent.TotalQuantity ?? 0) - entity.TotalQuantity;             //父类总数量减去相应数量
                        }
                        else
                        {//如果为禁用操作
                            parent.TotalQuantity = (parent.TotalQuantity ?? 0) + entity.TotalQuantity;             //父类总数量增加相应数量
                        }

                        this.Update(parent);
                    }
                    else
                    {
                        if (delete)
                        {
                            var subs = _CompanyGoodsCategoryRepository.Entities.Where(g => g.ParentId == entity.Id && !g.IsDeleted);

                            foreach (var sub in subs)
                            {
                                entity.TotalQuantity = 0;
                                entity.UsedQuantity = 0;
                                entity.IsDeleted = delete;
                                entity.UpdatedTime = DateTime.Now;
                                entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                            }
                        }
                    }

                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        #region 更新数据
        public OperationResult Update(params CompanyGoodsCategory[] entities)
        {
            foreach (var entity in entities)
            {
                var old_goods = this.Entities.FirstOrDefault(c => c.Id == entity.Id && !c.IsDeleted && c.IsEnabled);
                if (old_goods == null)
                {
                    return new OperationResult(OperationResultType.Error, "该物品不存在");
                }

                if ((old_goods.ParentId == 0 && old_goods.ParentId != entity.ParentId) || (entity.ParentId == 0 && entity.ParentId != old_goods.ParentId))
                {
                    return new OperationResult(OperationResultType.Error, "物品和类别无法互相转换");
                }

                if (old_goods.ParentId != entity.ParentId && (old_goods.Parent.IsUniqueness != _CompanyGoodsCategoryRepository.GetByKey(entity.Id).IsUniqueness))
                {
                    return new OperationResult(OperationResultType.Error, "物品只能在唯一性相同的类别下转换");
                }

                if (old_goods.IsUniqueness != entity.IsUniqueness)
                {
                    return new OperationResult(OperationResultType.Error, "唯一性不可修改");
                }

                if (entity.ParentId != null && entity.ParentId > 0)
                {//如果是子类
                    var parent = this.Entities.FirstOrDefault(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled);
                    if (parent == null)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }

                    if (entity.Status == (int)CompanyGoodsCategoryStatusFlag.Use && entity.Status != old_goods.Status)
                    {
                        return new OperationResult(OperationResultType.Error, "使用中无法改变物品状态");
                    }
                }
            }

            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _CompanyGoodsCategoryRepository.Update(entities,
                entity =>
                {
                    var old = _CompanyGoodsCategoryRepository.GetByKey(entity.Id);
                    if (entity.ParentId > 0 && old.ParentId != entity.ParentId)
                    {

                        var old_parent = _CompanyGoodsCategoryRepository.GetByKey(old.ParentId ?? 0);
                        var new_parent = _CompanyGoodsCategoryRepository.GetByKey(entity.ParentId ?? 0);

                        entity.Type = new_parent.Type;
                        old_parent.TotalQuantity = (old_parent.TotalQuantity ?? 0) - entity.TotalQuantity;
                        old_parent.UsedQuantity = (old_parent.UsedQuantity ?? 0) - entity.UsedQuantity;

                        new_parent.TotalQuantity = (new_parent.TotalQuantity ?? 0) + entity.TotalQuantity;
                        new_parent.UsedQuantity = (new_parent.UsedQuantity ?? 0) + entity.UsedQuantity;

                        //entity.IsUniqueness = new_parent.IsUniqueness;
                        //entity.Type = new_parent.Type;
                        //if (entity.IsUniqueness)
                        //{
                        //    entity.TotalQuantity = 1;
                        //    entity.UsedQuantity = entity.Status == (int)CompanyGoodsCategoryStatusFlag.Use ? 1 : 0;
                        //    entity.ImgAddress = new_parent.ImgAddress;
                        //}

                        Update(old_parent);
                        Update(new_parent);
                    }

                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Update);
        }
        #endregion

        public CompanyGoodsCategory View(int Id)
        {
            return _CompanyGoodsCategoryRepository.GetByKey(Id);
        }

        public OperationResult Insert(params CompanyGoodsCategoryDto[] dtos)
        {
            foreach (var dto in dtos)
            {
                if (dto.ParentId != null && dto.ParentId > 0)
                {//如果是子类
                    if (this.Entities.Count(c => c.Id == dto.ParentId && !c.IsDeleted && c.IsEnabled) == 0)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }
                }
            }

            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _CompanyGoodsCategoryRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        if (entity.ParentId != null && entity.ParentId > 0)
                        {//如果是子类
                            var parent = this.Entities.FirstOrDefault(c => c.Id == entity.ParentId && !c.IsDeleted && c.IsEnabled);

                            entity.IsUniqueness = parent.IsUniqueness;
                            entity.Type = parent.Type;

                            if (entity.IsUniqueness)
                            {//且为唯一性物品时
                                entity.TotalQuantity = 1;         //数量固定为1
                                entity.ImgAddress = parent.ImgAddress;            //当为唯一性物品时，物品图片等于父类的图片
                            }

                            parent.TotalQuantity = (parent.TotalQuantity ?? 0) + entity.TotalQuantity;             //添加子类时父类总数量增加

                            this.Update(parent);
                        }
                        else
                        {//如果是父类
                            entity.ParentId = null;
                            entity.Price = 0;     //单价为0
                            entity.TotalQuantity = 0;         //父类初始总数量为0
                        }

                        entity.Status = (int)CompanyGoodsCategoryStatusFlag.Free;            //物品初始状态为0（空闲状态）
                        entity.UsedQuantity = 0;          //物品初始使用数量为0

                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }, Operation.Add);
        }

        #region 更新数据
        public OperationResult Update(params CompanyGoodsCategoryDto[] dtos)
        {
            foreach (var dto in dtos)
            {
                var old_goods = this.Entities.FirstOrDefault(c => c.Id == dto.Id && !c.IsDeleted && c.IsEnabled);
                if (old_goods == null)
                {
                    return new OperationResult(OperationResultType.Error, "该物品不存在");
                }

                if ((old_goods.ParentId == 0 && old_goods.ParentId != dto.ParentId) || (dto.ParentId == 0 && dto.ParentId != old_goods.ParentId))
                {
                    return new OperationResult(OperationResultType.Error, "物品和类别无法互相转换");
                }

                if (old_goods.ParentId != dto.ParentId && (old_goods.Parent.IsUniqueness != _CompanyGoodsCategoryRepository.GetByKey(dto.Id).IsUniqueness))
                {
                    return new OperationResult(OperationResultType.Error, "物品只能在唯一性相同的类别下转换");
                }

                if (old_goods.IsUniqueness != dto.IsUniqueness)
                {
                    return new OperationResult(OperationResultType.Error, "唯一性不可修改");
                }

                if (dto.ParentId != null && dto.ParentId > 0)
                {//如果是子类
                    var parent = this.Entities.FirstOrDefault(c => c.Id == dto.ParentId && !c.IsDeleted && c.IsEnabled);
                    if (parent == null)
                    {
                        return new OperationResult(OperationResultType.Error, "类别不存在");
                    }

                    if (dto.Status == (int)CompanyGoodsCategoryStatusFlag.Use && dto.Status != old_goods.Status)
                    {
                        return new OperationResult(OperationResultType.Error, "使用中无法改变物品状态");
                    }
                }
            }

            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _CompanyGoodsCategoryRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        var old = _CompanyGoodsCategoryRepository.GetByKey(entity.Id);
                        if (entity.ParentId > 0 && old.ParentId != entity.ParentId)
                        {

                            var old_parent = _CompanyGoodsCategoryRepository.GetByKey(old.ParentId ?? 0);
                            var new_parent = _CompanyGoodsCategoryRepository.GetByKey(entity.ParentId ?? 0);

                            entity.Type = new_parent.Type;
                            old_parent.TotalQuantity = (old_parent.TotalQuantity ?? 0) - entity.TotalQuantity;
                            old_parent.UsedQuantity = (old_parent.UsedQuantity ?? 0) - entity.UsedQuantity;

                            new_parent.TotalQuantity = (new_parent.TotalQuantity ?? 0) + entity.TotalQuantity;
                            new_parent.UsedQuantity = (new_parent.UsedQuantity ?? 0) + entity.UsedQuantity;

                            //entity.IsUniqueness = new_parent.IsUniqueness;
                            //if (entity.IsUniqueness)
                            //{
                            //    entity.TotalQuantity = 1;
                            //    entity.UsedQuantity = entity.Status == (int)CompanyGoodsCategoryStatusFlag.Use ? 1 : 0;
                            //    entity.ImgAddress = new_parent.ImgAddress;
                            //}

                            Update(old_parent);
                            Update(new_parent);
                        }

                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }, Operation.Update);
        }
        #endregion

        public CompanyGoodsCategoryDto Edit(int Id)
        {
            var entity = _CompanyGoodsCategoryRepository.GetByKey(Id);
            Mapper.CreateMap<CompanyGoodsCategory, CompanyGoodsCategoryDto>();
            var dto = Mapper.Map<CompanyGoodsCategory, CompanyGoodsCategoryDto>(entity);
            return dto;
        }

        /// <summary>
        /// 获取空闲物品的数量
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int GetFreeNumById(int Id)
        {
            var good = _CompanyGoodsCategoryRepository.Entities.FirstOrDefault(g => g.Id == Id && !g.IsDeleted && g.IsEnabled);
            if (good == null)
            {
                return 0;
            }
            if (good.ParentId > 0)
            {
                if (good.IsUniqueness)
                {
                    return good.Status == 0 ? 1 : 0;
                }
                else
                {
                    return (good.TotalQuantity ?? 0) - (good.UsedQuantity ?? 0);
                }
            }

            if (good.IsUniqueness)
            {
                return _CompanyGoodsCategoryRepository.Entities.Count(g => g.Status == 0 && !g.IsDeleted && g.IsEnabled);
            }

            return _CompanyGoodsCategoryRepository.Entities.Where(g => g.ParentId == Id && !g.IsDeleted && g.IsEnabled).Sum(g =>
                g.TotalQuantity - g.UsedQuantity
            ) ?? 0;
        }
    }
}

