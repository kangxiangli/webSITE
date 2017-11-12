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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MemberSignService : ServiceBase, IMemberSignContract
    {
        #region 声明数据层对象

        private readonly IRepository<Coupon, int> _couponRepository;

        private readonly IRepository<CouponItem, int> _couponItemRepository;

        private readonly IRepository<MemberSign, int> _memberSignRepository;

        private readonly IRepository<Member, int> _memberRepository;

        private readonly IRepository<SignRule, int> _signRuleRepository;

        private readonly IRepository<Prize, int> _prizeRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberSignService));

        public MemberSignService(IRepository<MemberSign, int> memberSignRepository,
            IRepository<SignRule, int> signRuleRepository,
            IRepository<Member, int> memberRepository,
            IRepository<Coupon, int> couponRepository,
            IRepository<CouponItem, int> couponItemRepository,
            IRepository<Prize, int> prizeRepository)
            : base(memberSignRepository.UnitOfWork)
        {
            _couponRepository = couponRepository;
            _couponItemRepository = couponItemRepository;
            _memberSignRepository = memberSignRepository;
            _signRuleRepository = signRuleRepository;
            _memberRepository = memberRepository;
            _prizeRepository = prizeRepository;
        }
        #endregion

        #region 查看数据
                
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MemberSign View(int Id)
        {
            var entity = _memberSignRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑数据对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MemberSignDto Edit(int Id)
        {
            var entity = _memberSignRepository.GetByKey(Id);
            Mapper.CreateMap<MemberSign, MemberSignDto>();
            var dto = Mapper.Map<MemberSign, MemberSignDto>(entity);
            return dto;
        }
        #endregion

        #region 获取优惠卷数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<MemberSign> MemberSigns { get { return _memberSignRepository.Entities; } }
        #endregion

        #region 按条件检查数据是否存在

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<MemberSign, bool>> predicate, int id = 0)
        {
            return _memberSignRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
                
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MemberSignDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                DateTime nowTime = DateTime.Now.Date;
                DateTime startWeek = nowTime.AddDays(1 - Convert.ToInt32(nowTime.DayOfWeek.ToString("d")));  //本周周一
                //DateTime endWeek = startWeek.AddDays(6);  //本周周日

                OperationResult oper = new OperationResult(OperationResultType.Success, "签到成功");
                IQueryable<MemberSign> listMemberSign = MemberSigns.Where(x=>x.IsDeleted==false && x.IsEnabled==true);                
                foreach (var dto in dtos)
                {
                    var count = listMemberSign.Where(x => x.SignTime >= nowTime && x.MemberId == dto.MemberId).Count();

                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Success, "已经签到");
                    }
                    else
                    {
                        var signCount = listMemberSign.Count(c => c.SignTime >= startWeek && c.MemberId == dto.MemberId);
                        if (signCount < 7)
                        {
                            ++signCount;
                            oper = CheckSignRule(signCount, dto);
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "本周已签到完成");
                        }
                    }
                }
                OperationResult result = _memberSignRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                     
                    return entity;
                });
                if (oper.ResultType!=OperationResultType.Success)
                {
                    return oper;
                }
                else
                {
                    int saveResult = UnitOfWork.SaveChanges();
                    return saveResult > 0 ? oper : new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 校验签到规则
        /// <summary>
        /// 校验签到规则
        /// </summary>
        /// <param name="month"></param>
        /// <param name="signCount"></param>
        /// <param name="dto"></param>
        private OperationResult CheckSignRule(int week, MemberSignDto dto)
        {
            OperationResult oper;
            List<SignRule> listSignRule = _signRuleRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            SignRule signRule = listSignRule.Where(x => x.Week == week).FirstOrDefault();
            if (signRule != null)
            {
                int prizeType = signRule.PrizeType;
                dto.PrizeType = prizeType;
                if (prizeType == (int)PrizeFlag.Score)
                {
                    int prizeId=signRule.PrizeId??0;                    
                    oper = GetScore(prizeId, dto);
                }
                else if(prizeType==(int)PrizeFlag.Coupon)
                {
                    int couponId = signRule.CouponId??0;
                    oper = GetCoupon(couponId, dto);
                }
                else
                {
                    int prizeId = signRule.PrizeId ?? 0;
                    oper = GetRes(prizeId, dto);
                }
                return oper;
            }
            else
            {
                dto.PrizeType = (int)PrizeFlag.None;
                oper = new OperationResult(OperationResultType.Success, "签到成功");
                return oper;
            }
        }
        #endregion

        #region 获取积分

        /// <summary>
        /// 会员签到提示语
        /// </summary>
        private string[] _SginMsgArray = { "积分丢在了来的路上", "你的积分被小花猫叼走了", "你的积分已捐赠给流浪儿童", "你的钱包已撑爆了", "行啦~你的钱已经够多了", "消费点积分再来吧", "先去买件衣服后再来试试" };

        /// <summary>
        /// 获取积分
        /// </summary>
        /// <param name="score"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        private OperationResult GetScore(int prizeId, MemberSignDto dto)
        {
            Member member = _memberRepository.Entities.Where(x => x.Id == dto.MemberId).FirstOrDefault();
            if (member == null)
            {
                return new OperationResult(OperationResultType.Error, "会员不存在");
            }
            else
            {
                Prize prize =  _prizeRepository.Entities.Where(x => x.Id == prizeId && x.IsDeleted==false && x.IsEnabled==true).FirstOrDefault();
                if (prize==null)
                {
                    return new OperationResult(OperationResultType.Error, "获取奖励积分失败，请稍后重试");
                }
                else
                {
                    string message = string.Empty;
                    if (member.Score < 100)
                    {
                        member.Score = member.Score + prize.Score;
                        member.UpdatedTime = DateTime.Now;
                        _memberRepository.Update(member);
                        message = "恭喜你获得" + prize.Score + "积分";
                    }
                    else
                    {
                        message = RandomHelper.random.NextItem(_SginMsgArray);
                    }

                    dto.PrizeId = prize.Id;
                    prize.ReceiveQuantity = prize.ReceiveQuantity + 1;
                    prize.UpdatedTime = DateTime.Now;
                    _prizeRepository.Update(prize);
                    
                    return new OperationResult(OperationResultType.Success, message, prize.RewardImagePath);
                }
            }
        }
        #endregion

        #region 获取优惠券
        /// <summary>
        /// 获取优惠券
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        private OperationResult GetCoupon(int couponId, MemberSignDto dto)
        {
            OperationResult oper;
            Coupon coupon = _couponRepository.Entities.Where(x => x.Id == couponId && x.IsDeleted==false && x.IsEnabled==true).FirstOrDefault();
            if (coupon==null)
            {
                oper=new OperationResult(OperationResultType.Error,"领取优惠卷失败");
                return oper;
            }
            else
            {
                List<CouponItem> listCoponItem = coupon.CouponItems.Where(x=>x.MemberId==null && x.IsUsed==false && x.IsDeleted==false && x.IsEnabled==true).ToList();
                if (listCoponItem.Count > 0)
                {
                    CouponItem couponItem = listCoponItem[0];
                    couponItem.MemberId = dto.MemberId;
                    couponItem.UpdatedTime = DateTime.Now;
                    dto.CouponId = couponItem.Id;
                    _couponItemRepository.Update(couponItem);
                    string message = "恭喜你获得优惠券一张";
                    return new OperationResult(OperationResultType.Success, message, couponItem.Coupon.CouponImagePath);
                }
                else 
                {
                    oper = new OperationResult(OperationResultType.Error, "优惠券已经领取完");
                    return oper;
                }
            }
        }
        #endregion

        #region 获取物品
        /// <summary>
        /// 获取物品
        /// </summary>
        /// <param name="prizeId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        private OperationResult GetRes(int prizeId, MemberSignDto dto)
        {
            Prize prize = _prizeRepository.Entities.Where(x => x.Id == prizeId && x.IsDeleted == false && x.IsEnabled == true).FirstOrDefault();
            if (prize == null)
            {
                return new OperationResult(OperationResultType.Error, "获取奖励物品失败，请稍后重试");
            }
            else
            {
                prize.ReceiveQuantity = prize.ReceiveQuantity + 1;
                dto.PrizeId = prize.Id;
                prize.UpdatedTime = DateTime.Now;                
                _prizeRepository.Update(prize);
                string message = "恭喜你获得" + prize.PrizeName ;
                return new OperationResult(OperationResultType.Success, message, prize.RewardImagePath);
            }      
        }
        #endregion

        #region 深拷贝优惠券详细信息
        /// <summary>
        /// 深拷贝优惠券详细信息
        /// </summary>
        /// <param name="couponItem"></param>
        /// <returns></returns>
        private MemberSign DeepClone(MemberSignDto dto)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, dto);
                stream.Seek(0, SeekOrigin.Begin);
                MemberSignDto entityDto =formatter.Deserialize(stream) as MemberSignDto;
                Mapper.CreateMap<MemberSignDto, MemberSign>();
                var entity = Mapper.Map<MemberSignDto, MemberSign>(entityDto);
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
        public OperationResult Update(params MemberSignDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled=true;
                IQueryable<MemberSign> listMemberSign = MemberSigns.Where(x=>x.IsDeleted==false && x.IsEnabled==true);                
                          
                OperationResult result = _memberSignRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
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
                var entities = _memberSignRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSignRepository.Update(entity);
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
                var entities = _memberSignRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSignRepository.Update(entity);
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
                OperationResult result = _memberSignRepository.Delete(ids);
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
                var entities = _memberSignRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSignRepository.Update(entity);
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
                var entities = _memberSignRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _memberSignRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion
    }
}
