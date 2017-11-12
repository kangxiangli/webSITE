using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Log;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Warehouse
{
    public class CheckupItemService : ServiceBase, ICheckupItemContract
    {
        #region 初始化操作对象

        protected readonly IRepository<CheckupItem, int> _checkupItemRepositiory;

        protected readonly IRepository<Checker, int> _checkerRepositiory;

        protected readonly IRepository<CheckerItem, int> _checkerItemRepositiory;

        protected readonly IRepository<Inventory, int> _inventoryRepositiory;

        protected readonly IRepository<Product, int> _productRepositiory;

        protected readonly IRepository<Store, int> _storeRepositiory;

        protected readonly IRepository<Storage, int> _storageRepositiory;

        protected readonly IRepository<InventoryRecord, int> _inventoryRecordRepositiory;

        protected readonly IRepository<ProductBarcodeDetail, int> _productBarcodeDetailRepositiory;

        protected readonly IRepository<ProductTrack, int> _ProductTrackRepositiory;

        public CheckupItemService(IRepository<CheckupItem, int> checkupItemRepositiory,
            IRepository<Checker, int> checkerRepositiory,
            IRepository<CheckerItem, int> checkerItemRepositiory,
            IRepository<Inventory, int> inventoryRepositiory,
            IRepository<Product, int> productRepositiory,
            IRepository<Store, int> storeRepositiory,
            IRepository<Storage, int> storageRepositiory,
            IRepository<InventoryRecord, int> inventoryRecordRepositiory,
            IRepository<ProductBarcodeDetail, int> productBarcodeDetailRepositiory,
            IRepository<ProductTrack, int> ProductTrackRepositiory)
            : base(checkupItemRepositiory.UnitOfWork)
        {
            _checkupItemRepositiory = checkupItemRepositiory;
            _checkerRepositiory = checkerRepositiory;
            _checkerItemRepositiory = checkerItemRepositiory;
            _inventoryRepositiory = inventoryRepositiory;
            _productRepositiory = productRepositiory;
            _storeRepositiory = storeRepositiory;
            _storageRepositiory = storageRepositiory;
            _inventoryRecordRepositiory = inventoryRecordRepositiory;
            _productBarcodeDetailRepositiory = productBarcodeDetailRepositiory;
            _ProductTrackRepositiory = ProductTrackRepositiory;
        }
        #endregion

        public IQueryable<CheckupItem> CheckupItems
        {
            get
            {
                return _checkupItemRepositiory.Entities;
            }
        }

        public Utility.Data.OperationResult Insert(params CheckupItem[] items)
        {
            OperationResult resu = new OperationResult(OperationResultType.Error);
            int res = _checkupItemRepositiory.Insert((IEnumerable<CheckupItem>)items);
            if (res == items.Count())
            {
                resu = new OperationResult(OperationResultType.Success, "成功插入" + res + "条数据") { Data = items.Select(c => c.Id).ToArray() };
            }
            return resu;
        }

        #region 更改全部缺货和余货
        /// <summary>
        /// 更改全部缺货和余货
        /// </summary>
        /// <param name="Id">盘点Id</param>
        /// <param name="Flag">缺货或者余货标识</param>
        /// <returns></returns>
        public OperationResult ChangeAllChecker(int Id, int Flag)
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                UnitOfWork.TransactionEnabled = true;
                Checker checker = _checkerRepositiory.GetByKey(Id);
                if (checker == null)
                {
                    oper.Message = "校对数据不存在";
                }
                else
                {
                    if (Flag == (int)CheckerItemFlag.Surplus || Flag == (int)CheckerItemFlag.Lack)
                    {
                        List<CheckerItem> listCheckupItem = checker.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerItemType == Flag).ToList();
                        //listCheckupItem.
                    }

                    //
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }
        #endregion

        #region 移除缺货
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public OperationResult RemoveLack(params int[] Ids)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                UnitOfWork.TransactionEnabled = true;
                IQueryable<ProductBarcodeDetail> listProBar = _productBarcodeDetailRepositiory.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status != (int)ProductBarcodeDetailFlag.Abolish);
                IQueryable<CheckerItem> listCheckerItem = _checkerItemRepositiory.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                //出去一个Id找出对应的盘点数据,获取盘点的店铺和仓库
                int checkerItemId = Ids[0];
                Checker checker = listCheckerItem.FirstOrDefault(x => x.Id == checkerItemId).Cherker;
               var storageName= _storageRepositiory.Entities.FirstOrDefault(x => x.Id == checker.StorageId).StorageName;
                DateTime currentDate = DateTime.Now;
                checker.UpdatedTime = currentDate;
                //IQueryable<Inventory> listInventory = _inventoryRepositiory.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                //.Where(x => x.StorageId == checker.StorageId && x.StoreId == checker.StoreId && x.Status == (int)InventoryStatus.Default);
                IQueryable<Inventory> listInventory = FilterProduct(checker);
                foreach (int id in Ids)
                {
                    CheckerItem checkerItem = listCheckerItem.FirstOrDefault(x => x.Id == id);
                    Inventory inventory = listInventory.FirstOrDefault(x => x.ProductId == checkerItem.ProductId);
                    string ProductBarcode = string.Empty;
                    if (checkerItem != null)
                    {
                        //if (checker.MissingCount > 0)
                        //{
                        //    checker.MissingCount -= 1;
                        //}
                        CheckupItem entity = new CheckupItem();
                        entity.CheckerItemId = checkerItem.Id;
                        entity.CheckGuid = checkerItem.CheckGuid;
                        entity.CheckupType = (int)OperationFlag.Del;
                        checkerItem.IsCheckup = true;
                        checkerItem.UpdatedTime = currentDate;
                        string strProductBarcode = checkerItem.ProductBarcode;
                        int length = strProductBarcode.Length;
                        string strProductNumber = strProductBarcode.Substring(0, length - 3);
                        string strOnlyFlag = strProductBarcode.Substring(length - 3);
                        ProductBarcodeDetail proBar = listProBar.FirstOrDefault(x => x.ProductNumber == strProductNumber && x.OnlyFlag == strOnlyFlag && x.ProductId == checkerItem.ProductId);
                        if (proBar != null)
                        {
                            proBar.Status = (int)ProductBarcodeDetailFlag.Unused;
                            proBar.UpdatedTime = DateTime.Now;
                            ProductBarcode = proBar.ProductNumber + proBar.OnlyFlag;
                            _productBarcodeDetailRepositiory.Update(proBar);
                        }
                        _checkerItemRepositiory.Update(checkerItem);
                        _checkupItemRepositiory.Insert(entity);
                    }
                    if (inventory != null)
                    {
                        inventory.IsDeleted = true;
                        inventory.UpdatedTime = currentDate;
                        _inventoryRepositiory.Update(inventory);
                        if (!string.IsNullOrEmpty(ProductBarcode))
                        {
                            ProductTrack pt = new ProductTrack();
                            #region 商品追踪
                            pt.ProductNumber = ProductBarcode.Substring(0, ProductBarcode.Length - 3);
                            pt.ProductBarcode = ProductBarcode;
                            pt.OperatorId = AuthorityHelper.OperatorId;
                            pt.Describe =string.Format(ProductOptDescTemplate.ON_PRODUCT_CHECKER_INVENTORY_DELETE, storageName);
                            _ProductTrackRepositiory.Insert(pt);
                            #endregion
                        }

                    }
                }
                _checkerRepositiory.Update(checker);
                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    oper.ResultType = OperationResultType.Success;

                }
                else
                {
                    oper.Message = "移除失败";
                }
            }
            catch (Exception)
            {
                oper.Message = "服务忙，请稍后访问";
            }
            return oper;
        }
        #endregion

        #region 添加余货到仓库
        public OperationResult AddInventory(params int[] Ids)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                UnitOfWork.TransactionEnabled = true;
                IQueryable<CheckerItem> listCheckerItem = _checkerItemRepositiory.Entities.Where(x => x.IsCheckup == false && x.IsEnabled == true);
                //出去一个Id找出对应的盘点数据,获取盘点的店铺和仓库
                int checkerItemId = Ids[0];
                int checkerId = listCheckerItem.FirstOrDefault(x => x.Id == checkerItemId).CheckerId ?? 0;
                Checker checker = _checkerRepositiory.GetByKey(checkerId);
                var storageName = _storageRepositiory.Entities.FirstOrDefault(x => x.Id == checker.StorageId).StorageName;
                DateTime currentDate = DateTime.Now;
                IQueryable<Inventory> listInventory = FilterProduct(checker);
                IQueryable<ProductBarcodeDetail> listProBar = _productBarcodeDetailRepositiory.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)ProductBarcodeDetailFlag.Unused);
                List<ProductBarcodeDetail> listProBarUpdate = new List<ProductBarcodeDetail>();
                InventoryRecord inventoryRecord = new InventoryRecord()
                {
                    StoreId = checker.StoreId,
                    StorageId = checker.StorageId,
                };
                int quantity = 0;
                foreach (int id in Ids)
                {
                    CheckerItem checkerItem = listCheckerItem.FirstOrDefault(x => x.Id == id);
                    if (checkerItem != null)
                    {
                        //if (checker.ResidueCount > 0)
                        //{
                        //    checker.ResidueCount -= 1;
                        //}
                        CheckupItem entity = new CheckupItem();
                        entity.CheckerItemId = checkerItem.Id;
                        //entity.CheckerItem = checkerItem;
                        entity.CheckGuid = checkerItem.CheckGuid;
                        entity.CheckupType = (int)OperationFlag.Add;
                        checkerItem.IsCheckup = true;
                        checkerItem.UpdatedTime = currentDate;
                        _checkerItemRepositiory.Update(checkerItem);
                        _checkupItemRepositiory.Insert(entity);
                        //查找仓库
                        Inventory inventory = listInventory.FirstOrDefault(x => x.ProductId == checkerItem.ProductId);
                        if (inventory == null)
                        {

                            inventory = new Inventory()
                            {
                                StoreId = checker.StoreId,
                                //Store=checker.Store,
                                StorageId = checker.StorageId,
                                //Storage=checker.Storage,
                                ProductId = checkerItem.ProductId ?? 0,
                                //Product=checkerItem.Product,
                                ProductNumber = checkerItem.Product.ProductNumber,
                                OnlyFlag = checkerItem.Product.ProductOriginNumber.AssistantNum,
                                ProductBarcode = checkerItem.ProductBarcode,
                                IsLock = false,
                                //TagPrice = checkerItem.Product.ProductOriginNumber.TagPrice,
                                //WholesalePrice = checkerItem.Product.ProductOriginNumber.WholesalePrice,
                                //PurchasePrice = checkerItem.Product.ProductOriginNumber.PurchasePrice,
                                Status = (int)InventoryStatus.Default,
                                ProductLogFlag = Guid.NewGuid().ToString().Replace("-", ""),
                                OperatorId = AuthorityHelper.OperatorId,
                            };
                            quantity++;
                            string strProductBarcode = checkerItem.ProductBarcode;
                            int length = strProductBarcode.Length;
                            //取出后三位标识
                            string strOnlyFlag = strProductBarcode.Substring(length - 3);
                            string strProNumber = strProductBarcode.Substring(0, length - 3);
                            ProductBarcodeDetail proBar = listProBar.FirstOrDefault(x => x.ProductId == checkerItem.ProductId && x.ProductNumber == strProNumber && x.OnlyFlag == strOnlyFlag);
                            if (proBar != null)
                            {
                                proBar.UpdatedTime = DateTime.Now;
                                proBar.Status = (int)ProductBarcodeDetailFlag.AddStorage;
                                proBar.OperatorId = AuthorityHelper.OperatorId;

                                listProBarUpdate.Add(proBar);
                            }
                            else
                            {
                                oper.Message = "编码不存在或者已经被使用";
                                return oper;
                            }
                            inventoryRecord.RecordOrderNumber = checker.CheckGuid;
                            inventoryRecord.TagPrice = inventory.Product.ProductOriginNumber.TagPrice + inventoryRecord.TagPrice;
                            inventoryRecord.OperatorId = AuthorityHelper.OperatorId;
                            inventoryRecord.Inventories.Add(inventory);
                            //_inventoryRepositiory.Insert(inventory);
                        }
                    }
                }
                if (inventoryRecord != null && inventoryRecord.Inventories.Count() > 0)
                {
                    inventoryRecord.Quantity = quantity;
                    _inventoryRecordRepositiory.Insert(inventoryRecord);
                    foreach (var item in inventoryRecord.Inventories)
                    {
                        var ProductBarcode = item.ProductBarcode;
                        if (!string.IsNullOrEmpty(ProductBarcode))
                        {
                            ProductTrack pt = new ProductTrack();
                            #region 商品追踪
                            pt.ProductNumber = ProductBarcode.Substring(0, ProductBarcode.Length - 3);
                            pt.ProductBarcode = ProductBarcode;
                            pt.OperatorId = AuthorityHelper.OperatorId;
                            pt.Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_CHECKER_INVENTORY_ADD, storageName);
                            _ProductTrackRepositiory.Insert(pt);
                            #endregion
                        }
                    }
                }
                else
                {
                    oper.Message = "入库失败";
                    return oper;
                }
                if (listProBarUpdate != null && listProBarUpdate.Count > 0)
                {
                    _productBarcodeDetailRepositiory.Update(listProBarUpdate);
                }
                else
                {
                    oper.Message = "入库失败";
                    return oper;
                }
                _checkerRepositiory.Update(checker);
                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    oper.ResultType = OperationResultType.Success;
                }
                else
                {
                    oper.Message = "移除失败";
                }
            }
            catch (Exception)
            {

                oper.Message = "服务忙，请稍后访问";
            }
            return oper;
        }
        #endregion

        #region 委托-根据盘点条件对商品进行筛选
        private IQueryable<Inventory> FilterProduct(Checker dto)
        {

            IQueryable<Inventory> invertoryList = _inventoryRepositiory.Entities.Where(x => x.StorageId == dto.StorageId && x.StoreId == dto.StoreId && x.Status == (int)InventoryStatus.Default);
            Func<Inventory, bool> predicate = (invent) => invent.IsEnabled == true && invent.IsDeleted == false;
            if (dto.BrandId != null && dto.BrandId > 0)
            {
                predicate += (
                  (invent) => invent.Product.ProductOriginNumber.BrandId == dto.BrandId
               );
            }
            if (dto.CategoryId != null)
            {
                predicate += (
                  (invent) => invent.Product.ProductOriginNumber.CategoryId == dto.CategoryId
               );
            }
            //if (dto.ColorId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.ColorId == dto.ColorId
            //   );
            //}
            //if (dto.SeasonId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.ProductOriginNumber.SeasonId == dto.SeasonId
            //   );
            //}
            //if (dto.SizeId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.SizeId == dto.SizeId
            //   );
            //}
            return invertoryList.Where(predicate).AsQueryable();
        }
        #endregion

    }
}
