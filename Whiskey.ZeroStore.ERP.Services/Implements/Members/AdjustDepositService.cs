using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AdjustDepositService : ServiceBase, IAdjustDepositContract
    {
        #region AdjustDepositService

        #region 声明数据层操作对象
        private readonly IRepository<AdjustDeposit, int> _adjustDepositRepository;
        private readonly IMemberDepositContract _memberDepositContract;
        private readonly IMemberConsumeContract _memberConsumeContract;
        private readonly INotificationContract _notificationContract;
        protected readonly ITemplateContract _templateContract;
        private readonly ISmsContract _smsContract;

        private readonly IRepository<Member, int> _memberRepository;
        public AdjustDepositService(
            IRepository<AdjustDeposit, int> adjustDepositRepository,
            IRepository<Member, int> memberRepository,
            INotificationContract _notificationContract,
            IMemberDepositContract memberDepositContract,
            ITemplateContract templateContract,
            ISmsContract _smsContract,
            IMemberConsumeContract memberConsumeContract)
            : base(adjustDepositRepository.UnitOfWork)
        {
            _adjustDepositRepository = adjustDepositRepository;
            _memberRepository = memberRepository;
            _memberDepositContract = memberDepositContract;
            _memberConsumeContract = memberConsumeContract;
            _templateContract = templateContract;
            this._notificationContract = _notificationContract;
            this._smsContract = _smsContract;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public AdjustDeposit View(int Id)
        {
            var entity = _adjustDepositRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public AdjustDepositDto Edit(int Id)
        {
            var entity = _adjustDepositRepository.GetByKey(Id);
            Mapper.CreateMap<AdjustDeposit, AdjustDepositDto>();
            var dto = Mapper.Map<AdjustDeposit, AdjustDepositDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<AdjustDeposit> AdjustDeposits { get { return _adjustDepositRepository.Entities; } }

        public object EntityContract { get; private set; }
        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<AdjustDeposit, bool>> predicate, int id = 0)
        {
            return _adjustDepositRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AdjustDepositDto[] dtos)
        {

            dtos.CheckNotNull("dtos");
            using (var transaction = _memberRepository.GetTransaction())
            {
                try
                {
                    List<int> listMemberId = dtos.Select(x => x.MemberId ?? 0).ToList();
                    List<Member> listMember = _memberRepository.Entities.Where(x => listMemberId.Contains(x.Id)).ToList();
                    OperationResult oper = new OperationResult(OperationResultType.Error);
                    foreach (AdjustDepositDto dto in dtos)
                    {
                        if (dto.VerifyType == (int)VerifyFlag.Pass)
                        {
                            Member member = listMember.FirstOrDefault(x => x.Id == dto.MemberId);
                            if (member == null)
                            {
                                oper.Message = "会员不存在";
                                return oper;
                            }
                            else
                            {
                                LogBalanceAndScoreChange(dto, member);

                                decimal currentBalance = member.Balance;
                                decimal currentScore = member.Score;
                                member.Balance = currentBalance + dto.Balance;
                                member.Score = currentScore + dto.Score;

                                if (member.Balance < 0)
                                {
                                    oper.ResultType = OperationResultType.Error;
                                    oper.Message = "修改后储值为负值";
                                    return oper;
                                }
                                if (member.Score < 0)
                                {
                                    oper.ResultType = OperationResultType.Error;
                                    oper.Message = "修改后积分为负值";
                                    return oper;
                                }

                                member.UpdatedTime = DateTime.Now;
                                _memberRepository.Update(member);
                            }
                        }
                        else if (dto.VerifyType == (int)VerifyFlag.Verifing)
                        {
                            IQueryable<AdjustDeposit> listAd = _adjustDepositRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == dto.MemberId && x.VerifyType == (int)VerifyFlag.Verifing);
                            if (listAd.Count() > 0)
                            {
                                oper.Message = "该会员的已经提交审核，审核通过后才能重试";
                                return oper;
                            }
                        }
                    }
                    OperationResult result = _adjustDepositRepository.Insert(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                    int count = UnitOfWork.SaveChanges();
                    transaction.Commit();
                    return new OperationResult(OperationResultType.Success, "添加成功");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
                }

            }


        }

        /// <summary>
        /// 记录储值积分变动记录
        /// </summary>
        private void LogBalanceAndScoreChange(AdjustDepositDto dto, Member member)
        {
            if (dto.Balance == 0 && dto.Score == 0)
            {
                throw new Exception("变动信息有误");
            }
            if (dto.Balance > 0)
            {
                // 记录到储值充值记录
                 _memberDepositContract.LogBalanceWhenAdjustDeposit(member, dto.Balance, dto.Notes);
            }
            else if (dto.Balance < 0)
            {
                // 记录到储值消费记录
                var balanceConsume = Math.Abs(dto.Balance);
                _memberConsumeContract.LogBalanceWhenAdjustDeposit(dto.MemberId.Value, balanceConsume);
            }
            if (dto.Score > 0)
            {
                // 记录到积分充值记录
                 _memberDepositContract.LogScoreWhenAdjustDeposit(member, dto.Score, dto.Notes);
            }
            else if (dto.Score < 0)
            {
                // 记录到积分消费记录
                var scoreConsume = Math.Abs(dto.Score);
                _memberConsumeContract.LogScoreWhenAdjustDeposit(dto.MemberId.Value, scoreConsume);
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params AdjustDepositDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _adjustDepositRepository.Update(dtos,
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
                var entities = _adjustDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _adjustDepositRepository.Update(entity);
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
                var entities = _adjustDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _adjustDepositRepository.Update(entity);
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
                OperationResult result = _adjustDepositRepository.Delete(ids);
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
                var entities = _adjustDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _adjustDepositRepository.Update(entity);
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
                var entities = _adjustDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _adjustDepositRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 审核数据
        public OperationResult Verify(bool contentToTitle, Action<List<int>> sendNotificationAction, params int[] Ids)
        {
            using (var transaction = _adjustDepositRepository.GetTransaction())
            {

                try
                {
                    Ids.CheckNotNull("Ids");
                    OperationResult oper = new OperationResult(OperationResultType.Error);
                    var entitiesToVerify = _adjustDepositRepository.Entities.Where(m => Ids.Contains(m.Id)).Where(w => w.VerifyType == (int)VerifyFlag.Verifing);
                    List<int> memberIds = entitiesToVerify.Select(x => x.MemberId ?? 0).ToList();
                    List<Member> listMember = _memberRepository.Entities.Where(x => memberIds.Contains(x.Id)).ToList();
                    int sucCount = 0;
                    var adjustToUpdateList = new List<AdjustDeposit>();
                    var memberToUpdateList = new List<Member>();
                    var listApplicantIds = new List<int>();//需要通知的申请人
                    foreach (var entity in entitiesToVerify)
                    {
                        if (entity.VerifyType == (int)VerifyFlag.Pass)
                        {
                            oper.Message = "已经审核通过";
                            return oper;
                        }
                        else
                        {
                            ++sucCount;
                            Member member = listMember.FirstOrDefault(x => x.Id == entity.MemberId);
                            var dto = Mapper.Map<AdjustDepositDto>(entity);
                            LogBalanceAndScoreChange(dto, member);

                            // 更新entity
                            entity.VerifyType = (int)VerifyFlag.Pass;
                            entity.UpdatedTime = DateTime.Now;
                            entity.OperatorId = AuthorityHelper.OperatorId;
                            entity.ReviewersId = AuthorityHelper.OperatorId;
                            var hasUpdate = _adjustDepositRepository.Update(entity);
                            if (hasUpdate <= 0)
                            {
                                throw new Exception("调整记录更新失败");
                            }

                            decimal currentBalance = member.Balance;
                            decimal currentScore = member.Score;

                            member.Balance = currentBalance + entity.Balance;
                            member.Score = currentScore + entity.Score;

                            if (member.Balance < 0)
                            {
                                oper.ResultType = OperationResultType.Error;
                                oper.Message = "修改后储值为负值";
                                return oper;
                            }
                            if (member.Score < 0)
                            {
                                oper.ResultType = OperationResultType.Error;
                                oper.Message = "修改后积分为负值";
                                return oper;
                            }
                            member.UpdatedTime = DateTime.Now;

                            // 更新member
                            var hasUpdateMember = _memberRepository.Update(member);
                            if (hasUpdateMember <= 0)
                            {
                                throw new Exception("调整记录更新失败");
                            }
                            if (entity.ApplicantId.HasValue)
                            {
                                listApplicantIds.Add(entity.ApplicantId.Value);
                            }
                        }
                            

                        var Verifymessage = ((VerifyFlag)Enum.Parse(typeof(VerifyFlag), entity.VerifyType + ""));
                        var notification =  EnumHelper.GetValue<string>(Verifymessage);

                        var dic = new Dictionary<string, object>();
                        dic.Add("storeId", entity.Member?.StoreId);
                        dic.Add("storeName", entity.Member?.Store?.StoreName ?? string.Empty);
                        dic.Add("storePhone", entity.Member?.Store?.StorePhoto ?? string.Empty);
                        dic.Add("storeAddress", entity.Member?.Store?.Address ?? string.Empty);
                        dic.Add("memberId", entity.MemberId);
                        dic.Add("sendTime", DateTime.Now);
                        dic.Add("memberName", entity.Member.MemberName ?? string.Empty);
                        dic.Add("memberPhone", entity.Member.MobilePhone ?? string.Empty);
                        dic.Add("balance", entity.Balance);
                        dic.Add("score", entity.Score);
                        dic.Add("VerifyType", notification);
                        dic.Add("checktime", DateTime.Now);
                        var template = _templateContract.Templates.FirstOrDefault(i => i.templateNotification.NotifciationType == TemplateNotificationType.Changecheck && i.IsDefault);
                        if (template != null)
                        {
                            var sto = new NotificationDto();
                            sto.Title = template.TemplateName ?? "储值积分调整审核通知";
                            sto.Description = NVelocityHelper.Generate(template.TemplateHtml, dic, "_noti_chuzhijifentiaozheng_shenhe_");
                            sto.NoticeTargetType = (int)NoticeTargetFlag.Admin;
                            sto.SendTime = DateTime.Now;
                            sto.AdministratorIds = listApplicantIds;
                            sto.NoticeType = (int)NoticeFlag.Immediate;
                            _notificationContract.Insert(sendNotificationAction, sto);
                        }

                        if (Verifymessage == VerifyFlag.Pass && entity.Member != null)
                        {
                            var tempDict = new Dictionary<string, object> {
                                {"storeName",    entity.Member?.Store?.StoreName ?? string.Empty},
                                {"memberName",   entity.Member.MemberName ?? string.Empty},
                                {"DepositPrice", entity.Balance},
                                {"DepositScore", entity.Score},
                                {"$DepositTime", DateTime.Now}
                            };
                            _smsContract.SendSms(entity.Member.MobilePhone, TemplateNotificationType.MemberDeposit, tempDict);
                        }
                    }

                    transaction.Commit();
                    return new OperationResult(OperationResultType.Success, "审核成功！", new { allCount = Ids.Count(), sucCount = sucCount, listApplicantIds= listApplicantIds });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "审核失败！错误如下：" + ex.Message);
                }
            }
        }
        #endregion

        #endregion
    }
}
