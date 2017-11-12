using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 业务层接口
    /// </summary>
    public interface IApprovalContract : IDependency
    {
        #region 业务接口

        /// <summary>
        /// 获取数据
        /// </summary>
        IQueryable<Approval> Approvals { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params ApprovalDto[] dtos);
        
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

         
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="approvalDto">领域模型</param>
        /// <param name="productApprovalType">点赞类型</param>
        /// <returns></returns>
        OperationResult Delete(ApprovalDto approvalDto);

        

        /// <summary>
        /// 获取点赞集合
        /// </summary>
        /// <param name="singleProId">商品Id</param>
        /// <param name="productApprovalType">点赞类型</param>
        /// <returns></returns>
        IQueryable<Approval> GetList(int sourceId, CommentSourceFlag approvalSourceFlag);

        #endregion

 
    }
}
