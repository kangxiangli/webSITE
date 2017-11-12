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
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment; 

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ApprovalService : ServiceBase,IApprovalContract
    {
        #region 声明数据层对象

        private readonly IRepository<Approval, int> _ApprovalRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(ApprovalService));

        public ApprovalService(IRepository<Approval, int> ApprovalRepository)
            : base(ApprovalRepository.UnitOfWork)
        {
            _ApprovalRepository = ApprovalRepository;
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Approval> Approvals { get { return _ApprovalRepository.Entities; } }

        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="singleProId"></param>
        /// <returns></returns>
        public IQueryable<Approval> GetList(int sourceId, CommentSourceFlag approvalSourceFlag)
        {
            var entityList = Approvals.Where(x => x.SourceId == sourceId && x.ApprovalSource == (int)approvalSourceFlag);
            return entityList;
        }

        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public OperationResult Insert(params  ApprovalDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                for (int i = 0; i < dtos.Length; i++)
                {
                    int memberId = dtos[i].MemberId;
                    int approvalSource = dtos[i].ApprovalSource;
                    int sourceId = dtos[i].SourceId;
                    int count = Approvals.Where(x => x.MemberId == memberId && x.ApprovalSource == approvalSource && x.SourceId == sourceId).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "您已经赞过了！");
                    }
                }
                OperationResult result = _ApprovalRepository.Insert(dtos,
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
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器正在升级维护中，请稍后重试！");
            }
        }
        #endregion

        #region 删除数据
                
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _ApprovalRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="approvalDto"></param>
        /// <returns></returns>
        public OperationResult Delete(ApprovalDto approvalDto)
        {
            try
            {  
                
                approvalDto.CheckNotNull("ids");
                var app = _ApprovalRepository.Entities.Where(x => x.SourceId == approvalDto.SourceId && x.MemberId == approvalDto.MemberId && x.ApprovalSource==approvalDto.ApprovalSource).FirstOrDefault();
                int count = _ApprovalRepository.Delete(app.Id);
                if (count>0)
                {
                    return new OperationResult(OperationResultType.Success, "取赞成功！");
                }
                else
                {
                    return new OperationResult(OperationResultType.Error, "取赞失败！");
                }
 
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器正在维护升级中，请稍后重试！");
            }
        }
        #endregion

     
        
    }
}
