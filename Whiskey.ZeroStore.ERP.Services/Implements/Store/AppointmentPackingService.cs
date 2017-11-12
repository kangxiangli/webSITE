
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppointmentPackingService : ServiceBase, IAppointmentPackingContract
    {

        private readonly IRepository<AppointmentPacking, int> _repo;
        private readonly IRepository<ProductTrack, int> _trackRepo;
        private readonly IInventoryContract _inventoryContract;


        public AppointmentPackingService(IRepository<AppointmentPacking, int> repo,
            IRepository<ProductTrack, int> trackRepo,
            IInventoryContract inventoryContract
           
                     ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _trackRepo = trackRepo;
            _inventoryContract = inventoryContract;
        }
        public IQueryable<AppointmentPacking> Entities => _repo.Entities;


        public OperationResult Insert(params AppointmentPacking[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params AppointmentPacking[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<AppointmentPacking> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public AppointmentPacking Edit(int id)
        {
            return _repo.GetByKey(id);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public AppointmentPacking View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params AppointmentPacking[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }

        public OperationResult AddBarcode(int adminId, AddBarcodeReq dto)
        {
            if (dto.ToStorageId <= 0)
            {
                throw new Exception("请选择收货仓库");
            }

            var entity = _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == dto.Id).Include(e => e.AppointmentPackingItem).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception("数据异常");
            }


            if (!entity.ToStorageId.HasValue)
            {
                entity.ToStorageId = dto.ToStorageId;
            }
            else
            {
                if (entity.ToStorageId != dto.ToStorageId)
                {
                    throw new Exception("收货仓库与已选择仓库冲突");
                }
            }
            var inventoryEntities = _inventoryContract.Inventorys.Where(i => !i.IsDeleted && i.IsEnabled && dto.ProductBarcodes.Contains(i.ProductBarcode));

            var chkRes = _inventoryContract.CheckBarcodes(InventoryCheckContext.配货, entity.FromStoreId, entity.FromStorageId, adminId, dto.ProductBarcodes);
            if (!chkRes.Any(i => i.Value.Item1))
            {
                return new OperationResult(OperationResultType.Success, string.Empty, chkRes);
            }
            var errorDic = new Dictionary<string, Tuple<bool, string, string>>();
            using (var transaction = _repo.GetTransaction())
            {
                var trackToSave = new List<ProductTrack>();
                var inventoryToUpdate = new List<Inventory>();

                // double check
                foreach (var chkItem in chkRes.Where(p => p.Value.Item1))
                {
                    var inventoryEntity = inventoryEntities.FirstOrDefault(i => i.ProductBarcode == chkItem.Key);
                    var item = entity.AppointmentPackingItem.FirstOrDefault(p => p.ProductNumber == inventoryEntity.ProductNumber);
                    if (!string.IsNullOrEmpty(item.ProductBarcode) && item.ProductBarcode != inventoryEntity.ProductBarcode)
                    {

                        errorDic[chkItem.Key] = Tuple.Create(false, $"此流水号与流水号{item.ProductBarcode}冲突,请先移除之前的流水号", string.Empty);
                        continue;
                    }
                    item.ProductBarcode = inventoryEntity.ProductBarcode;
                    item.UpdatedTime = DateTime.Now;

                    inventoryToUpdate.Add(inventoryEntity);
                    trackToSave.Add(new ProductTrack
                    {
                        ProductBarcode = inventoryEntity.ProductBarcode,
                        ProductNumber = inventoryEntity.ProductNumber,
                        Describe = string.Format(ProductOptDescTemplate.ON_APPOINTMENT_PACKING_ADD, entity.FromStore.StoreName, entity.FromStorage.StorageName),
                        UpdatedTime = DateTime.Now,
                        CreatedTime = DateTime.Now
                    });

                }


                // update check result
                foreach (var item in errorDic)
                {
                    if (chkRes.ContainsKey(item.Key))
                    {
                        chkRes[item.Key] = item.Value;
                    }
                }

                if (!chkRes.Any(i => i.Value.Item1))
                {
                    return new OperationResult(OperationResultType.Success, string.Empty, chkRes);
                }


                // persisit state
                var cnt = _repo.Update(entity);
                if (cnt <= 0)
                {
                    throw new Exception("状态更新失败");
                }


                inventoryToUpdate.Each(i =>
                {
                    i.Status = InventoryStatus.Purchased;
                    i.Others = "预约采购单扫码锁定";
                });

                var res = _inventoryContract.Update(inventoryToUpdate.ToArray());
                if (res.ResultType != OperationResultType.Success)
                {
                    throw new Exception("库存状态更新失败");
                }

                cnt = _trackRepo.Insert(trackToSave.AsEnumerable());
                if (cnt <= 0)
                {
                    throw new Exception("追踪记录保存失败");
                }
                transaction.Commit();
                return new OperationResult(OperationResultType.Success, string.Empty, chkRes);
            }

        }

        public OperationResult RemoveBarcode(int adminId, RemoveBarcodeReq dto)
        {


            var entity = _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == dto.Id).Include(e => e.AppointmentPackingItem).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception("数据异常");
            }


            var inventoryEntity = _inventoryContract.Inventorys.FirstOrDefault(i => !i.IsDeleted && i.IsEnabled && dto.ProductBarcode == i.ProductBarcode);


            var errorDic = new Dictionary<string, Tuple<bool, string>>();
            using (var transaction = _repo.GetTransaction())
            {

                // 处理校验成功的
                var item = entity.AppointmentPackingItem.FirstOrDefault(p => p.ProductNumber == inventoryEntity.ProductNumber);
                if (!string.IsNullOrEmpty(item.ProductBarcode) && item.ProductBarcode != inventoryEntity.ProductBarcode)
                {

                    return OperationResult.Error($"此流水号与之前的已配货的流水号{item.ProductBarcode}不一致");
                }

                item.ProductBarcode = null;
                item.UpdatedTime = DateTime.Now;

                var cnt = _repo.Update(entity);
                if (cnt <= 0)
                {
                    throw new Exception("状态更新失败");
                }

                // 库存解锁
                inventoryEntity.Status = InventoryStatus.Default;
                inventoryEntity.Others = "从预约采购单移除解锁";
                var res = _inventoryContract.Update(inventoryEntity);
                if (res.ResultType != OperationResultType.Success)
                {
                    throw new Exception("库存状态更新失败");
                }

                // 追踪记录
                var track = new ProductTrack
                {
                    ProductBarcode = inventoryEntity.ProductBarcode,
                    ProductNumber = inventoryEntity.ProductNumber,
                    Describe = string.Format(ProductOptDescTemplate.ON_APPOINTMENT_PACKING_DROP, entity.FromStore.StoreName, entity.FromStorage.StorageName)
                };

                cnt = _trackRepo.Insert(track);
                if (cnt <= 0)
                {
                    throw new Exception("追踪记录保存失败");
                }

                transaction.Commit();
                return OperationResult.OK();
            }
        }



    }
}
