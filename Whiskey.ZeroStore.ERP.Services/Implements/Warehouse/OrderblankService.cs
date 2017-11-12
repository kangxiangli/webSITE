using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Web.Helper;
using AutoMapper;
using System.Web.Mvc;
using System.Linq.Expressions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class OrderblankService : ServiceBase, IOrderblankContract
    {
        #region 初始化操作对象

        private readonly IRepository<Orderblank, int> _orderblankRepository;

        private readonly IRepository<OrderblankAudit, int> _orderblankAuditRepository;

        private readonly IRepository<Inventory, int> _inventoryRepository;
        private readonly IRepository<AppointmentPacking, int> _packingRepo;
        private readonly IRepository<Appointment, int> _appointmentRepo;
        private readonly IStoreContract _storeContract;
        protected readonly ITimeoutSettingContract _timeoutSettingContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IOrderblankItemContract _orderblankItemContract;
        private readonly IProductTrackContract _productTrackContract;
        private readonly IPurchaseContract _purchaseContract;
        private readonly IPurchaseItemContract _purchaseItemContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IStorageContract _storageContract;
        private readonly IAppointmentPackingContract _appointmentPackingContract;
        private readonly IPunishScoreRecordContract _adminPunishScoreRecordContract;

        protected readonly ITimeoutRequestContract _timeoutRequestContract;

        public OrderblankService(IRepository<Orderblank, int> orderblankRepository
            , IRepository<OrderblankAudit, int> orderblankAuditRepository
            , IRepository<Inventory, int> inventoryRepository
            , IRepository<AppointmentPacking, int> packingRepo
            , IRepository<Appointment, int> appointmentRepo
           , ITimeoutSettingContract timeoutSettingContract
            , IMemberContract memberContract
            , IAdministratorContract adminContract
            , IOrderblankItemContract orderblankItemContract
            , IStoreContract storeContract
            , IProductTrackContract productTrackContract
            , IPurchaseContract purchaseContract
            , IPurchaseItemContract purchaseItemContract
            , IInventoryContract inventoryContract,
            IStorageContract storageContract
            , IAppointmentPackingContract appointmentPackingContract,
            IPunishScoreRecordContract adminPunishScoreRecordContract,
             ITimeoutRequestContract timeoutRequestContract
            )
            : base(orderblankRepository.UnitOfWork)
        {
            _orderblankRepository = orderblankRepository;
            _orderblankAuditRepository = orderblankAuditRepository;
            _inventoryRepository = inventoryRepository;
            _packingRepo = packingRepo;
            _appointmentRepo = appointmentRepo;
            _memberContract = memberContract;
            _timeoutSettingContract = timeoutSettingContract;
            _adminContract = adminContract;
            _orderblankItemContract = orderblankItemContract;
            _storeContract = storeContract;
            _productTrackContract = productTrackContract;
            _purchaseContract = purchaseContract;
            _purchaseItemContract = purchaseItemContract;
            _inventoryContract = inventoryContract;
            _storageContract = storageContract;
            _appointmentPackingContract = appointmentPackingContract;
            _adminPunishScoreRecordContract = adminPunishScoreRecordContract;
            _timeoutRequestContract = timeoutRequestContract;

        }
        #endregion

        #region 获取数据集

        public IQueryable<Orderblank> Orderblanks
        {
            get { return _orderblankRepository.Entities; }
        }
        #endregion

        public OperationResult Insert(Orderblank[] ordeblankArr)
        {
            return _orderblankRepository.Insert((IEnumerable<Orderblank>)ordeblankArr) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Delete(int id)
        {
            //软删除操作
            //1.删除item
            //2.删除配货单
            //2.解锁库存

            // 获取配货单
            var orderblankEntity = _orderblankRepository.Entities.Where(o => id == o.Id).FirstOrDefault();
            if (orderblankEntity == null)
            {
                return new OperationResult(OperationResultType.Error, "配货单信息有误");
            }

            if (orderblankEntity.Status != OrderblankStatus.配货中)
            {
                return new OperationResult(OperationResultType.Error, "配货单必须在配货状态才能撤销");
            }


            var chkRes = CheckOrderbalnkAction(orderblankEntity, OrderblankAction.Delete);
            if (chkRes.ResultType != OperationResultType.Success)
            {
                return chkRes;
            }

            // 获取明细
            var itemList = orderblankEntity.OrderblankItems.ToList();

            // 获取库存
            var barcodes = itemList.SelectMany(i => i.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            var inventoryList = _inventoryRepository.Entities.Where(i => i.IsEnabled && !i.IsDeleted && barcodes.Contains(i.ProductBarcode)).ToList();
            var settingEntity = _timeoutSettingContract.GetTimeoutSettingForOrderblank(OrderblankAction.Delete);

            // 判断是否有超时的配货单
            var hasTimeout = false;
            if (CheckTimeout(orderblankEntity.CreatedTime, settingEntity))
            {
                hasTimeout = true;
            }
            using (var transaction = GetTransaction())
            {

                // 超时扣积分
                if (hasTimeout)
                {
                    TimeoutProcess(PunishTypeEnum.配货单撤销超时, orderblankEntity.OrderBlankNumber, settingEntity.DeductScore);
                }

                // 删除明细
                if (itemList.Count > 0)
                {
                    itemList.Each(i => i.IsDeleted = true);
                    var res = _orderblankItemContract.Update(itemList.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "删除配货明细失败");
                    }
                }


                // 删除配货单
                orderblankEntity.IsDeleted = true;
                orderblankEntity.Status = OrderblankStatus.已撤销;
                var res2 = _orderblankRepository.Update(orderblankEntity);
                if (res2 <= 0)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "撤销配货单失败");
                }
                if (inventoryList.Count > 0)
                {
                    // 库存解锁
                    inventoryList.Each(i =>
                    {
                        i.IsLock = false;
                        i.Status = (int)InventoryStatus.Default;
                        i.UpdatedTime = DateTime.Now;
                        i.OperatorId = AuthorityHelper.OperatorId;
                    });
                    var res = _inventoryRepository.UpdateBulk(inventoryList);
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "库存状态还原失败");
                    }

                    res = LogWhenOrderblankDrop(inventoryList.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "日志保存失败");
                    }
                }
                transaction.Commit();
                return new OperationResult(OperationResultType.Success, string.Empty);

            }

        }

        public Utility.Data.OperationResult Update(Orderblank[] orderblankArr)
        {
            return _orderblankRepository.Update(orderblankArr);
        }

        #region 更新配货状态
        public OperationResult UpdateCheckstat(string Number)
        {
            Orderblank orderblank = _orderblankRepository.Entities.FirstOrDefault(x => x.OrderBlankNumber == Number);
            int count = 0;
            if (orderblank != null)
            {
                orderblank.UpdatedTime = DateTime.Now;
                count = _orderblankRepository.Update(orderblank);
            }
            return count > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error, "盘点失败");
        }
        #endregion



        #region 添加数据-重载
        public OperationResult Insert(bool checkUnFinished, params OrderblankDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                GetNumber(dtos);

                // 正在盘点中的店铺不可创建配货单
                var storesInChecking = _storeContract.GetStoresInChecking().ToList();
                if (dtos.Any(dto => storesInChecking.Contains(dto.OutStoreId)))
                {
                    return new OperationResult(OperationResultType.Error, "发货店铺正在盘点中,无法创建配货单");
                }

                // 校验是否有未完成的配货单
                var outStoreId = dtos.Select(o => o.OutStoreId).ToList();

                if (checkUnFinished && _orderblankRepository.Entities.Any(o => o.IsEnabled
                                                         && !o.IsDeleted
                                                         && outStoreId.Contains(o.OutStoreId)
                                                         && o.Status == OrderblankStatus.配货中))
                {
                    return new OperationResult(OperationResultType.Error, "出货店铺中有未完成的配货单，无法创建新的配货单");
                }


                var result = _orderblankRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                result.Data = dtos.First().OrderBlankNumber;
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        /// <summary>
        /// 生成配货单的编码
        /// </summary>
        private void GetNumber(params OrderblankDto[] dtos)
        {
            IQueryable<Orderblank> listAppArticleItem = this.Orderblanks;
            string strNumber = string.Empty;
            bool isHave = false;
            List<string> listNumber = new List<string>();
            //是否有数据
            isHave = listAppArticleItem.Any();
            if (isHave == true)
            {
                listNumber = listAppArticleItem.Select(x => x.OrderBlankNumber).ToList();
            }
            foreach (OrderblankDto dto in dtos)
            {
                while (true)
                {
                    strNumber = RandomHelper.GetRandomCode(10);
                    //先校验在自己的数组内是否用重复
                    isHave = dtos.Any(x => x.OrderBlankNumber == strNumber);
                    if (isHave == false)
                    {
                        //再校验数据库的数据
                        isHave = listNumber.Any(x => x == strNumber);
                        if (isHave == false)
                        {
                            dto.OrderBlankNumber = strNumber;
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region 拒绝配货--重载
        /// <summary>
        /// 是否超时判断
        /// </summary>
        /// <param name="referTimePoint">参照时间点</param>
        /// <param name="settingEntity">超时配置信息</param>
        /// <returns></returns>
        private bool CheckTimeout(DateTime referTimePoint, TimeoutSetting settingEntity)
        {
            if (settingEntity == null)
            {
                return false;
            }
            var timeoutDate = referTimePoint.Add(TimeSpan.FromSeconds(settingEntity.TimeSpan));
            return DateTime.Now > timeoutDate;
        }

        public void TimeoutProcess(PunishTypeEnum punishType, string number, int deductScore)
        {
            // 判断是否申请了超时
            var requestQuery = _timeoutRequestContract.Entities.Where(t => !t.IsDeleted && t.IsEnabled)
                .Where(t => !t.IsUsed && t.State == TimeoutRequestState.已通过)
                .Where(t => t.RequestAdminId == AuthorityHelper.OperatorId.Value && t.Number == number)
                .FirstOrDefault();
            if (requestQuery != null)
            {
                requestQuery.IsUsed = true;
                var hasUpdate = _timeoutRequestContract.Update(requestQuery);
                if (hasUpdate.ResultType != OperationResultType.Success)
                {
                    throw new Exception("超时申请单状态更新失败");
                }
            }
            else
            {
                SavePunishRecord(punishType, number, deductScore);
            }
        }

        private void SavePunishRecord(PunishTypeEnum punishType, string number, int deductScore)
        {
            var optId = AuthorityHelper.OperatorId;
            if (!optId.HasValue)
            {
                throw new Exception("请重新登录");
            }
            var operatorAdmin = _adminContract.Administrators.Where(a => a.Id == optId.Value).Include(a => a.Member).FirstOrDefault();
            if (operatorAdmin == null)
            {
                throw new Exception("请重新登录");
            }
            var memberEntity = operatorAdmin.Member;
            if (memberEntity.Score < deductScore)
            {
                throw new Exception("积分不足,无法继续操作");
            }

            //扣掉积分
            memberEntity.Score -= deductScore;

            var memberDto = Mapper.Map<MemberDto>(memberEntity);
            var updateRes = _memberContract.UpdateScore(memberDto);
            if (updateRes.ResultType != OperationResultType.Success)
            {
                throw new Exception("积分扣除失败,无法继续操作");
            }

            // 记录扣除信息
            var punishRecord = new PunishScoreRecord()
            {
                PunishAdminId = AuthorityHelper.OperatorId.Value,
                OperatorId = AuthorityHelper.OperatorId.Value,
                PunishScore = deductScore,
                PunishType = punishType,
                Remarks = number
            };

            var hasSave = _adminPunishScoreRecordContract.Insert(punishRecord);
            if (hasSave.ResultType != OperationResultType.Success)
            {
                throw new Exception("积分扣除记录保存失败,无法继续操作");
            }
        }

        public OperationResult Refuse(string orderblankNum, string refuseReason, Action<OrderblankNoticeModel> sendNotice)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问");
            try
            {
                if (string.IsNullOrEmpty(orderblankNum) || string.IsNullOrEmpty(refuseReason))
                {
                    oper.Message = "未找到配货单记录或者拒绝理由为空";
                    return oper;
                }

                var orderblankEntity = Orderblanks.FirstOrDefault(c => c.OrderBlankNumber == orderblankNum && c.IsDeleted == false && c.IsEnabled == true);
                if (orderblankEntity == null)
                {
                    oper.Message = "未找到配货单记录";
                    return oper;
                }
                if (orderblankEntity.Status != OrderblankStatus.发货中)
                {
                    //未配货->拒绝配货，如果状态不为0，表示可能其他的用户已经修改了该配货单，则需要客户端刷新，拒绝本次操作
                    oper = new OperationResult(OperationResultType.Error, "操作失败：其他用户已经修改了当前配货单或当前配货单被锁定，请重新获取数据");
                    return oper;
                }

                var chkRes = CheckOrderbalnkAction(orderblankEntity, OrderblankAction.Reject);
                if (chkRes.ResultType != OperationResultType.Success)
                {
                    return chkRes;
                }


                var orderblankItemsFromDb = orderblankEntity.OrderblankItems.ToList();
                var barcodseFromDb = orderblankItemsFromDb.SelectMany(o => o.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
                var listInventory = _inventoryRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                                                                 .Where(x => barcodseFromDb.Contains(x.ProductBarcode))
                                                                 .ToList();
                var settingEntity = _timeoutSettingContract.GetTimeoutSettingForOrderblank(OrderblankAction.Reject);
                var timeout = CheckTimeout(orderblankEntity.DeliveryTime.Value, settingEntity);
                using (var transaction = GetTransaction())
                {
                    //超时扣积分
                    if (timeout)
                    {
                        TimeoutProcess(PunishTypeEnum.配货单拒绝收货超时, orderblankEntity.OrderBlankNumber, settingEntity.DeductScore);
                    }
                    // 拒绝收货后,配货单状态回滚到[配货中]状态,
                    orderblankEntity.Status = OrderblankStatus.配货中;

                    // 配货单的发货人,发货时间重置
                    orderblankEntity.DeliverAdminId = null;
                    orderblankEntity.DeliveryTime = null;

                    // 配货单创建时间更新为当前时间,用于作为下次发货判断超时的参照时间点
                    var currentTime = DateTime.Now;
                    orderblankEntity.CreatedTime = currentTime;
                    orderblankEntity.UpdatedTime = currentTime;

                    var res = Update(new Orderblank[] { orderblankEntity });
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "配货单状态更新失败" + res.Message);
                    }

                    // 拒绝收货后,配货单状态回滚到[配货锁定]状态,
                    listInventory.Each(i =>
                    {
                        i.Status = InventoryStatus.DeliveryLock;
                        i.UpdatedTime = currentTime;
                        i.OperatorId = AuthorityHelper.OperatorId;
                    });

                    res = _inventoryRepository.UpdateBulk(listInventory);
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "拒绝退货失败，请稍后再试" + res.Message);
                    }
                    var notice = new OrderblankNoticeModel()
                    {
                        IsReject = true,
                        OrderblankNumer = orderblankEntity.OrderBlankNumber,
                        OutStorageId = orderblankEntity.ReceiverStorageId,
                        ReceiverStorageId = orderblankEntity.OutStorageId,
                        RejectReason = refuseReason
                    };
                    res = LogWhenOrderblankRefuse(orderblankEntity, listInventory.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, "日志保存失败，请稍后再试" + res.Message);
                    }
                    sendNotice(notice);
                    transaction.Commit();
                    return new OperationResult(OperationResultType.Success, string.Empty);
                }

            }
            catch (Exception)
            {
                oper.Message = "服务器忙，请稍后重试";
            }
            return oper;
        }

        public DbContextTransaction GetTransaction()
        {
            return _orderblankRepository.GetTransaction();
        }

        public OperationResult LogWhenOrderblankRemove(params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0)
            {
                throw new Exception("记录不能为空");
            }
            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = ProductOptDescTemplate.ON_ORDERBLANK_REMOVE
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        public OperationResult LogWhenOrderblankAdd(Orderblank orderblankEntity, params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0 || orderblankEntity == null)
            {
                throw new Exception("记录不能为空");
            }
            var strDesc = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_ADD, orderblankEntity.OutStore.StoreName, orderblankEntity.ReceiverStore.StoreName, orderblankEntity.OrderBlankNumber);
            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = strDesc
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        public OperationResult LogWhenOrderblankDelivery(Orderblank orderblankEntity, params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0 || orderblankEntity == null)
            {
                throw new Exception("记录不能为空");
            }
            var strDesc = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_DELIVERY, orderblankEntity.OutStore.StoreName, orderblankEntity.ReceiverStore.StoreName);
            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = strDesc
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        public OperationResult LogWhenOrderblankRefuse(Orderblank orderblankEntity, params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0 || orderblankEntity == null)
            {
                throw new Exception("记录不能为空");
            }
            var strDesc = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_REFUSE, orderblankEntity.ReceiverStore.StoreName, orderblankEntity.OutStore.StoreName);
            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = strDesc
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        public OperationResult LogWhenOrderblankAccept(Orderblank orderblankEntity, params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0 || orderblankEntity == null)
            {
                throw new Exception("记录不能为空");
            }
            var strDesc = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_ACCEPT, orderblankEntity.ReceiverStore.StoreName);
            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = strDesc
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        public OperationResult LogWhenOrderblankDrop(params Inventory[] inventoryList)
        {
            if (inventoryList == null || inventoryList.Length <= 0)
            {
                throw new Exception("记录不能为空");
            }

            var logs = inventoryList.Select(i => new ProductTrack
            {
                ProductNumber = i.ProductNumber,
                ProductBarcode = i.ProductBarcode,
                Describe = ProductOptDescTemplate.ON_ORDERBLANK_DROP
            }).ToArray();

            var res = _productTrackContract.BulkInsert(logs);
            return res;
        }
        #endregion

        /// <summary>
        /// 获取指定仓库下未完成配货单的数量
        /// </summary>
        /// <param name="storageIds"></param>
        /// <returns></returns>
        public OperationResult<int> GetInCompleteOrderblankCount(int[] storageIds)
        {
            var count = _orderblankRepository.Entities
                .Where(o => !o.IsDeleted && o.IsEnabled)
                .Where(o => storageIds.Contains(o.OutStorageId) || storageIds.Contains(o.ReceiverStorageId))
                .Where(o => o.Status == OrderblankStatus.配货中 || o.Status == OrderblankStatus.发货中).Count();
            return new OperationResult<int>(OperationResultType.Success, string.Empty, count);
        }


        /// <summary>
        /// 采购单生成配货单
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        public OperationResult SaveOrderblankFromPurchaseOrder(int? purchaseId)
        {
            if (!purchaseId.HasValue)
            {
                return new OperationResult(OperationResultType.Error, "参数错误");
            }

            var purchaseEntity = _purchaseContract.Purchases.Where(p => !p.IsDeleted && p.IsEnabled && p.Id == purchaseId.Value).FirstOrDefault();
            if (purchaseEntity == null)
            {
                return new OperationResult(OperationResultType.Error, "采购单未找到");
            }
            //校验是否已生成orderblank
            if (purchaseEntity.Orderblanks.Any())
            {
                return new OperationResult(OperationResultType.Error, "采购单已有配货单，无法重复生成");
            }

            //校验item
            var purchaseItems = _purchaseItemContract.PurchaseItems.Where(i => i.PurchaseId.Value == purchaseEntity.Id).Include(i => i.PurchaseItemProducts).ToList();


            if (!purchaseItems.Any())
            {
                return new OperationResult(OperationResultType.Error, "采购单中没有商品");
            }

            var purchaseBarcodes = purchaseItems.SelectMany(i => i.PurchaseItemProducts.Select(p => p.ProductBarcode)).ToList();

            //校验barcodes
            var inventoryListFromDb = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                 .Where(i => !i.IsDeleted && i.IsEnabled)
                                                 .Where(i => i.StorageId == purchaseEntity.StorageId.Value)
                                                 .Where(i => purchaseBarcodes.Contains(i.ProductBarcode))
                                                 .ToList();
            if (purchaseBarcodes.Count != inventoryListFromDb.Count)
            {
                return new OperationResult(OperationResultType.Error, "库存数量不一致");
            }
            var modelList = purchaseBarcodes.Select(barcode => new Product_Model { ProductBarcode = barcode, UUID = Guid.NewGuid().ToString() }).ToList();
            foreach (var modelToCheck in modelList)
            {
                var res = CheckBarcode(modelToCheck, new List<Product_Model>(), new List<Product_Model>(), new List<OrderblankItem>(), inventoryListFromDb);
                if (!res.Item1)
                {
                    return new OperationResult(OperationResultType.Error, res.Item2);
                }
            }
            // 生成配货单
            var orderblankDto = new OrderblankDto()
            {
                OrderblankType = OrderblankType.采购单创建,
                PurchaseId = purchaseEntity.Id,
                OutStorageId = purchaseEntity.StorageId.Value,
                OutStoreId = purchaseEntity.Storage.StoreId,
                ReceiverStorageId = purchaseEntity.ReceiverStorageId.Value,
                ReceiverStoreId = purchaseEntity.ReceiverId.Value,
                Status = OrderblankStatus.配货中,
            };

            using (var transaction = this.GetTransaction())
            {
                // 保存配货单
                var res = this.Insert(true, orderblankDto);
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "配货单保存失败" + res.Message);
                }
                var orderblankNumber = res.Data.ToString();
                var orderblankEntity = this.Orderblanks.Where(o => o.OrderBlankNumber == orderblankNumber)
                                                        .Include(o => o.OutStore)
                                                        .Include(o => o.ReceiverStore)
                                                        .FirstOrDefault();
                if (orderblankEntity == null)
                {
                    throw new Exception("保存配货单保存失败失败");
                }
                // 保存配货明细
                var itemList = new List<OrderblankItem>();
                foreach (var item in purchaseItems)
                {
                    //新增
                    var orderblankItem = new OrderblankItem()
                    {
                        OrderblankId = orderblankEntity.Id,
                        OrderblankNumber = orderblankEntity.OrderBlankNumber,
                        ProductId = item.ProductId.Value,
                        OrderBlankBarcodes = string.Join(",", item.PurchaseItemProducts.Select(p => p.ProductBarcode).ToList()),
                        Quantity = item.Quantity,
                        OperatorId = AuthorityHelper.OperatorId
                    };
                    orderblankEntity.OrderblankItems.Add(orderblankItem);
                }
                res = this.Update(new Orderblank[] { orderblankEntity });
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "配货单保存失败" + res.Message);
                }

                // 锁定库存
                inventoryListFromDb.Each(i =>
                {
                    i.Status = InventoryStatus.DeliveryLock;
                    i.IsLock = true;
                });
                res = _inventoryContract.BulkUpdate(inventoryListFromDb);

                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "库存状态更新失败" + res.Message);
                }

                // 记录锁定日志
                if (inventoryListFromDb.Count > 0)
                {
                    res = this.LogWhenOrderblankAdd(orderblankEntity, inventoryListFromDb.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, res.Message);
                    }
                }
                transaction.Commit();
                return new OperationResult(OperationResultType.Success, string.Empty);

            }
        }


        /// <summary>
        /// 预约采购单生成配货单
        /// </summary>
        /// <param name="packingId"></param>
        /// <returns></returns>
        public OperationResult SaveOrderblankFromAppointmentPacking(int packingId)
        {

            var purchaseEntity = _appointmentPackingContract.Entities.Where(p => !p.IsDeleted && p.IsEnabled && p.Id == packingId).Include(e => e.AppointmentPackingItem).FirstOrDefault();
            if (purchaseEntity == null)
            {
                return new OperationResult(OperationResultType.Error, "采购单未找到");
            }
            var appointmentEntity = _appointmentRepo.Entities.FirstOrDefault(a => a.AppointmentPackingId == packingId);
            if (appointmentEntity == null)
            {
                return new OperationResult(OperationResultType.Error, "预约信息未找到");

            }

            //校验是否已生成orderblank
            if (purchaseEntity.OrderblankId.HasValue)
            {
                return new OperationResult(OperationResultType.Error, "采购单已有配货单，无法重复生成");
            }

            if (purchaseEntity.AppointmentPackingItem.Any(i => string.IsNullOrEmpty(i.ProductBarcode)))
            {
                return OperationResult.Error("配货商品有空缺,无法结束配货");
            }

            //校验item
            var purchaseItems = purchaseEntity.AppointmentPackingItem.ToList();


            if (!purchaseItems.Any())
            {
                return new OperationResult(OperationResultType.Error, "预约采购单中没有商品");
            }

            var purchaseBarcodes = purchaseItems.Select(p => p.ProductBarcode).ToList();

            //校验barcodes
            var inventoryListFromDb = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                 .Where(i => !i.IsDeleted && i.IsEnabled)
                                                 .Where(i => i.StorageId == purchaseEntity.FromStorageId)
                                                 .Where(i => purchaseBarcodes.Contains(i.ProductBarcode))
                                                 .ToList();
            if (purchaseBarcodes.Count != inventoryListFromDb.Count)
            {
                return new OperationResult(OperationResultType.Error, "库存数量不一致");
            }
            var modelList = purchaseBarcodes.Select(barcode => new Product_Model { ProductBarcode = barcode, UUID = Guid.NewGuid().ToString() }).ToList();
            foreach (var modelToCheck in modelList)
            {
                var res = CheckBarcode(modelToCheck, new List<Product_Model>(), new List<Product_Model>(), new List<OrderblankItem>(), inventoryListFromDb);
                if (!res.Item1)
                {
                    return new OperationResult(OperationResultType.Error, res.Item2);
                }
            }
            // 生成配货单
            var orderblankDto = new OrderblankDto()
            {
                OrderblankType = OrderblankType.采购单创建,
                OutStorageId = purchaseEntity.FromStorageId,
                OutStoreId = purchaseEntity.FromStoreId,
                ReceiverStorageId = purchaseEntity.ToStorageId.Value,
                ReceiverStoreId = purchaseEntity.ToStoreId,
                Status = OrderblankStatus.发货中,
                AppointmentNumber = appointmentEntity.Number,
                DeliveryTime = DateTime.Now
               
            };

            using (var transaction = this.GetTransaction())
            {
                // 保存配货单
                var res = this.Insert(false, orderblankDto);
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "配货单保存失败" + res.Message);
                }
                var orderblankNumber = res.Data.ToString();
                var orderblankEntity = this.Orderblanks.Where(o => o.OrderBlankNumber == orderblankNumber)
                                                        .Include(o => o.OutStore)
                                                        .Include(o => o.ReceiverStore)
                                                        .FirstOrDefault();
                if (orderblankEntity == null)
                {
                    throw new Exception("保存配货单保存失败失败");
                }
                // 保存配货明细
                var itemList = new List<OrderblankItem>();
                foreach (var item in purchaseItems)
                {
                    //新增
                    var orderblankItem = new OrderblankItem()
                    {
                        OrderblankId = orderblankEntity.Id,
                        OrderblankNumber = orderblankEntity.OrderBlankNumber,
                        ProductId = item.ProductId.Value,
                        OrderBlankBarcodes = item.ProductBarcode,
                        Quantity = 1,
                        OperatorId = AuthorityHelper.OperatorId
                    };
                    orderblankEntity.OrderblankItems.Add(orderblankItem);
                }
                res = this.Update(new Orderblank[] { orderblankEntity });
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "配货单保存失败" + res.Message);
                }

                purchaseEntity.OrderblankId = orderblankEntity.Id;
                purchaseEntity.State = AppointmentPackingState.已装箱;
                purchaseEntity.UpdatedTime = DateTime.Now;
                var cnt = _packingRepo.Update(purchaseEntity);
                if (cnt <= 0)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "装箱状态保存失败" + res.Message);
                }

                appointmentEntity.State = AppointmentState.已装箱;
                appointmentEntity.UpdatedTime = DateTime.Now;
                cnt = _appointmentRepo.Update(appointmentEntity);
                if (cnt <= 0)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "预约状态保存失败" + res.Message);
                }


                // 锁定库存
                inventoryListFromDb.Each(i =>
                {
                    i.Status = InventoryStatus.DeliveryLock;
                    i.IsLock = true;
                });
                res = _inventoryContract.Update(inventoryListFromDb.ToArray());

                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "库存状态更新失败" + res.Message);
                }

                // 记录锁定日志
                if (inventoryListFromDb.Count > 0)
                {
                    res = this.LogWhenOrderblankAdd(orderblankEntity, inventoryListFromDb.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, res.Message);
                    }
                }
                transaction.Commit();
                return new OperationResult(OperationResultType.Success, string.Empty);

            }
        }

        private Tuple<bool, string> CheckBarcode(Product_Model model, List<Product_Model> validList, List<Product_Model> invalidlist, List<OrderblankItem> orderblankItemsFromDb, List<Inventory> inventoryList)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            var number = model.ProductBarcode;
            // 长度校验
            if (number.Length != 14)
            {
                return Tuple.Create(false, "扫入的商品条码不符合要求");
            }

            // 排重
            if (validList.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码已经扫入：" + number);
            }

            if (invalidlist.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码无效，并且已重复出现：" + number);
            }

            #region 校验库存信息
            var inventoryEntity = inventoryList.FirstOrDefault(c => c.ProductBarcode == number);
            if (inventoryEntity == null)
            {
                return Tuple.Create(false, "在库存中未查找到该商品条码：" + number + ",可能是当前用户对该库存不具有操作权限");
            }

            if (inventoryEntity.IsDeleted)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经被移至回收站");
            }
            if (!inventoryEntity.IsEnabled)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存处于禁用状态");
            }
            //if (inventoryEntity.IsLock)
            //{
            //    return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存处于锁定状态");
            //}

            //if (inventoryEntity.Status >= (int)InventoryStatus.PurchasStart &&
            //    inventoryEntity.Status <= (int)InventoryStatus.PurchasEnd)
            //{
            //    return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入采购状态");
            //}
            if (inventoryEntity.Status >= InventoryStatus.DeliveryStart &&
              inventoryEntity.Status <= InventoryStatus.DeliveryEnd)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入配货状态");
            }

            if (inventoryEntity.Status >= InventoryStatus.SaleStart &&
              inventoryEntity.Status <= InventoryStatus.SaleEnd)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入销售状态");
            }


            #endregion

            return Tuple.Create(true, string.Empty);


            //if (inventoryEntity.Status == (int)InventoryStatus.Default)
            //{
            //    if (orderblankItemsFromDb.Any(o => o.OrderBlankBarcodes.Contains(number)))
            //    {
            //        return Tuple.Create(false, "配货单中已存在该商品条码：" + number);
            //    }
            //    // 装载数据
            //    model.Id = inventoryEntity.Id;
            //    model.ProductId = inventoryEntity.ProductId;
            //    model.IsValided = true;
            //    model.ProductNumber = inventoryEntity.ProductNumber;
            //    model.Thumbnail = inventoryEntity.Product.ThumbnailPath;
            //    model.Brand = inventoryEntity.Product.ProductOriginNumber.Brand.BrandName;
            //    model.Size = inventoryEntity.Product.Size.SizeName;
            //    model.Color = inventoryEntity.Product.Color.ColorName;
            //    model.Season = inventoryEntity.Product.ProductOriginNumber.Season.SeasonName;
            //    model.Category = inventoryEntity.Product.ProductOriginNumber.Category.CategoryName;
            //    return Tuple.Create(true, string.Empty);
            //}
            //else
            //{
            //    return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存由于其他原因被锁定");
            //}

        }


        /// <summary>
        /// 根据店铺状态,判断是否可进行配货单的各种操作
        /// </summary>
        /// <param name="orderblankEntity"></param>
        /// <param name="action"></param>
        public OperationResult CheckOrderbalnkAction(Orderblank orderblankEntity, OrderblankAction action)
        {
            var closedStoreIds = _storeContract.GetStoresClosed().ToList();
            var checkingStoreIds = _storeContract.GetStoresInChecking().ToList();

            //发货店铺状态
            var isOutStoreClosed = closedStoreIds.Contains(orderblankEntity.OutStoreId);
            var isOutStoreChecking = checkingStoreIds.Contains(orderblankEntity.OutStoreId);

            //收货店铺状态
            var isReceiverStoreClosed = closedStoreIds.Contains(orderblankEntity.ReceiverStoreId);
            var isReceiverStoreChecking = checkingStoreIds.Contains(orderblankEntity.ReceiverStoreId);
            switch (action)
            {
                case OrderblankAction.Create:
                case OrderblankAction.Delete:
                case OrderblankAction.Delivery:
                    {
                        //发货店铺不能处于盘点,闭店状态
                        if (isOutStoreClosed)
                        {
                            return OperationResult.Error("发货店铺已闭店,无法进行收货操作!");
                        }
                        if (isOutStoreChecking)
                        {
                            return OperationResult.Error("发货店铺正在盘点,无法进行收货操作!");
                        }
                    }
                    break;
                case OrderblankAction.Reject:
                case OrderblankAction.Accept:
                    {
                        //收货店铺不能处于盘点,闭店状态
                        if (isReceiverStoreClosed)
                        {
                            return OperationResult.Error("收货店铺已闭店,无法进行收货操作!");
                        }
                        if (isReceiverStoreChecking)
                        {
                            return OperationResult.Error("收货店铺正在盘点,无法进行收货操作!");
                        }
                    }
                    break;
                default:
                    break;
            }
            return OperationResult.OK("没毛病");
        }
        private Tuple<bool, string> CheckOrderblankEntity(Orderblank entity)
        {
            if (entity == null)
            {
                return Tuple.Create(false, "配货单不存在");
            }
            if (entity.IsDeleted)
            {
                return Tuple.Create(false, "配货单已经被删除");
            }
            if (!entity.IsEnabled)
            {
                return Tuple.Create(false, "配货单被禁用");
            }

            return Tuple.Create(true, string.Empty);
        }

        /// <summary>
        /// 配货单确认收货
        /// </summary>
        /// <param name="orderblankNumber">配货单</param>
        /// <returns></returns>
        public OperationResult ReceptProduct(string orderblankNumber)
        {
            var resul = new OperationResult(OperationResultType.Error);
            string num = orderblankNumber;
            if (string.IsNullOrEmpty(num))
            {
                throw new Exception("参数错误");
            }
            var orderblankEntity = _orderblankRepository.Entities.FirstOrDefault(c => c.OrderBlankNumber == num);

            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                return OperationResult.Error(checkRes.Item2);
            }

            //配货单店铺状态校验
            var chk = CheckOrderbalnkAction(orderblankEntity, OrderblankAction.Accept);
            if (chk.ResultType != OperationResultType.Success)
            {
                return chk;
            }

            var barcodeArr = orderblankEntity.OrderblankItems.SelectMany(c => c.OrderBlankBarcodes.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToList();
            var inventoryList = _inventoryContract.Inventorys.Where(c => barcodeArr.Contains(c.ProductBarcode)).ToList();
            var disablInves = inventoryList.Where(c => c.IsDeleted || !c.IsEnabled).Select(c => c.ProductBarcode).ToList();
            if (disablInves.Any())
            {
                return OperationResult.Error("配货单中的商品被移除、禁用，无法完成配货：" + string.Join(",", disablInves));
            }
            var settingEntity = _timeoutSettingContract.GetTimeoutSettingForOrderblank(OrderblankAction.Accept);
            var timeout = CheckTimeout(orderblankEntity.DeliveryTime.Value, settingEntity);
            using (var transaction = _orderblankRepository.GetTransaction())
            {
                // 配货单更新
                var currentTime = DateTime.Now;
                //超时扣积分
                if (timeout)
                {
                    TimeoutProcess(PunishTypeEnum.配货单确认收货超时, orderblankEntity.OrderBlankNumber, settingEntity.DeductScore);
                }

                orderblankEntity.Status = OrderblankStatus.已完成;
                orderblankEntity.ReceiveTime = currentTime;
                orderblankEntity.UpdatedTime = currentTime;
                orderblankEntity.ReceiverAdminId = AuthorityHelper.OperatorId;
                var res = _orderblankRepository.Update(new Orderblank[] { orderblankEntity });

                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return OperationResult.Error("配货单保存失败" + res.Message);
                }
                if (!string.IsNullOrEmpty(orderblankEntity.AppointmentNumber))
                {
                    var appointmentEntity = _appointmentRepo.Entities.Where(a => a.Number == orderblankEntity.AppointmentNumber).Include(a => a.AppointmentPacking).FirstOrDefault();
                    appointmentEntity.State = AppointmentState.已接收;
                    appointmentEntity.AppointmentPacking.State = AppointmentPackingState.已接收;
                    var cnt = _appointmentRepo.Update(appointmentEntity);
                    if (cnt<=0)
                    {
                        transaction.Rollback();
                        return OperationResult.Error("预约状态更新失败" + res.Message);
                    }
                }

                // 库存解锁,归属店铺,归属仓库转移
                inventoryList.Each(c =>
                {
                    // 库存解锁
                    c.IsLock = false;
                    c.Status = InventoryStatus.Default;
                    // 归属店铺,归属仓库转移
                    c.StorageId = orderblankEntity.ReceiverStorageId;
                    c.StoreId = orderblankEntity.ReceiverStoreId;
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;
                });

                res = _inventoryContract.Update(inventoryList.ToArray());
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return OperationResult.Error("库存状态更新失败" + res.Message);
                }

                // 记录收货日志
                res = LogWhenOrderblankAccept(orderblankEntity, inventoryList.ToArray());
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return OperationResult.Error("库存追踪日志保存失败" + res.Message);

                }
                transaction.Commit();
                return OperationResult.OK();
            }

        }



        [Serializable]
        private class Product_Model
        {
            public Product_Model()
            {
                Id = 0;
                UUID = "";
                Thumbnail = "";
                ProductNumber = "";
                TagPrice = 0;
                WholesalePrice = 0;
                Season = "";
                Size = "";
                Color = "";
                Amount = 1;
                ValidCoun = 0;
                UpdateTime = DateTime.Now;
            }
            public int Id { get; set; }

            public string UUID { get; set; }

            public string Thumbnail { get; set; }

            public string ProductNumber { get; set; }
            public string ProductBarcode { get; set; }
            public string ProductName { get; set; }
            public string Category { get; set; }

            public string Brand { get; set; }
            public float TagPrice { get; set; }//吊牌价

            public float WholesalePrice { get; set; }//进货价

            public string Season { get; set; }

            public string Size { get; set; }

            public string Color { get; set; }

            public int Amount { get; set; } //库存数
            public int Quantity { get; set; }
            public int ValidCoun { get; set; }
            public bool IsValided { get; set; }
            public string Other { get; set; }

            public DateTime UpdateTime { get; set; }
            public string Notes { get; set; }

            public int ProductId { get; set; }

            public float PurchasePrice { get; set; }

            /// <summary>
            /// 采购数量
            /// </summary>
            public int PurchaseQuantity { get; set; }
        }

    }
}
