
using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using XKMath36;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class SaleAutoGenService : ServiceBase, ISaleAutoGenContract
    {
        protected static object objlock = new object();

        private readonly IRepository<SaleAutoGen, int> _SaleAutoGenRepository;
        private readonly IRepository<ProductOriginNumber, int> _ProductOriginNumRepository;
        private readonly IRepository<Storage, int> _StorageRepository;
        private readonly IRepository<Administrator, int> _AdministratorRepository;
        private readonly IRepository<Member, int> _MemberRepository;
        private readonly IRepository<Product, int> _ProductRepository;
        private readonly IRepository<ProductTrack, int> _ProductTrackRepository;
        private readonly IRepository<ProductBarcodeDetail, int> _ProductBarcodeDetailRepository;
        private readonly IRepository<Inventory, int> _InventoryRepository;
        private readonly IRepository<Orderblank, int> _OrderblankRepository;
        private readonly IRepository<Retail, int> _RetailRepository;
        public SaleAutoGenService(
            IRepository<SaleAutoGen, int> _SaleAutoGenRepository,
            IRepository<ProductOriginNumber, int> _ProductOriginNumRepository,
            IRepository<Storage, int> _StorageRepository,
            IRepository<Administrator, int> _AdministratorRepository,
            IRepository<Product, int> _ProductRepository,
            IRepository<ProductTrack, int> _ProductTrackRepository,
            IRepository<ProductBarcodeDetail, int> _ProductBarcodeDetailRepository,
            IRepository<Inventory, int> _InventoryRepository,
            IRepository<Orderblank, int> _OrderblankRepository,
            IRepository<Retail, int> _RetailRepository,
            IRepository<Member, int> _MemberRepository
            ) : base(_SaleAutoGenRepository.UnitOfWork)
        {
            this._SaleAutoGenRepository = _SaleAutoGenRepository;
            this._ProductOriginNumRepository = _ProductOriginNumRepository;
            this._StorageRepository = _StorageRepository;
            this._AdministratorRepository = _AdministratorRepository;
            this._MemberRepository = _MemberRepository;
            this._ProductRepository = _ProductRepository;
            this._ProductTrackRepository = _ProductTrackRepository;
            this._ProductBarcodeDetailRepository = _ProductBarcodeDetailRepository;
            this._InventoryRepository = _InventoryRepository;
            this._OrderblankRepository = _OrderblankRepository;
            this._RetailRepository = _RetailRepository;
        }

        public IQueryable<SaleAutoGen> Entities
        {
            get
            {
                return _SaleAutoGenRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _SaleAutoGenRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params SaleAutoGen[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _SaleAutoGenRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
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
                var entities = _SaleAutoGenRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params SaleAutoGen[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SaleAutoGenRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public SaleAutoGen View(int Id)
        {
            return _SaleAutoGenRepository.GetByKey(Id);
        }

        public OperationResult Insert(params SaleAutoGenDto[] dtos)
        {
            return Insert(null, dtos);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="process">bool=trueʱ�ر�poploading,stringΪ����,int[]Ϊ֪ͨ��</param>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(Action<bool, string, int[]> process, params SaleAutoGenDto[] dtos)
        {
            var sendpeople = new int[] { AuthorityHelper.OperatorId ?? 0 };//����֪ͨ��
            return OperationHelper.Try(() =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                var list = new List<SaleAutoGen>();

                #region dtoתEntity
                process?.Invoke(false, "����ת��...", sendpeople);
                foreach (var dto in dtos)
                {
                    var ent = Mapper.Map<SaleAutoGen>(dto);
                    ent.Products = _ProductRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => dto.ProductIds.Contains(w.Id)).ToList();
                    ent.ReceiveStorages = _StorageRepository.Entities.Where(w => dto.ReceiveStorageIds.Contains(w.Id)).ToList();
                    ent.SendStorage = _StorageRepository.Entities.FirstOrDefault(w => w.Id == dto.SendStorageId);
                    ent.SendStore = ent.SendStorage.Store;
                    ent.SellerMembers = dto.SellerMemberIds.Select(s => new SellerMember()
                    {
                        StoreId = s.StoreId,
                        Sellers = _AdministratorRepository.Entities.Where(w => s.SellerIds.Contains(w.Id)).ToList(),
                        Members = _MemberRepository.Entities.Where(w => s.MemberIds.Contains(w.Id)).ToList()
                    }).ToList();

                    ent.CreatedTime = DateTime.Now;
                    ent.OperatorId = AuthorityHelper.OperatorId;
                    list.Add(ent);
                }

                OperationResult result = _SaleAutoGenRepository.Insert(list, a => { });

                #endregion

                #region �Զ���������
                Random rand = new Random();
                foreach (var item in list)
                {
                    #region �����9-18�㷶Χ֮��
                    process?.Invoke(false, "ʱ��У��...", sendpeople);

                    var TimeS = item.StartTime > item.EndTime ? item.EndTime : item.StartTime;//������ʼʱ��
                    var TimeE = item.StartTime < item.EndTime ? item.EndTime : item.StartTime;//��������ʱ��

                    TimeS = TimeS.Hour >= 9 && TimeS.Hour <= 18 ? TimeS : TimeS.Date.AddHours(9);
                    TimeE = TimeE.Hour >= 9 && TimeE.Hour <= 18 ? TimeE : TimeE.Date.AddHours(18);

                    #endregion

                    #region ��������ʱ��

                    var TimeSR = item.RetailStartTime > item.RetailEndTime ? item.RetailEndTime : item.RetailStartTime;
                    var TimeSE = item.RetailStartTime < item.RetailEndTime ? item.RetailEndTime : item.RetailStartTime;

                    var dtnow = DateTime.Now;

                    TimeSR = TimeSR > dtnow ? dtnow.AddDays(-1) : TimeSR;
                    TimeSE = TimeSE > dtnow ? dtnow : TimeSE;

                    #endregion

                    var discount = (item.Discount ?? 10) * (decimal)0.1;//�ۿ�

                    var OpTime = rand.NextDateTime(TimeS, TimeE).Date.AddHours(9).AddSeconds(rand.Next(0, 36000));//������׼ʱ��

                    var allcount = item.AllSaleCount;//Ҫ�������������
                    //�����ɵĴ�ӡ����������
                    List<string> listAllBarcode = new List<string>();
                    //���Ѵ�ӡ������׼�����
                    List<Inventory> listAllInventroy = new List<Inventory>();
                    //�Ѵ�ӡ��������ϸ��Ϣ
                    List<ProductBarcodeDetail> listAllBarDetail = new List<ProductBarcodeDetail>();

                    #region ��������
                    process?.Invoke(false, "��������...", sendpeople);

                    //����ӡ�������Ʒ
                    var listpro = item.Products.DistinctBy(d => d.ProductNumber)
                    .OrderBy(o => o.ProductNumber).Skip(0).Take(allcount).ToList();//Take��ֹ������Ʒ��������Ҫ���ɵ���������

                    var procount = listpro.Count;//��Ʒ���������ַ�����

                    var floorCount = (int)Math.Floor((double)allcount / procount);//��Сÿ��ƽ����

                    var readminCount = allcount - floorCount * (procount - 1);//���ܳ�����,�����ŵ����һ��
                    var listPartCount = new List<int>();//����Ʒ��Ҫ��ӡ�ķ���
                    for (int i = 0; i < procount - 1; i++)
                    {
                        listPartCount.Add(floorCount);
                    }
                    listPartCount.Add(readminCount);
                    listPartCount = ListRandom(listPartCount);

                    Math36 math36 = new Math36();
                    for (int i = 0; i < listpro.Count; i++)
                    {
                        var pro = listpro[i];//��Ʒ
                        var curPartCount = listPartCount[i];//��ǰ��ƷҪ��ӡ��������
                        pro.BarcodePrintCount += curPartCount;
                        var orgPrintFlag = pro.BarcodePrintInfo?.CurPrintFlag ?? "0";
                        var orgPrintFlagInt = (int)math36.To10(orgPrintFlag, 0);
                        if (pro.BarcodePrintInfo == null)
                        {
                            ProductBarcodePrintInfo barcodePrintInfo = new ProductBarcodePrintInfo()
                            {
                                CurPrintFlag = math36.To36(curPartCount).PadLeft(3, '0'),
                                ProductNumber = pro.ProductNumber,
                                LastUpdateTime = OpTime
                            };
                            pro.BarcodePrintInfo = barcodePrintInfo;
                        }
                        else
                        {
                            pro.BarcodePrintInfo.CurPrintFlag = math36.To36(orgPrintFlagInt + curPartCount).PadLeft(3, '0');
                            pro.BarcodePrintInfo.LastUpdateTime = OpTime;
                        }
                        for (int j = 1; j <= curPartCount; j++)
                        {
                            var curFlagInt = orgPrintFlagInt + j;
                            var curFlag = math36.To36(curFlagInt).PadLeft(3, '0');

                            var curbarcode = pro.ProductNumber + curFlag;
                            var logflag = Guid.NewGuid().ToString("N");

                            #region �����ӡ����

                            var pbd = new ProductBarcodeDetail()
                            {
                                ProductNumber = pro.ProductNumber,
                                OnlyFlag = curFlag,
                                OnlfyFlagOfInt = curFlagInt,
                                LogFlag = logflag,
                                Status = 0,
                                ProductId = pro.Id,
                                OperatorId = AuthorityHelper.OperatorId,
                                CreatedTime = OpTime,
                            };

                            listAllBarDetail.Add(pbd);

                            #endregion

                            #region ��Ʒ������־

                            pro.ProductOperationLogs.Add(new ProductOperationLog()
                            {
                                ProductNumber = pro.ProductNumber,
                                ProductBarcode = curbarcode,
                                LogFlag = logflag,
                                OnlyFlag = curFlag,
                                Description = "��ӡ����",
                                OperatorId = AuthorityHelper.OperatorId,
                                CreatedTime = OpTime,
                                ProdutBarcodeDetails = new ProductBarcodeDetail[] { pbd }
                            });

                            #endregion

                            #region ��Ʒ׷��

                            ProductTrack pt = new ProductTrack();
                            pt.ProductNumber = pro.ProductNumber;
                            pt.ProductBarcode = pro.ProductNumber + curFlag;
                            pt.Describe = ProductOptDescTemplate.ON_PRODUCT_PRINT;
                            pt.CreatedTime = OpTime;
                            pt.OperatorId = AuthorityHelper.OperatorId;
                            _ProductTrackRepository.Insert(pt);
                            listAllBarcode.Add(pt.ProductBarcode);

                            #endregion

                            #region ׼���������
                            OpTime = OpTime.AddSeconds(rand.Next(0, 1800));
                            var inv = new Inventory();
                            inv.ProductNumber = pro.ProductNumber;
                            inv.OnlyFlag = curFlag;
                            inv.ProductLogFlag = logflag;
                            inv.ProductBarcode = pt.ProductBarcode;
                            inv.StorageId = item.SendStorageId;
                            inv.Storage = item.SendStorage;
                            inv.StoreId = item.SendStoreId;
                            inv.Store = item.SendStore;
                            inv.ProductId = pro.Id;
                            inv.Product = pro;
                            inv.Description = string.Empty;
                            inv.OperatorId = AuthorityHelper.OperatorId;
                            inv.CreatedTime = OpTime;
                            listAllInventroy.Add(inv);

                            #endregion
                        }
                    }

                    _ProductRepository.Update(listpro);

                    #endregion

                    #region ���
                    process?.Invoke(false, "��Ʒ���...", sendpeople);

                    var totalTagPrice = listAllInventroy.Sum(s => s.Product.ProductOriginNumber.TagPrice);

                    var record = new InventoryRecord()
                    {
                        Quantity = listAllInventroy.Count,
                        OperatorId = AuthorityHelper.OperatorId,
                        StorageId = item.SendStorageId,
                        StoreId = item.SendStoreId,
                        TagPrice = totalTagPrice,
                        RecordOrderNumber = string.Empty,
                        CreatedTime = OpTime
                    };

                    listAllInventroy.Each(e =>
                    {
                        e.InventoryRecord = record;

                        e.ProductOperationLogs = new ProductOperationLog[] {
                            new ProductOperationLog()
                            {
                                ProductNumber = e.ProductNumber,
                                OnlyFlag = e.OnlyFlag,
                                ProductBarcode = e.ProductNumber + e.OnlyFlag,
                                LogFlag = e.ProductLogFlag,
                                Description = "��Ʒ���",
                                OperatorId = AuthorityHelper.OperatorId,
                                CreatedTime=OpTime
                            }
                        };

                        ProductTrack pt = new ProductTrack();
                        pt.ProductNumber = e.ProductNumber;
                        pt.ProductBarcode = e.ProductNumber + e.OnlyFlag;
                        pt.OperatorId = AuthorityHelper.OperatorId;
                        pt.Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_INVENTORY, e.Storage.StorageName);
                        _ProductTrackRepository.Insert(pt);
                    });
                    _InventoryRepository.Insert(listAllInventroy, null);

                    listAllBarDetail.Each(e => e.Status = 1);

                    _ProductBarcodeDetailRepository.Update(listAllBarDetail);

                    #endregion

                    #region ���
                    process?.Invoke(false, "��ʼ���...", sendpeople);

                    //���ɵ����������
                    List<Orderblank> listAllOrderblank = new List<Orderblank>();

                    #region ���������

                    //ÿ���ֿ�Ҫ�����������
                    List<int> listPartRSCount = new List<int>();

                    var rsPartCount = item.ReceiveStorages.Count;//����ֿ⡾���ַ�����
                    var floorRSCount = (int)Math.Floor((double)listAllBarcode.Count / rsPartCount);//��Сÿ��ƽ����
                    var readminRSCount = listAllBarcode.Count - floorRSCount * (rsPartCount - 1);//���ܳ�����,�����ŵ����һ��
                    for (int i = 0; i < rsPartCount - 1; i++)
                    {
                        listPartRSCount.Add(floorRSCount);
                    }
                    listPartRSCount.Add(readminRSCount);
                    listPartRSCount = ListRandom(listPartRSCount);

                    Random rd = new Random();
                    var indRs = 0;
                    var sucCount = 0;
                    foreach (var rs in item.ReceiveStorages)
                    {
                        var curSellers = item.SellerMembers.FirstOrDefault(f => f.StoreId == rs.StoreId).Sellers.ToArray();
                        var admin = rd.NextItem(curSellers);//���һ�������������
                        var itemcount = listPartRSCount[indRs++];//���������

                        var curInvs = listAllInventroy.Skip(sucCount).Take(itemcount);//��ǰҪ�����������Ϣ
                        sucCount += itemcount;

                        var blanknum = string.Empty;
                        do
                        {
                            blanknum = RandomHelper.GetRandomCode(10);
                        } while (listAllOrderblank.Any(a => a.OrderBlankNumber == blanknum));

                        OpTime = OpTime.AddSeconds(rand.Next(0, 1800));

                        var ob = new Orderblank()
                        {
                            CreatedTime = OpTime,
                            OrderBlankNumber = blanknum,
                            OrderblankType = OrderblankType.ֱ�Ӵ���,
                            Status = OrderblankStatus.�����,
                            OutStoreId = item.SendStoreId,
                            OutStorageId = item.SendStorageId,
                            ReceiverStoreId = rs.StoreId,
                            ReceiverStorageId = rs.Id,
                            DeliverAdminId = AuthorityHelper.OperatorId,
                            ReceiverAdminId = admin.Id,
                            DeliveryTime = OpTime.AddSeconds(rand.Next(0, 300)),
                            ReceiveTime = OpTime.AddSeconds(rand.Next(500, 1200)),
                            OperatorId = AuthorityHelper.OperatorId,
                            OrderblankItems = curInvs.GroupBy(g => g.ProductId).Select(s => new OrderblankItem()
                            {
                                ProductId = s.Key,
                                OrderblankNumber = blanknum,
                                Quantity = s.Count(),
                                OrderBlankBarcodes = string.Join(",", s.Select(ss => ss.ProductBarcode)),
                                OperatorId = AuthorityHelper.OperatorId,
                            }).ToList()
                        };
                        listAllOrderblank.Add(ob);
                        curInvs.Each(e =>
                        {
                            e.Status = (int)InventoryStatus.Default; e.IsLock = false;
                            e.UpdatedTime = ob.ReceiveTime.Value;
                            e.StorageId = ob.ReceiverStorageId;
                            e.StoreId = ob.ReceiverStoreId;
                            e.OperatorId = admin.Id;
                        });

                        #region ��¼����׷����־

                        #region ������־

                        var logs = curInvs.Select(i => new ProductTrack
                        {
                            ProductNumber = i.ProductNumber,
                            ProductBarcode = i.ProductBarcode,
                            Describe = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_DELIVERY, item.SendStore.StoreName, rs.Store.StoreName),
                            CreatedTime = ob.DeliveryTime.Value,
                            OperatorId = AuthorityHelper.OperatorId
                        }).ToArray();
                        _ProductTrackRepository.Insert(logs, null);

                        #endregion

                        #region �ջ���־

                        var logs2 = curInvs.Select(i => new ProductTrack
                        {
                            ProductNumber = i.ProductNumber,
                            ProductBarcode = i.ProductBarcode,
                            Describe = string.Format(ProductOptDescTemplate.ON_ORDERBLANK_ACCEPT, rs.Store.StoreName),
                            CreatedTime = ob.ReceiveTime.Value,
                            OperatorId = AuthorityHelper.OperatorId
                        }).ToArray();
                        _ProductTrackRepository.Insert(logs2, null);

                        #endregion

                        #endregion
                    }

                    _InventoryRepository.Update(listAllInventroy);
                    _OrderblankRepository.Insert(listAllOrderblank, null);

                    #endregion

                    #endregion

                    #region ����
                    //���ظ������۵���,�������Ż�
                    var listRetailNumber = new List<string>();

                    #region ���ɵ���
                    process?.Invoke(false, "�������۶���...", sendpeople);

                    do
                    {
                        listRetailNumber.Clear();
                        listAllInventroy.Each(e =>
                        {
                            var strrand = string.Empty;
                            do
                            {
                                strrand = rand.GetRandomLetterAndNumberString(10);

                            } while (listRetailNumber.Contains(strrand));
                            listRetailNumber.Add(strrand);
                        });
                    } while (_RetailRepository.Entities.Where(w => listRetailNumber.Contains(w.RetailNumber)).Any());

                    #endregion

                    var indRN = 0;
                    OpTime = OpTime.AddSeconds(rand.Next(1200, 1500));//���ۻ�׼ʱ��
                    OpTime = OpTime > TimeSR ? OpTime : TimeSR;
                    foreach (var sm in item.SellerMembers)
                    {
                        var listinv = listAllInventroy.OrderBy(o => rand.Next()).Where(w => w.StoreId == sm.StoreId).ToList();//�뵽�õ��̵Ŀ�沢�������˳��

                        if (listinv.IsNotNullOrEmptyThis()&& sm.Members.IsNotNullOrEmptyThis()&& sm.Sellers.IsNotNullOrEmptyThis())
                        {
                            var listsl = sm.Sellers.ToArray();//����Ա
                            var listm = sm.Members.ToArray();//��Ա

                            var Copies = listinv.Count < sm.Members.Count ? listinv.Count : sm.Members.Count;//����,��ֹ�����˲�����

                            var listmppro = new List<int>();//�����Ա����

                            #region �����������һ���м�����Ʒ��

                            var mpcount = (int)Math.Floor((double)listinv.Count / Copies);//ÿ����Ա����ƽ������
                            var lastmpcount = listinv.Count - (Copies - 1) * mpcount;//���һ����Ա����,��Ϊ���ܳ�����
                            for (int m = 1; m < Copies; m++)
                            {
                                listmppro.Add(mpcount);
                            }
                            listmppro.Add(lastmpcount);
                            listmppro = ListRandom(listmppro);

                            #endregion

                            var curitemcount = 0;

                            foreach (var itemc in listmppro)
                            {
                                var listcurinv = listinv.Skip(curitemcount).Take(itemc);
                                curitemcount += itemc;

                                var randSeller = rand.NextItem(listsl);//�������Ա
                                var randMember = rand.NextItem(listm);//�����Ա
                                var tagorgprice = (decimal)listcurinv.Sum(s => s.Product.ProductOriginNumber.TagPrice);//ԭ�����ܼ�
                                var tagprice = tagorgprice * discount;//�����ܼ�*�ۿ�

                                var RetTime = rand.NextDateTime(OpTime, TimeSE);//�������ʱ��
                                if (RetTime.Hour < 9 || RetTime.Hour >= 22)
                                {
                                    RetTime = rand.NextDateTime(RetTime.Date.AddHours(9), RetTime.Date.AddHours(22));//ת����9-22��
                                }

                                decimal StoredValueConsume = 0;//��ֵ����
                                decimal ScoreConsume = 0;//��������
                                decimal CashConsume = 0;//�ֽ�����

                                #region ����۳����ֺʹ�ֵ

                                if (randMember.Balance - tagprice < 0)
                                {
                                    StoredValueConsume = randMember.Balance;
                                    randMember.Balance = 0;
                                    var rbal = tagprice - StoredValueConsume;//����
                                    if (randMember.Score - rbal < 0)
                                    {
                                        ScoreConsume = randMember.Score;
                                        randMember.Score = 0;
                                        CashConsume = rbal - ScoreConsume;
                                    }
                                    else
                                    {
                                        randMember.Score -= rbal;
                                        ScoreConsume = rbal;
                                    }
                                }
                                else
                                {
                                    randMember.Balance -= tagprice;
                                    StoredValueConsume = tagprice;
                                }

                                _MemberRepository.Update(randMember);

                                #endregion

                                var curRN = listRetailNumber[indRN++];//��ǰ�����۵���

                                #region �������۵�

                                var modRet = new Retail() {
                                    RetailNumber = curRN,
                                    ConsumeCount = tagprice,
                                    StoredValueConsume = StoredValueConsume,
                                    RemainValue = randMember.Balance,
                                    ScoreConsume = ScoreConsume,
                                    RemainScore = randMember.Score,
                                    CashConsume = CashConsume,
                                    Quotiety = item.Quotiety,
                                    RealStoredValueConsume = StoredValueConsume * (item.Quotiety ?? 1),
                                    //LevelDiscountAmount = tagorgprice - tagprice,
                                    //LevelDiscount = discount,
                                    ConsumerId = randMember.Id,
                                    OutStorageDatetime = RetTime,//����ʱ��
                                    CreatedTime = RetTime,
                                    UpdatedTime = RetTime,
                                    StoreId = listcurinv.First().StoreId,
                                    OperatorId = randSeller.Id,//����Ա
                                };

                                #region ����������

                                foreach (var curinv in listcurinv)
                                {
                                    var curtagprice = (decimal)curinv.Product.ProductOriginNumber.TagPrice;
                                    var ritem = new RetailItem()
                                    {
                                        ProductId = curinv.ProductId,
                                        OutStorageIds = curinv.StorageId + "",
                                        ProductTagPrice = curtagprice,
                                        ProductRetailPrice = curtagprice * discount,
                                        ProductRetailItemMoney = curtagprice * discount,
                                        RetailCount = 1,
                                        OperatorId = randSeller.Id,
                                        BrandDiscount = discount,
                                        CreatedTime = RetTime,
                                        UpdatedTime = RetTime,
                                        RetailInventorys = new RetailInventory[] {
                                                new RetailInventory() {
                                                    Inventory = curinv,
                                                    RetailNumber = curRN,
                                                    OperatorId = randSeller.Id,
                                                    CreatedTime = RetTime,
                                                    UpdatedTime = RetTime,
                                                    ProductBarcode = curinv.ProductBarcode,
                                                }
                                            }
                                    };

                                    curinv.Status = InventoryStatus.JoinOrder;

                                    var log = new ProductTrack
                                    {
                                        ProductNumber = curinv.ProductNumber,
                                        ProductBarcode = curinv.ProductBarcode,
                                        Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_RETAIL, curinv.Store.StoreName),
                                        CreatedTime = RetTime,
                                        OperatorId = randSeller.Id
                                    };

                                    _ProductTrackRepository.Insert(log);
                                    modRet.RetailItems.Add(ritem);

                                }

                                #endregion

                                _InventoryRepository.Update(listcurinv.ToArray());
                                _RetailRepository.Insert(modRet);

                                #endregion
                            }
                        }
                    }

                    #endregion
                }

                #endregion

                process?.Invoke(false, "�ύ��������...", sendpeople);

                int count = UnitOfWork.SaveChanges();
                process?.Invoke(true, "�������...", sendpeople);
                return OperationHelper.ReturnOperationResult(count > 0, Operation.Add);
            },(ex)=> { process?.Invoke(true, "", sendpeople); return OperationHelper.ReturnOperationExceptionResult(ex, Operation.Add); });
        }

        /// <summary>
        /// ������ֵ�����������
        /// </summary>
        /// <param name="list"></param>
        /// <param name="minValue">ÿ����Сֵ</param>
        private List<int> ListRandom(List<int> list, int minValue = 1)
        {
            minValue = minValue < 1 ? 1 : minValue;
            Random rd = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item > minValue)
                {
                    var ind = rd.Next(0, list.Count);//���һ����������
                    var gval = rd.Next(0, item - minValue);//�������ӵ�ֵ
                    list[i] -= gval;
                    list[ind] += gval;
                }
            }
            return list;
        }

        public OperationResult Update(params SaleAutoGenDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SaleAutoGenRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public SaleAutoGenDto Edit(int Id)
        {
            var entity = _SaleAutoGenRepository.GetByKey(Id);
            Mapper.CreateMap<SaleAutoGen, SaleAutoGenDto>();
            var dto = Mapper.Map<SaleAutoGen, SaleAutoGenDto>(entity);
            return dto;
        }
    }
}

