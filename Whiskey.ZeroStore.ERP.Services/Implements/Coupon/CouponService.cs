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
using Whiskey.Utility.Logging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class CouponService : ServiceBase, ICouponContract
    {
        #region 声明数据层对象

        private readonly IRepository<Coupon, int> _couponRepository;

        private readonly IRepository<CouponItem, int> _couponItemRepository;
        private readonly ITemplateContract _templateContract;
        private readonly IMemberContract _memberContract;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponService));

        public CouponService(IRepository<Coupon, int> couponRepository,
            ITemplateContract _templateContract,
            IMemberContract _memberContract,
            IRepository<CouponItem, int> couponItemRepository)
            : base(couponRepository.UnitOfWork)
        {
            _couponRepository = couponRepository;
            _couponItemRepository = couponItemRepository;
            this._templateContract = _templateContract;
            this._memberContract = _memberContract;
        }
        #endregion

        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 查看数据
                
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Coupon View(int Id)
        {
            var entity = _couponRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑数据对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CouponDto Edit(int Id)
        {
            var entity = _couponRepository.GetByKey(Id);
            Mapper.CreateMap<Coupon, CouponDto>();
            var dto = Mapper.Map<Coupon, CouponDto>(entity);
            dto.PartnerName = entity.PartnerId == null ? string.Empty : entity.Partner.PartnerName;
            return dto;
        }
        #endregion

        #region 获取优惠卷数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Coupon> Coupons { get { return _couponRepository.Entities; } }
        #endregion

        #region 获取优惠卷详情数据集
        public IQueryable<CouponItem> CouponItems { get { return _couponItemRepository.Entities;} }
        #endregion

        #region 按条件检查数据是否存在

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Coupon, bool>> predicate, int id = 0)
        {
            return _couponRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
                
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params CouponDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                string link = strWebUrl+"/Coupon/Get/?num=";
                string strNum = string.Empty;
                IQueryable<Coupon> listCoupon = Coupons.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                foreach (var dto in dtos)
                {
                    int count = listCoupon.Where(x => x.CouponName == dto.CouponName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "优惠卷名称已经存在");
                    }
                    List<CouponItem> listCouponItem = new List<CouponItem>();
                    bool res = CreateCouponItem(dto.Quantity,out listCouponItem);
                    if(res==true)
                    {
                        dto.CouponItems = listCouponItem;    
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "生成优惠券失败");
                    }
                    while (true)
                    {
                        strNum = RandomHelper.GetRandomCode(10);
                        int index = listCoupon.Where(x => x.CouponNum == strNum).Count();
                        if (index > 0)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    dto.CouponNum = strNum;
                    link = link + strNum;
                    //string strPath = ImageHelper.CreateQRCode(link, strNum);
                    //if (string.IsNullOrEmpty(strPath))
                    //{
                    //    return new OperationResult(OperationResultType.Error, "生成二维码失败");
                    //}
                    //else
                    //{
                    //    dto.CouponQRCodePath = strPath;
                    //}
                    }
                OperationResult result = _couponRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.CouponItems = dto.CouponItems;
                    return entity;
                });
                UnitOfWork.SaveChanges();
                return new OperationResult(OperationResultType.Success);
                //return result;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 生成优惠券详细信息
        /// <summary>
        /// 生成优惠券详细信息
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private bool CreateCouponItem(int quantity, out List<CouponItem> listCouponItem)
        {
            try
            {
                CouponItemDto couponItem = new CouponItemDto();
                listCouponItem = new List<CouponItem>();
                StringBuilder sbNum = new StringBuilder();
                //获取所有优惠券的数据
                List<string> listNum = _couponItemRepository.Entities.Select(x => x.CouponNumber).ToList();
                //条形码保存路径
                string savePath = ConfigurationHelper.GetAppSetting("CouponBarcodePath");
                StringBuilder sbPath = new StringBuilder();
                for (int i = 0; i < quantity; i++)
                {
                    CouponItem couItem = DeepClone(couponItem);
                    //生成唯一编码
                    while (true)
                    {
                        sbNum.Append(RandomHelper.GetRandomCode(13));
                        int count = listNum.Where(x => x == sbNum.ToString()).Count();
                        int index = listCouponItem.Where(x => x.CouponNumber == sbNum.ToString()).Count();
                        if (count > 0 || index > 0)
                        {
                            sbNum.Clear();
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    couItem.CouponNumber = sbNum.ToString();
                    //生成二维码
                    sbPath.Append(savePath + sbNum.ToString() + ".jpg");
                    //bool res = ImageHelper.CreateBarcode(sbPath.ToString(), sbNum.ToString(), 200, 70);
                    //if (res)
                    //{
                    //    couItem.BarcodePath = sbPath.ToString();
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                    listCouponItem.Add(couItem);
                    sbNum.Clear();
                    sbPath.Clear();
                }
                return true;
            }
            catch (Exception)
            {
                listCouponItem = null;
                return false;
            }
            
        }
        
        #endregion

        #region 深拷贝优惠券详细信息
        /// <summary>
        /// 深拷贝优惠券详细信息
        /// </summary>
        /// <param name="couponItem"></param>
        /// <returns></returns>
        private CouponItem DeepClone(CouponItemDto dto)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, dto);
                stream.Seek(0, SeekOrigin.Begin);
                CouponItemDto entityDto =formatter.Deserialize(stream) as CouponItemDto;
                Mapper.CreateMap<CouponItemDto, CouponItem>();
                var entity = Mapper.Map<CouponItemDto, CouponItem>(entityDto);
                return entity;
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params CouponDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled=true;
                IQueryable<Coupon> listCoupon = Coupons.Where(x=>x.IsDeleted==false && x.IsEnabled==true);                
                foreach (var dto in dtos)
                {
                    int count = listCoupon.Where(x => x.CouponName == dto.CouponName && x.Id != dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "修改失败,名称已经存在！");
                    }
                    Coupon coupon = listCoupon.Where(x=>x.Id==dto.Id).FirstOrDefault();
                    if (coupon==null)
                    {
                        return new OperationResult(OperationResultType.Error, "修改失败,优惠券不存在！");
                    }
                    else
                    {
                        dto.CouponNum = coupon.CouponNum;
                        OperationResult oper = UpdateCouponItem(coupon, dto.Quantity);
                        if (oper.ResultType != OperationResultType.Success)
                        {
                            return oper;
                        }
                        else
                        {
                            dto.CouponItems = oper.Data as ICollection<CouponItem>;
                        }
                        //dto.CouponQRCodePath = coupon.CouponQRCodePath;
                    }
                    
                }                 
                OperationResult result = _couponRepository.Update(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                int resCount =UnitOfWork.SaveChanges();
                return resCount > 0 ? new OperationResult(OperationResultType.Success, "修改成功") : new OperationResult(OperationResultType.Error, "修改失败！");

            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "修改失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新优惠卷详细信息
        /// <summary>
        /// 更新优惠卷详细信息
        /// </summary>
        /// <param name="coupon">优惠卷</param>
         /// <param name="num">要更新的数量</param>
        /// <returns></returns>
        private OperationResult UpdateCouponItem(Coupon coupon,int num)
        {
            try
            {
                List<CouponItem> listCouponItem = new List<CouponItem>();
                int quantity = num - coupon.Quantity;
                if (quantity < 0) //减少优惠券数量
                {
                    listCouponItem = coupon.CouponItems.Where(x => x.IsUsed == false && x.MemberId == null).ToList();
                    quantity=quantity *(-1);
                    int currCount = listCouponItem.Count() - quantity;
                    if (currCount >= 0)
                    {                        
                        for (int i = 0; i < quantity; i++)
                        {
                            _couponItemRepository.Delete(listCouponItem[0]);
                            listCouponItem.RemoveAt(0);
                        }
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "删除优惠券的数量超过了剩余量");
                    }
                }
                else if (quantity > 0) //添加优惠数量
                {
                    bool res = CreateCouponItem(quantity, out listCouponItem);
                    if (res)
                    {
                        foreach (var couponItem in listCouponItem)
                        {
                            couponItem.CouponId = coupon.Id;
                            //_couponItemRepository.Insert(couponItem);
                        }
                        listCouponItem.AddRange(coupon.CouponItems);
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "增加优惠卷数量失败");
                    }
                }
                else //数量不变
                {
                    listCouponItem = coupon.CouponItems.ToList();
                }
                return new OperationResult(OperationResultType.Success, "添加成功", listCouponItem);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "修改优惠卷失败");
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
                var entities = _couponRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _couponRepository.Update(entity);
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
                var entities = _couponRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _couponRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
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
                OperationResult result = _couponRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
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
                var entities = _couponRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _couponRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
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
                var entities = _couponRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _couponRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 发送优惠卷
        /// <summary>
        /// 发送优惠卷
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="listMemberId"></param>
        /// <returns></returns>
        public OperationResult Send(int couponId, List<int> listMemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, string.Empty);
            try
            {
                Coupon coupon = Coupons.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == couponId).FirstOrDefault();
                if (coupon==null)
                {
                    oper.ResultType = OperationResultType.Error;
                    oper.Message = "发送失败，发送的优惠卷不存在";
                    return oper;
                }
                else
                {
                    //可发送的优惠卷数量
                    List<CouponItem> listCouponItem= coupon.CouponItems.Where(x => x.IsUsed == false && x.MemberId == null).ToList();
                    int num= listCouponItem.Where(x => listMemberId.Contains(x.MemberId ?? 0)).Count();
                    if (num>0)
                    {
                        oper.ResultType = OperationResultType.Error;
                        oper.Message = "发送失败，部门会员已经发送过同样的优惠卷";
                        return oper;
                    }
                    int quantity = listCouponItem.Count();
                    int memberQuantity = listMemberId.Count();
                    int resCount = memberQuantity - quantity;
                    List<CouponItem> listCouItem = new List<CouponItem>();
                    if (resCount>0)
                    {
                        oper.ResultType = OperationResultType.Error;
                        oper.Message = "发送失败，发送会员的数量超过了优惠卷的数量";
                        return oper;
                    }
                    else
                    {
                        for (int i = 0; i < memberQuantity; i++)
                        {
                            int memberId = listMemberId[i];
                            listCouponItem[i].MemberId = memberId;
                            listCouItem.Add(listCouponItem[i]);
                        }
                    }
                    UnitOfWork.TransactionEnabled = true;
                    foreach (var item in listCouItem)
                    {
                        item.UpdatedTime = DateTime.Now;
                        item.OperatorId = AuthorityHelper.OperatorId;
                        _couponItemRepository.Update(item);
                    }
                    int index= UnitOfWork.SaveChanges();
                    if (index>0)
                    {
                        oper.ResultType = OperationResultType.Success;
                        oper.Message = "发送成功";

                        OperationHelper.Try(() =>
                        {
                            var modtemp = _templateContract.GetNotificationTemplate(TemplateNotificationType.MemberGetCoupons);
                            if (modtemp.IsNotNull() && modtemp.TemplateHtml.IsNotNullAndEmpty())
                            {
                                var listmembers = _memberContract.Members.Where(w => w.IsEnabled && !w.IsDeleted && listMemberId.Contains(w.Id)).Select(s => new
                                {
                                    s.Id,
                                    s.MemberName,
                                    s.MobilePhone,
                                    s.CardNumber,
                                }).ToList();

                                var startdate = coupon.StartDate.ToString("yyyy年MM月dd日");
                                var enddate = coupon.EndDate.ToString("yyyy年MM月dd日");

                                foreach (var item in listmembers)
                                {
                                    Dictionary<string, object> dic = new Dictionary<string, object>();
                                    dic.Add("MemberName", item.MemberName);
                                    dic.Add("MobilePhone", item.MobilePhone);
                                    dic.Add("CardNumber", item.CardNumber);
                                    dic.Add("CouponName", coupon.CouponName);
                                    dic.Add("CouponNum", coupon.CouponNum);
                                    dic.Add("CouponPrice", coupon.CouponPrice);
                                    dic.Add("StartDate", startdate);
                                    dic.Add("EndDate", enddate);

                                    var content = NVelocityHelper.Generate(modtemp.TemplateHtml, dic);
                                    _memberContract.SendAppNotification(modtemp.TemplateName, content, JPushFlag.我的钱包, item.Id);
                                }
                            }
                        });

                        return oper;
                    }
                    else
                    {
                        oper.ResultType = OperationResultType.Error;
                        oper.Message = "发送失败";
                        return oper;
                    }                    
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.ResultType = OperationResultType.Error;
                oper.Message = "发送失败，请稍后重试";
                return oper;
            }
        }
        #endregion

        #region 扫描二维码获取优惠券
        /// <summary>
        /// 扫描二维码获取优惠券
        /// </summary>
        /// <param name="strNum"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public OperationResult Get(string strNum, int memberId)
        {
            try
            {
                Coupon coupon = _couponRepository.Entities.Where(x => x.CouponNum == strNum).FirstOrDefault();
                CouponItem couponItem = coupon.CouponItems.FirstOrDefault(x => x.MemberId == null && x.IsUsed == false);
                if (couponItem.IsNotNull())
                {
                    var isExist = coupon.CouponItems.Any(x => x.MemberId == memberId);
                    if (isExist)
                    {
                        return new OperationResult(OperationResultType.Error, "您已经领取了该优惠卷");
                    }
                    else
                    {
                        couponItem.MemberId = memberId;
                        couponItem.UpdatedTime = DateTime.Now;
                        int index = _couponItemRepository.Update(couponItem);
                        if (index > 0)
                        {
                            return new OperationResult(OperationResultType.Success, "领取成功");
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "领取失败");
                        }
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "优惠券已经发送完");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            }
        }
        #endregion

        /// <summary>
        /// 将优惠券设为已使用状态
        /// </summary>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        public OperationResult SetCouponItemUsed(string couponNum)
        {
            try
            {
                var entity = _couponItemRepository.Entities.FirstOrDefault(i => i.CouponNumber == couponNum);
                entity.IsUsed = true;
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "操作成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "操作失败！错误如下：" + ex.Message);
            }
           

        }

        /// <summary>
        /// 将优惠券设为已使用状态
        /// </summary>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        public OperationResult SetCouponItemUnUsed(string couponNum)
        {
            try
            {
                var entity = _couponItemRepository.Entities.FirstOrDefault(i => i.CouponNumber == couponNum);
                entity.IsUsed = false;
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "操作成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "操作失败！错误如下：" + ex.Message);
            }


        }


        /// <summary>
        /// 使用优惠券
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public OperationResult Use(string Number, int MemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍候访问");
            CouponItem item = _couponItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                .FirstOrDefault(x => x.CouponNumber == Number && x.MemberId == MemberId);
            if (item!=null)
            {
                if (item.IsUsed==true)
                {
                    oper.Message = "优惠券已经使用过了";
                }
                else
                {
                    Coupon coupon = item.Coupon;
                    item.UpdatedTime = DateTime.Now;
                    //标记是否可以使用优惠券
                    bool isChange = false;
                    if (coupon.IsForever == true)
                    {
                        isChange = true;
                    }
                    else
                    {
                        DateTime stratDate = coupon.StartDate;
                        DateTime endDate = coupon.EndDate;
                        DateTime currentDate = DateTime.Now;
                        if (currentDate.CompareTo(stratDate)>=0 &&currentDate.CompareTo(endDate)<=0)
                        {
                            isChange = true;
                        }
                    }
                    if (isChange==true)
                    {
                        int resultCount = _couponItemRepository.Update(item);
                        if (resultCount>0)
                        {
                            oper.ResultType = OperationResultType.Success;
                        }
                    }
                }                               
            }
            return oper;
        }
    }
}
